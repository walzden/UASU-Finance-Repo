# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

There is no `.sln` file and no test project — build/run directly against the web project.

```bash
dotnet restore
dotnet build
dotnet run
```

Visit `https://localhost:5001` (or whatever port `dotnet run` prints; see `Properties/launchSettings.json`).

`Tools/HashPasswordCli` is a separate console project used only to hash a password for manually seeding the first login (see Setup below). It's excluded from the main project's compile (`UASU_VoucherApprovals.csproj` has `<Compile Remove="Tools/**/*.cs" />`) because both projects would otherwise conflict on having their own `Program.cs` entry point:

```bash
cd Tools/HashPasswordCli
dotnet run
```

No test suite, linter, or formatter is configured in this repo.

## Architecture

ASP.NET Core 8 Razor Pages app, backed by SQL Server (`UASU_Finance_01`/`UASU_Finance_Web`) via Dapper — no EF Core, no ORM migrations. This app is the only thing that talks to SQL Server; nothing outside it ever sees a connection string.

```
Browser --HTTPS--> Razor Pages app
                      |-- cookie auth + role claims
                      |-- Services/ (Dapper, raw SQL)
                      v
                  SQL Server (UASU_Finance_01 / UASU_Finance_Web)
                      |-- same role rules re-checked by triggers
```

### Layers

- **`Data/DbConnectionFactory.cs`** — the only place a `SqlConnection` is constructed, from the `UasuFinance` connection string in `appsettings.json`. Services open a connection per call and let `using`/Dapper dispose it.
- **`Services/`** — one service per domain area (`AuthService`, `VoucherService`, `ReportService`, `DebtService`, `BankingService`), each an interface + implementation registered as Scoped in `Program.cs`. All data access is raw SQL strings via Dapper (`QueryAsync`/`ExecuteAsync`/`ExecuteScalarAsync`), not a repository/unit-of-work abstraction. IDs (`Voucher_ID`, `Payment_ID`, `Debt_ID`, `Withdrawal_ID`, `Charge_ID`) are server-generated in SQL via sequence-backed `DEFAULT` expressions, retrieved with `OUTPUT INSERTED.<col>` rather than a follow-up query.
- **`Models/`** — plain request/response shapes and Razor Page `InputModel`s with `[Required]`/`[Range]`/`[MaxLength]` data annotations. Validation here is a UX nicety (friendly error before submit) that mirrors, but does not replace, constraints enforced in SQL.
- **`Pages/`** — one folder per feature area, each `.cshtml` + `.cshtml.cs` code-behind `PageModel`. Handlers follow the pattern: call into a service, catch `SqlException` from a trigger and surface `ex.Message` back to the user as a status banner (see `Pages/Approvals/Index.cshtml.cs`), then re-render.

### Authorization — enforced twice on purpose

Two ASP.NET Core policies, registered in `Program.cs`, gate what each of the four officer roles can reach:

- **`Approvers`** (Chairman, Chapter Secretary) — the Approvals page only. They can also read Reports (open to any logged-in user) but have no route to create a voucher, debt, payment, acknowledgement, or login.
- **`TreasuryAdmin`** (Treasurer, Deputy Treasurer) — every creation/admin page (New Voucher, New Debt, Record Payment, Acknowledge Receipt, Add Logins). Deliberately excludes the Approvals page, so Treasurer/Deputy Treasurer can prepare and pay but not approve their own work.

The nav bar/home page hiding links a role can't use is a convenience, not the security boundary — the `[Authorize(Policy = ...)]` attribute on each `PageModel` is what actually enforces it.

**The same separation-of-duties rule is re-checked independently at the database layer** by triggers (`trg_Voucher_Approvals_Validate`, `trg_PaymentAllocations_RequireApproval`), so it holds even if a bug in the app layer, or some other future caller, tries to write to the database directly. When adding a new mutating operation, expect a matching trigger may reject it with a `SqlException` — catch it and surface `ex.Message` rather than letting it raise as an unhandled error, matching the existing pattern in every `OnPost*` handler.

A global middleware in `Program.cs` (after `UseAuthorization`, before `MapRazorPages`) force-redirects any authenticated request to `/Account/ChangePassword` while the `MustChangePassword` claim is `true`, except that page and `/Account/Logout` — this runs on every request, not just immediately after login, so a direct URL can't bypass it.

### Data model touchpoints worth knowing before changing a Service

- `VoucherPaymentEligible`, `DebtSummary`, `BankReconciliation`, and `PaymentsAwaitingAcknowledgement` are SQL views that services deliberately read from instead of recomputing the same logic in C# — e.g. `GetFullyApprovedUnpaidVouchersAsync` reads `VoucherPaymentEligible` so its results can never drift from what `trg_PaymentAllocations_RequireApproval` actually allows. Prefer extending a view over duplicating its logic in a new query.
- `Vouchers.Is_Legacy` marks vouchers paid before this approval workflow existed; they're excluded from approval queues and treated as pre-approved for payment eligibility. Never set it from application code — it's only set by the one-time backfill in `SQL/002_LegacyVoucherExemption.sql`.
- Creating a voucher against an existing debt (`VoucherInputModel.Debt_Link` set) makes `VoucherService.CreateVoucherAsync` override the submitted payee fields with the debt's own creditor — the debt is authoritative over whatever the form said.
- `VoucherService.RecordPaymentAsync(PaymentInputModel)` settles one or more approved vouchers with a single payment — `PaymentInputModel.Voucher_IDs` is a list, each allocated in full (its own `Amount`, looked up server-side) — and wraps the `Payments` insert, one `PaymentAllocations` insert per voucher, and (if a withdrawal is linked) the `WithdrawalPayments` insert in one real `SqlTransaction`, so a failure partway through rolls back everything rather than leaving a partially-allocated payment.
- The `SQL/` folder is an ordered set of migration scripts (`001`...`008`), applied manually against SQL Server — there's no migration runner. Read each script's own header comments before running it; several are conditional (e.g. `002` only applies if legacy pre-workflow vouchers exist) or require running sections in a specific order (`004`).

### Login bootstrapping

`Pages/Account/SeedDefaultLogins` is a permanent (not one-time) in-app tool: signed-in users can create logins for the current Chairman/Chapter Secretary/Treasurer/Deputy Treasurer from `Ref_Officials`, with a shared default password (`MustChangePassword = 1`, forced change on first login). It never touches an existing login. For the very first account in a brand-new database, or a role outside that fixed list, use `Tools/HashPasswordCli` to hash a password by hand and `INSERT` it directly.
