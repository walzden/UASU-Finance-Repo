using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using UASU_VoucherApprovals.Data;
using UASU_VoucherApprovals.Services;

var builder = WebApplication.CreateBuilder(args);

// No-op unless the published app is actually launched by the Windows Service
// Control Manager (see README: `sc.exe create`) - lets `dotnet run` keep
// working exactly as before for local development.
builder.Host.UseWindowsService();

builder.Services.AddRazorPages();

builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IDebtService, DebtService>();
builder.Services.AddScoped<IBankingService, BankingService>();
builder.Services.AddScoped<IReferenceDataService, ReferenceDataService>();
builder.Services.AddScoped<ICertificationService, CertificationService>();
builder.Services.AddScoped<IBudgetPlanningService, BudgetPlanningService>();
builder.Services.AddScoped<IMemberRegisterService, MemberRegisterService>();
builder.Services.AddScoped<IHonorariaService, HonorariaService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        // Idle timeout, not a hard session cap: SlidingExpiration renews the
        // cookie on activity, so this only logs someone out after 30 minutes
        // with no requests at all - see wwwroot/js/idle-timeout.js for the
        // client-side warning shown before that happens.
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        // Set to Always once the app is served over HTTPS in production
        // (Cloudflare Tunnel / your reverse proxy terminates TLS).
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
    });

builder.Services.AddAuthorization(options =>
{
    // Chairman/Chapter Secretary: approve vouchers, view reports - nothing else.
    options.AddPolicy("Approvers", policy => policy.RequireRole("Chairman", "Chapter Secretary"));

    // Treasurer/Deputy Treasurer: everything that creates or records
    // data (vouchers, debts, payments, acknowledgements, new logins).
    // Deliberately does NOT include approval - trg_Voucher_Approvals_
    // Validate in the database already rejects an approval from anyone
    // outside Chairman/Chapter Secretary, so even if this policy were
    // misapplied somewhere, the separation of duties still holds at
    // the database layer.
    options.AddPolicy("TreasuryAdmin", policy => policy.RequireRole("Treasurer", "Deputy Treasurer"));
});

var app = builder.Build();

// Cloudflare Tunnel (and any reverse proxy) terminates HTTPS at its edge
// and talks to Kestrel over plain HTTP on localhost - without this, the
// app sees every request as HTTP and CookieSecurePolicy.Always below
// would refuse to ever issue the login cookie in production. The default
// KnownProxies/KnownNetworks (loopback only) are enough here since
// cloudflared connects to the app via localhost on the same machine.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Catches every request from a signed-in user still mid-onboarding -
// not just the moment right after login. Without this, someone could
// sidestep the redirect by typing a different URL straight into the
// address bar before verifying their email or changing their password.
// Email verification comes first (SQL/025) - no point letting someone
// set a real password on an account seeded with a dummy address.
app.Use(async (context, next) =>
{
    var user = context.User;
    if (user.Identity?.IsAuthenticated == true)
    {
        var mustChangePassword = user.FindFirst("MustChangePassword")?.Value == "true";
        var emailVerified = user.FindFirst("EmailVerified")?.Value == "true";
        var path = context.Request.Path;

        if (mustChangePassword && !emailVerified)
        {
            var allowed = path.StartsWithSegments("/Account/VerifyEmail") ||
                          path.StartsWithSegments("/Account/Logout");
            if (!allowed)
            {
                context.Response.Redirect("/Account/VerifyEmail");
                return;
            }
        }
        else if (mustChangePassword)
        {
            var allowed = path.StartsWithSegments("/Account/ChangePassword") ||
                          path.StartsWithSegments("/Account/Logout");
            if (!allowed)
            {
                context.Response.Redirect("/Account/ChangePassword");
                return;
            }
        }
    }

    await next();
});

app.MapRazorPages();

app.Run();
