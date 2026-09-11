using Dapper;
using UASU_VoucherApprovals.Data;
using UASU_VoucherApprovals.Models;

namespace UASU_VoucherApprovals.Services;

public interface IBankingService
{
    Task<string> CreateWithdrawalAsync(BankWithdrawalInputModel input);
    Task<IEnumerable<WithdrawalListRow>> GetWithdrawalsAsync();
    Task<IEnumerable<SimpleOption>> GetWithdrawalsWithBalanceAsync();
    Task<IEnumerable<SimpleOption>> GetAllWithdrawalsAsync();
    Task<IEnumerable<WithdrawalPaymentRow>> GetWithdrawalPaymentsAsync();
    Task<IEnumerable<WithdrawalPaymentLinkRow>> GetWithdrawalPaymentLinksAsync();
    Task<IEnumerable<WithdrawalPaymentVoucherRow>> GetWithdrawalPaymentVouchersAsync();

    Task LinkPaymentToWithdrawalAsync(string withdrawalId, string paymentId, decimal amount);

    Task<IEnumerable<SimpleOption>> GetRecentPaymentsAsync();
    Task<IEnumerable<SimpleOption>> GetPaymentsWithUnallocatedBalanceAsync();
    Task<string> CreateChargeAsync(PaymentChargeInputModel input);

    Task<string> CreateAccountChargeAsync(AccountChargeInputModel input);
    Task<IEnumerable<AccountChargeRow>> GetAccountChargesAsync();

    Task AddAllocationAsync(AddAllocationInputModel input);
}

public class BankingService : IBankingService
{
    private readonly IDbConnectionFactory _db;

    public BankingService(IDbConnectionFactory db)
    {
        _db = db;
    }

    // ------------------------------------------------------------------
    // Withdrawals
    // ------------------------------------------------------------------

    public async Task<string> CreateWithdrawalAsync(BankWithdrawalInputModel input)
    {
        using var conn = _db.CreateConnection();

        // Withdrawal_ID is server-generated (DEFAULT expression using
        // BankWithdrawalSeq), so OUTPUT INSERTED.Withdrawal_ID hands the
        // new number straight back rather than re-querying for it.
        const string sql = @"
            INSERT INTO BankWithdrawals (Withdrawal_Date, Amount, Reference_No, Bank_Account, Notes)
            OUTPUT INSERTED.Withdrawal_ID
            VALUES (@Withdrawal_Date, @Amount, @Reference_No, @Bank_Account, @Notes);";

        return await conn.ExecuteScalarAsync<string>(sql, input);
    }

    public async Task<IEnumerable<WithdrawalListRow>> GetWithdrawalsAsync()
    {
        using var conn = _db.CreateConnection();

        // BankReconciliation already does this calculation correctly,
        // including apportioning charges that aren't tied to a specific
        // withdrawal (Withdrawal_ID IS NULL on PaymentCharges) pro-rata
        // across the withdrawals that funded that payment. Reusing it
        // here means this list can never drift out of sync with the
        // reconciliation report - there's exactly one place that knows
        // how to net a withdrawal against both allocations and charges.
        const string sql = @"
            SELECT w.Withdrawal_ID, w.Withdrawal_Date, w.Amount, w.Reference_No, w.Bank_Account, w.Notes,
                   br.TotalAllocated AS Allocated,
                   br.TotalCharges AS Charges,
                   br.UnallocatedBalance AS Remaining
            FROM BankWithdrawals w
            INNER JOIN BankReconciliation br ON br.Withdrawal_ID = w.Withdrawal_ID
            ORDER BY w.Withdrawal_Date DESC;";

        return await conn.QueryAsync<WithdrawalListRow>(sql);
    }

    public async Task<IEnumerable<SimpleOption>> GetWithdrawalsWithBalanceAsync()
    {
        using var conn = _db.CreateConnection();

        // Same fix as GetWithdrawalsAsync above: a withdrawal with money
        // still owed to bank/M-PESA charges isn't actually available to
        // fund a new payment, even if WithdrawalPayments alone would
        // suggest otherwise.
        const string sql = @"
            SELECT br.Withdrawal_ID AS Id,
                   br.Withdrawal_ID + ' - ' + FORMAT(br.Withdrawal_Date,'yyyy-MM-dd')
                   + ' - Sh.' + FORMAT(br.UnallocatedBalance,'N2') + ' remaining' AS Label
            FROM BankReconciliation br
            WHERE br.UnallocatedBalance > 0
            ORDER BY br.Withdrawal_Date DESC;";

        return await conn.QueryAsync<SimpleOption>(sql);
    }

    public async Task<IEnumerable<SimpleOption>> GetAllWithdrawalsAsync()
    {
        using var conn = _db.CreateConnection();

        // Unlike GetWithdrawalsWithBalanceAsync above, this is NOT
        // filtered to remaining balance > 0 - a charge can validly
        // reference a withdrawal that's already fully allocated to
        // payments, since the trigger only checks that WithdrawalPayments
        // links this withdrawal to this payment, not whether money is
        // left over.
        const string sql = @"
            SELECT Withdrawal_ID AS Id,
                   Withdrawal_ID + ' - ' + FORMAT(Withdrawal_Date,'yyyy-MM-dd') AS Label
            FROM BankWithdrawals
            ORDER BY Withdrawal_Date DESC;";

        return await conn.QueryAsync<SimpleOption>(sql);
    }

    // Every payment that's been funded (in whole or in part) from any
    // withdrawal, flat - the page groups these by Withdrawal_ID itself.
    // WithdrawalPayments can hold more than one row for the same
    // (Withdrawal_ID, Payment_ID) pair - cash gets linked to a payment
    // in increments, not necessarily as one consolidated row - so this
    // SUMs Allocated_Amount per pair rather than returning one row per
    // link. WithdrawalCount uses COUNT(DISTINCT ...) for the same
    // reason: multiple increments from the *same* withdrawal must not
    // be counted as if the payment were split across several different
    // withdrawals.
    public async Task<IEnumerable<WithdrawalPaymentRow>> GetWithdrawalPaymentsAsync()
    {
        using var conn = _db.CreateConnection();

        const string sql = @"
            SELECT wp.Withdrawal_ID, p.Payment_ID, p.Payment_Date, p.Payment_Mode, p.[Description],
                   SUM(wp.Allocated_Amount) AS FundedFromThisWithdrawal,
                   p.Amount_Paid,
                   (SELECT COUNT(DISTINCT wp2.Withdrawal_ID) FROM WithdrawalPayments wp2 WHERE wp2.Payment_ID = p.Payment_ID) AS WithdrawalCount
            FROM WithdrawalPayments wp
            INNER JOIN Payments p ON p.Payment_ID = wp.Payment_ID
            GROUP BY wp.Withdrawal_ID, p.Payment_ID, p.Payment_Date, p.Payment_Mode, p.[Description], p.Amount_Paid
            ORDER BY wp.Withdrawal_ID, p.Payment_Date;";

        return await conn.QueryAsync<WithdrawalPaymentRow>(sql);
    }

    // Raw WithdrawalPayments rows, ungrouped - the page uses this to
    // attribute individual vouchers to the specific withdrawal that
    // funded them (by matching amounts against PaymentAllocations),
    // which GetWithdrawalPaymentsAsync's SUMmed rows can't support.
    public async Task<IEnumerable<WithdrawalPaymentLinkRow>> GetWithdrawalPaymentLinksAsync()
    {
        using var conn = _db.CreateConnection();

        const string sql = @"
            SELECT Withdrawal_ID, Payment_ID, Allocated_Amount
            FROM WithdrawalPayments
            ORDER BY Payment_ID, Withdrawal_ID;";

        return await conn.QueryAsync<WithdrawalPaymentLinkRow>(sql);
    }

    // Every voucher allocated against a payment that's linked to at
    // least one withdrawal - scoped this way (rather than pulling every
    // PaymentAllocations row) since a payment made straight from a
    // bank transfer with no withdrawal behind it has nothing to show
    // on the withdrawal breakdown report.
    public async Task<IEnumerable<WithdrawalPaymentVoucherRow>> GetWithdrawalPaymentVouchersAsync()
    {
        using var conn = _db.CreateConnection();

        const string sql = @"
            SELECT pa.Payment_ID, v.Voucher_ID, v.VoucherDate,
                   COALESCE(o.FullName, s.Business_Name, v.Manual_Payee_Name) AS PayeeDisplay,
                   v.[Description],
                   v.Amount AS VoucherAmount,
                   pa.Allocated_Amount AS AllocatedFromThisPayment,
                   bc.Budget_ID, bc.Category_Name,
                   v.Is_Legacy
            FROM PaymentAllocations pa
            INNER JOIN Vouchers v ON v.Voucher_ID = pa.Voucher_ID
            LEFT JOIN Ref_Officials o ON o.OfficialID = v.Official_Link
            LEFT JOIN Ref_Suppliers s ON s.Supplier_ID = v.Supplier_Link
            LEFT JOIN Ref_BudgetCodes bc ON bc.Budget_ID = v.Budget_Link
            WHERE EXISTS (SELECT 1 FROM WithdrawalPayments wp WHERE wp.Payment_ID = pa.Payment_ID)
            ORDER BY pa.Payment_ID, v.VoucherDate;";

        return await conn.QueryAsync<WithdrawalPaymentVoucherRow>(sql);
    }

    public async Task LinkPaymentToWithdrawalAsync(string withdrawalId, string paymentId, decimal amount)
    {
        using var conn = _db.CreateConnection();

        const string sql = @"
            INSERT INTO WithdrawalPayments (Withdrawal_ID, Payment_ID, Allocated_Amount)
            VALUES (@WithdrawalId, @PaymentId, @Amount);";

        await conn.ExecuteAsync(sql, new { WithdrawalId = withdrawalId, PaymentId = paymentId, Amount = amount });
    }

    // ------------------------------------------------------------------
    // Charges
    // ------------------------------------------------------------------

    public async Task<IEnumerable<SimpleOption>> GetRecentPaymentsAsync()
    {
        using var conn = _db.CreateConnection();

        // Scoped to M-Pesa payments with no charge logged yet - M-Pesa is
        // the mode that actually incurs a per-transaction charge here,
        // and once a payment already has a PaymentCharges row there's
        // nothing left to log against it, so leaving it in the dropdown
        // just invited picking it again by mistake. Also excludes Income
        // payments (e.g. a donor grant received via M-Pesa) - a charge is
        // a deduction against money the union paid OUT, not money it
        // received, same reasoning as excluding Income from
        // PaymentsAwaitingAcknowledgement (SQL/009).
        const string sql = @"
            SELECT TOP 100 p.Payment_ID AS Id,
                   p.Payment_ID + ' - ' + FORMAT(p.Payment_Date,'yyyy-MM-dd') + ' - Sh.' + FORMAT(p.Amount_Paid,'N2') AS Label
            FROM Payments p
            WHERE p.Payment_Mode = 'M-Pesa'
              AND NOT EXISTS (SELECT 1 FROM PaymentCharges pc WHERE pc.Payment_ID = p.Payment_ID)
              AND EXISTS (
                    SELECT 1 FROM PaymentAllocations pa
                    INNER JOIN Vouchers v ON v.Voucher_ID = pa.Voucher_ID
                    WHERE pa.Payment_ID = p.Payment_ID AND v.Transaction_Type = 'Expense'
              )
            ORDER BY p.Payment_Date DESC;";

        return await conn.QueryAsync<SimpleOption>(sql);
    }

    public async Task<IEnumerable<SimpleOption>> GetPaymentsWithUnallocatedBalanceAsync()
    {
        using var conn = _db.CreateConnection();

        // A payment's Amount_Paid is its fixed total; PaymentAllocations
        // is how much of that total has already been assigned to
        // vouchers. Once those two are equal, there's no capacity left
        // to add another voucher to this payment - so it drops out of
        // this list even though it still appears on the Charges page's
        // payment dropdown (a charge doesn't consume allocation
        // capacity, so that one intentionally isn't filtered this way).
        const string sql = @"
            SELECT TOP 100 p.Payment_ID AS Id,
                   p.Payment_ID + ' - ' + FORMAT(p.Payment_Date,'yyyy-MM-dd')
                   + ' - Sh.' + FORMAT(p.Amount_Paid - ISNULL(SUM(pa.Allocated_Amount), 0),'N2') + ' unallocated' AS Label
            FROM Payments p
            LEFT JOIN PaymentAllocations pa ON pa.Payment_ID = p.Payment_ID
            GROUP BY p.Payment_ID, p.Payment_Date, p.Amount_Paid
            HAVING p.Amount_Paid - ISNULL(SUM(pa.Allocated_Amount), 0) > 0
            ORDER BY p.Payment_Date DESC;";

        return await conn.QueryAsync<SimpleOption>(sql);
    }

    public async Task<string> CreateChargeAsync(PaymentChargeInputModel input)
    {
        using var conn = _db.CreateConnection();

        // trg_PaymentCharges_ValidateWithdrawal in the database still
        // enforces the real rule (Withdrawal_ID must actually have
        // funded this Payment_ID via WithdrawalPayments) - this insert
        // can throw a SqlException the calling page should show back
        // to the user rather than a raw constraint error.
        //
        // OUTPUT INSERTED.Charge_ID with no INTO clause isn't allowed on
        // a table with an enabled trigger matching the statement (SQL
        // Server restriction) - trg_PaymentCharges_ValidateWithdrawal is
        // exactly that (AFTER INSERT), so the new Charge_ID has to be
        // captured via a table variable instead, which sidesteps the
        // restriction entirely while the trigger still fires normally.
        const string sql = @"
            DECLARE @Inserted TABLE (Charge_ID NVARCHAR(40));
            INSERT INTO PaymentCharges (Payment_ID, Withdrawal_ID, Charge_Type, Charge_Amount, Notes)
            OUTPUT INSERTED.Charge_ID INTO @Inserted
            VALUES (@Payment_ID, @Withdrawal_ID, @Charge_Type, @Charge_Amount, @Notes);
            SELECT Charge_ID FROM @Inserted;";

        return await conn.ExecuteScalarAsync<string>(sql, input);
    }

    // ------------------------------------------------------------------
    // Account charges (bank-initiated, not tied to a payment/withdrawal)
    // ------------------------------------------------------------------

    public async Task<string> CreateAccountChargeAsync(AccountChargeInputModel input)
    {
        using var conn = _db.CreateConnection();

        const string sql = @"
            INSERT INTO AccountCharges (Charge_Date, Bank_Account, Charge_Type, Charge_Amount, Notes)
            OUTPUT INSERTED.Charge_ID
            VALUES (@Charge_Date, @Bank_Account, @Charge_Type, @Charge_Amount, @Notes);";

        return await conn.ExecuteScalarAsync<string>(sql, input);
    }

    public async Task<IEnumerable<AccountChargeRow>> GetAccountChargesAsync()
    {
        using var conn = _db.CreateConnection();

        const string sql = @"
            SELECT Charge_ID, Charge_Date, Bank_Account, Charge_Type, Charge_Amount, Notes
            FROM AccountCharges
            ORDER BY Charge_Date DESC, Charge_ID DESC;";

        return await conn.QueryAsync<AccountChargeRow>(sql);
    }

    // ------------------------------------------------------------------
    // Additional allocations on an existing payment
    // ------------------------------------------------------------------

    public async Task AddAllocationAsync(AddAllocationInputModel input)
    {
        using var conn = _db.CreateConnection();

        // trg_PaymentAllocations_RequireApproval and trg_PaymentStatusUpdate
        // both fire here exactly as they do for a brand-new payment - a
        // second voucher added to an existing payment goes through the
        // same approval-eligibility and over-allocation checks.
        const string sql = @"
            INSERT INTO PaymentAllocations (Payment_ID, Voucher_ID, Allocated_Amount)
            VALUES (@Payment_ID, @Voucher_ID, @Allocated_Amount);";

        await conn.ExecuteAsync(sql, input);
    }
}
