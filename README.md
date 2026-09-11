# UASU Voucher Approvals

ASP.NET Core (Razor Pages) front end for the UASU_Finance_01 database: sign in as
an officer, approve or reject vouchers, raise new vouchers, record payments, and
log payee acknowledgements of receipt.

## Architecture

```
Browser  --HTTPS-->  ASP.NET Core app (Razor Pages)
                         |-- cookie auth + role claims
                         |-- services (Dapper)
                         v
                     SQL Server (on-prem, UASU_Finance_01)
                         |-- same role rules re-checked by triggers
```

The app is the only thing that talks to SQL Server. Nothing outside it ever
sees a connection string. Role checks happen twice on purpose:

- **App layer** — two policies split what each role can reach:
  - `[Authorize(Policy = "Approvers")]` (Chairman, Chapter Secretary) — the
    Approvals page only. Reports are open to everyone with a login, so
    Approvers can view those too, but they have no route to create a
    voucher, a debt, a payment, an acknowledgement, or a new login.
  - `[Authorize(Policy = "TreasuryAdmin")]` (Treasurer, Deputy Treasurer) —
    every creation/admin page: New Voucher, New Debt, Record Payment,
    Acknowledge Receipt, Add Logins. Deliberately does **not** include the
    Approvals page - Treasurer and Deputy Treasurer can prepare and pay,
    but not approve their own work.
  - The nav bar and home page hide links a role can't use, but that's a
    convenience, not the actual security boundary - it's the `[Authorize]`
    attributes on each page that enforce it.
- **Database layer** — `trg_Voucher_Approvals_Validate` and
  `trg_PaymentAllocations_RequireApproval` re-check the approval rule
  independently of the app layer, so separation of duties holds even if
  something other than this app ever writes to the database.

## Setup

1. **Run the SQL** (in order, against `UASU_Finance_01`):
   - Your existing schema (`UASU_Finance_Complete.sql`)
   - `SQL/001_AddUsersTable.sql`
   - `SQL/002_LegacyVoucherExemption.sql` — only needed if you have
     vouchers that were paid *before* this approval workflow existed.
     Adds an `Is_Legacy` flag (set only by a one-time backfill, never
     by the app), so old payments aren't retroactively blocked without
     fabricating approval records that never actually happened. Skip
     this one if every voucher in your database postdates the workflow.
   - `SQL/003_ReportingViews.sql` — adds the four views behind the
     Reports page (monthly, quarterly, half-yearly, yearly).
   - `SQL/004_MigrateLegacyData.sql` — one-time copy of historical data
     from `UASU_Finance_01` into `UASU_Finance_Web`, if you're bringing
     old records into the database the app actually runs against. Has
     its own read-only pre-checks built in — **read the comments at the
     top and run its sections in order**, don't just execute the whole
     file blind.
   - `SQL/005_BootstrapFourUsers.sql` — only needed if `Users` is ever
     empty and nobody can sign in to reach the in-app `SeedDefaultLogins`
     page. Prefer `SeedDefaultLogins` when login already works.
   - `SQL/007_BudgetBreakdownViews.sql` — adds a budget-code breakdown
     view per reporting period, behind the "View Details by Budget
     Code" toggle on the Reports page.
   - `SQL/008_ValidatePaymentAllocationTotal.sql` — adds the missing
     symmetric check: a payment's total allocations can't exceed its
     `Amount_Paid`, mirroring the existing voucher-side check.

2. **Create logins.** This page requires being signed in as *someone*
   with a working login — it's not open to the public, but it's also
   not restricted to Chairman/Chapter Secretary, since whoever currently
   has access is the one who needs to onboard the next person. For the
   very first account in a brand-new database (when nobody can sign in
   at all yet), use the CLI tool below instead to create one login by
   hand — after that, everyone else can be added from
   `/Account/SeedDefaultLogins` in-app.

   Once signed in, visit `/Account/SeedDefaultLogins` and click the
   button. It creates a login for the current Chairman, Chapter
   Secretary, Treasurer, and Deputy Treasurer, using each officer's
   email already on file in `Ref_Officials` as their username, with a
   shared default password of `test1234`.

   `MustChangePassword` is set to `1` for every account it creates, so
   `test1234` is only good for one sign-in — the app forces a change
   immediately after login, before letting them reach any other page.

   **This page is a permanent onboarding tool, not a one-time script** —
   it's safe to come back and click the button again any time a new
   officer joins one of those four roles, since it never touches an
   account that already exists (existing logins are reported as
   `SKIPPED`, never reset).

   Onboarding someone in a role outside that fixed list, or prefer doing
   it by hand for one specific person? The standalone CLI tool works too:

   ```bash
   cd Tools/HashPasswordCli
   dotnet run
   Password to hash: ********
   ```

   It prints a hash — paste that into an `INSERT`, e.g.:

   ```sql
   INSERT INTO Users (OfficialID, Username, PasswordHash)
   SELECT OfficialID, 'chairman@uasu.example', '<paste-hash-here>'
   FROM Ref_Officials WHERE Role = 'Chairman' AND IsCurrent = 1;
   ```

   Copying a hash by hand like this is easy to get subtly wrong (an
   extra space, a missed character) — if login fails afterward with no
   obvious cause, that's the first thing to suspect.

3. **Set the connection string** via user-secrets rather than editing `appsettings.json` directly, so the real credentials never sit in a file inside the repo:

   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:UasuFinance" "Server=YOUR_SERVER;Database=UASU_Finance_01;User Id=...;Password=...;TrustServerCertificate=True;"
   ```

   `dotnet run` sets `ASPNETCORE_ENVIRONMENT=Development` (see `Properties/launchSettings.json`), so user-secrets are picked up automatically and override the placeholder value in `appsettings.json`. In production, set the `ConnectionStrings__UasuFinance` environment variable instead.

4. **Run it:**

   ```bash
   dotnet restore
   dotnet run
   ```

   Visit `https://localhost:5001` (or whatever port `dotnet run` prints).

## Hosting it so the Chairman/Chapter Secretary can reach it remotely

Since the database stays on-prem, host the app on the same machine/network
rather than splitting it across cloud + on-prem — that avoids opening the
SQL Server port to the internet entirely. To make the app reachable from
outside your office network without port-forwarding:

- **Cloudflare Tunnel** (free) — gives you a real HTTPS URL, no firewall
  changes needed.
- Or a reverse proxy (IIS/nginx) + a domain + a Let's Encrypt certificate,
  if you already have a static IP.

## Project layout

| Folder | Contents |
|---|---|
| `Program.cs` | Cookie auth, role policy, DI wiring |
| `Data/` | SQL Server connection factory |
| `Models/` | Request/response shapes |
| `Services/` | `AuthService` (login), `VoucherService` (approvals, vouchers, payments, acknowledgements) — all via Dapper |
| `Pages/Account/` | Login / logout / forced + voluntary password change |
| `Pages/Approvals/` | Chairman/Chapter Secretary approval queue — restricted by policy |
| `Pages/Vouchers/` | Voucher creation form |
| `Pages/Payments/` | Payment recording + payee acknowledgement |
| `Pages/Reports/` | Monthly/quarterly/half-yearly/yearly income-expense summaries, plus debt status/aging (`Reports/Debts`) - open to any logged-in user |
| `Pages/Debts/` | Record a new debt; raising a voucher against an existing debt is built into `Pages/Vouchers/Create` |
| `Pages/Banking/` | Record and list bank withdrawals |
| `Pages/Payments/Charges.cshtml` | Log a bank/M-PESA/cheque charge against a payment (and optionally a withdrawal) |
| `Pages/Payments/AddAllocation.cshtml` | Add a second (or third...) voucher to a payment that already exists |
| `SQL/001_AddUsersTable.sql` | The `Users` table this app needs, plus a guard trigger |
| `Tools/HashPasswordCli/` | Standalone console app - `dotnet run` to hash a password for seeding logins |

## Known gaps (things to add before real production use)

- No CSRF-specific hardening beyond Razor Pages' built-in antiforgery tokens
  (already on by default — just don't strip them from the forms).
- No rate limiting on the login page.
- No audit log beyond what `Voucher_Approvals`, `Users.LastLogin`, and
  `Payment_Acknowledgements` already capture.
- Not yet tested against a live SQL Server instance — validate the trigger
  interactions (`trg_PaymentStatusUpdate` vs `trg_PaymentAllocations_RequireApproval`)
  on a copy of the database before going live.
- `VoucherService.RecordPaymentAsync` inserts `Payments`, `PaymentAllocations`,
  and (when a withdrawal is linked) `WithdrawalPayments` as three separate
  statements with no explicit database transaction wrapping them. If the third
  insert fails, the first two have already committed — you'd end up with a
  real payment that's simply missing its withdrawal link, not a fully rolled-
  back attempt. Worth wrapping in an explicit `SqlTransaction` before relying
  on the withdrawal-linking feature for real volume.
