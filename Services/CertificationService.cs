using Dapper;
using UASU_VoucherApprovals.Data;
using UASU_VoucherApprovals.Models;

namespace UASU_VoucherApprovals.Services;

public interface ICertificationService
{
    Task<(decimal CashBalance, decimal BankBalance)> GetBookBalancesAsync(int year, int month);
    Task<string> CertifyAsync(CertificationInputModel input, string certifiedByOfficialId);
    Task<IEnumerable<CertificationRow>> GetCertificationsAsync();

    Task<OpeningBalanceInputModel> GetOpeningBalanceAsync();
    Task SetOpeningBalanceAsync(OpeningBalanceInputModel input, string setByOfficialId);
}

public class CertificationService : ICertificationService
{
    private readonly IDbConnectionFactory _db;

    public CertificationService(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<(decimal CashBalance, decimal BankBalance)> GetBookBalancesAsync(int year, int month)
    {
        using var conn = _db.CreateConnection();

        var monthEnd = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);

        // Cumulative to month-end, not just that month's movement -
        // "balance the cash book" means the running balance, the same
        // way a real cash book would read at any point in time.
        //
        // A payment's direction (money in vs. out) comes from whichever
        // voucher(s) it's allocated to. Which BUCKET it belongs in (cash
        // vs bank) is NOT simply Payment_Mode = 'Cash', though - a
        // payment funded by a bank withdrawal already left the bank
        // account the moment that withdrawal happened, regardless of how
        // the cash subsequently reached the recipient (handed over
        // directly, sent on via M-Pesa from the withdrawn float, etc.).
        // Payment_Mode records that final leg faithfully - e.g. "M-Pesa"
        // for a payment actually funded by a withdrawal - and changing
        // it to "Cash" to force it into the right bucket would make the
        // record itself inaccurate. So IsCashBucket is Payment_Mode =
        // 'Cash' OR the payment is linked to any BankWithdrawal
        // (WithdrawalPayments) - whichever is true, this payment's money
        // came from the withdrawn-cash pool, not straight out of the
        // bank account. Income can never be withdrawal-linked (a
        // withdrawal only ever funds outgoing Expense payments), so for
        // Income, Payment_Mode = 'Cash' remains the only signal available
        // for "this money never touched the bank account" (e.g. a grant
        // received and spent without ever being banked).
        //
        // PaymentCharges (bank/M-Pesa/cheque fees) split by whether the
        // underlying Payment was funded by any withdrawal at all
        // (WithdrawalPayments): a fee on a payment that was NEVER cash -
        // e.g. a bank-transfer payment straight out of the account -
        // comes out of the bank bucket, same as always. But a fee on a
        // payment that WAS funded from a withdrawal already came out of
        // that already-withdrawn cash, not the bank account a second
        // time - the withdrawal itself already reduced Bank for the
        // FULL amount, fees included (confirmed against real data: every
        // withdrawal's Amount exactly equals what it funded in
        // WithdrawalPayments plus the PaymentCharges attributed to it).
        // Subtracting those fees from Bank again as well double-counted
        // them, understating Bank and leaving a phantom balance sitting
        // in Cash that was never actually cash in hand. PaymentCharges
        // has no date column of its own, so its associated Payment's
        // date stands in as the closest available proxy for when the
        // charge landed.
        //
        // AccountCharges (ledger fees, excise duty, maintenance - not
        // tied to any payment) always come straight out of the bank
        // bucket, using their own Charge_Date directly since, unlike
        // PaymentCharges, there's no Payment to borrow a date from.
        const string sql = @"
            WITH PaymentDirection AS (
                SELECT p.Payment_ID, p.Amount_Paid,
                       MAX(v.Transaction_Type) AS Transaction_Type,
                       CASE WHEN p.Payment_Mode = 'Cash'
                              OR EXISTS (SELECT 1 FROM WithdrawalPayments wp WHERE wp.Payment_ID = p.Payment_ID)
                            THEN 1 ELSE 0 END AS IsCashBucket
                FROM Payments p
                INNER JOIN PaymentAllocations pa ON pa.Payment_ID = p.Payment_ID
                INNER JOIN Vouchers v ON v.Voucher_ID = pa.Voucher_ID
                WHERE p.Payment_Date <= @MonthEnd
                GROUP BY p.Payment_ID, p.Payment_Mode, p.Amount_Paid
            ),
            Withdrawn AS (
                SELECT ISNULL(SUM(Amount), 0) AS Total
                FROM BankWithdrawals
                WHERE Withdrawal_Date <= @MonthEnd
            ),
            Charged AS (
                SELECT ISNULL(SUM(pc.Charge_Amount), 0) AS Total
                FROM PaymentCharges pc
                INNER JOIN Payments p ON p.Payment_ID = pc.Payment_ID
                WHERE p.Payment_Date <= @MonthEnd
                  AND NOT EXISTS (SELECT 1 FROM WithdrawalPayments wp WHERE wp.Payment_ID = pc.Payment_ID)
            ),
            CashCharged AS (
                SELECT ISNULL(SUM(pc.Charge_Amount), 0) AS Total
                FROM PaymentCharges pc
                INNER JOIN Payments p ON p.Payment_ID = pc.Payment_ID
                WHERE p.Payment_Date <= @MonthEnd
                  AND EXISTS (SELECT 1 FROM WithdrawalPayments wp WHERE wp.Payment_ID = pc.Payment_ID)
            ),
            AccountCharged AS (
                SELECT ISNULL(SUM(Charge_Amount), 0) AS Total
                FROM AccountCharges
                WHERE Charge_Date <= @MonthEnd
            ),
            Opening AS (
                -- Guarded by As_Of_Date so certifying a period before the
                -- opening balance's own as-of date (shouldn't normally
                -- happen, but cheap to guard) doesn't add a starting
                -- figure that hadn't actually been established yet.
                SELECT
                    CASE WHEN As_Of_Date <= @MonthEnd THEN Cash_Balance ELSE 0 END AS Cash,
                    CASE WHEN As_Of_Date <= @MonthEnd THEN Bank_Balance ELSE 0 END AS Bank
                FROM OpeningBalance WHERE Id = 1
            )
            SELECT
                (
                    (SELECT Cash FROM Opening)
                    + (SELECT Total FROM Withdrawn)
                    + ISNULL((SELECT SUM(CASE WHEN Transaction_Type = 'Income' THEN Amount_Paid ELSE -Amount_Paid END)
                              FROM PaymentDirection WHERE IsCashBucket = 1), 0)
                    - (SELECT Total FROM CashCharged)
                ) AS CashBalance,
                (
                    (SELECT Bank FROM Opening)
                    + ISNULL((SELECT SUM(CASE WHEN Transaction_Type = 'Income' THEN Amount_Paid ELSE -Amount_Paid END)
                            FROM PaymentDirection WHERE IsCashBucket = 0), 0)
                    - (SELECT Total FROM Withdrawn)
                    - (SELECT Total FROM Charged)
                    - (SELECT Total FROM AccountCharged)
                ) AS BankBalance;";

        var result = await conn.QuerySingleAsync<BalancesResult>(sql, new { MonthEnd = monthEnd });
        return (result.CashBalance ?? 0m, result.BankBalance ?? 0m);
    }

    // Dapper maps POCOs from a row, not bare ValueTuples - this exists
    // purely to receive the two aggregate columns above.
    private class BalancesResult
    {
        public decimal? CashBalance { get; set; }
        public decimal? BankBalance { get; set; }
    }

    public async Task<string> CertifyAsync(CertificationInputModel input, string certifiedByOfficialId)
    {
        using var conn = _db.CreateConnection();

        var (cashBalance, bankBalance) = await GetBookBalancesAsync(input.Period_Year, input.Period_Month);

        // Certification_ID is server-generated (DEFAULT expression using
        // seq_CertificationID), so OUTPUT INSERTED.Certification_ID hands
        // the new ID straight back rather than re-querying for it.
        const string sql = @"
            INSERT INTO MonthlyCertifications
                (Period_Year, Period_Month, Book_Cash_Balance, Book_Bank_Balance,
                 Actual_Cash_On_Hand, Actual_Bank_Balance, Certified_By, Notes)
            OUTPUT INSERTED.Certification_ID
            VALUES
                (@Period_Year, @Period_Month, @BookCashBalance, @BookBankBalance,
                 @Actual_Cash_On_Hand, @Actual_Bank_Balance, @CertifiedBy, @Notes);";

        return await conn.ExecuteScalarAsync<string>(sql, new
        {
            input.Period_Year,
            input.Period_Month,
            BookCashBalance = cashBalance,
            BookBankBalance = bankBalance,
            input.Actual_Cash_On_Hand,
            input.Actual_Bank_Balance,
            CertifiedBy = certifiedByOfficialId,
            input.Notes
        });
    }

    public async Task<IEnumerable<CertificationRow>> GetCertificationsAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT
                c.Certification_ID, c.Period_Year, c.Period_Month,
                c.Book_Cash_Balance, c.Book_Bank_Balance,
                c.Actual_Cash_On_Hand, c.Actual_Bank_Balance,
                o.FullName AS CertifiedByName, c.Certified_Date, c.Notes
            FROM MonthlyCertifications c
            INNER JOIN Ref_Officials o ON o.OfficialID = c.Certified_By
            ORDER BY c.Period_Year DESC, c.Period_Month DESC, c.Certified_Date DESC;";
        return await conn.QueryAsync<CertificationRow>(sql);
    }

    // ------------------------------------------------------------------
    // Opening balance - a single settings-style row (Id = 1), not a
    // transaction log. Seeded at zero by SQL/015; edited in place once
    // the real figures are known.
    // ------------------------------------------------------------------

    public async Task<OpeningBalanceInputModel> GetOpeningBalanceAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = "SELECT As_Of_Date, Cash_Balance, Bank_Balance FROM OpeningBalance WHERE Id = 1;";
        return await conn.QuerySingleAsync<OpeningBalanceInputModel>(sql);
    }

    public async Task SetOpeningBalanceAsync(OpeningBalanceInputModel input, string setByOfficialId)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            UPDATE OpeningBalance
            SET As_Of_Date = @As_Of_Date, Cash_Balance = @Cash_Balance, Bank_Balance = @Bank_Balance,
                Set_By = @SetBy, Set_Date = GETDATE()
            WHERE Id = 1;";
        await conn.ExecuteAsync(sql, new
        {
            input.As_Of_Date,
            input.Cash_Balance,
            input.Bank_Balance,
            SetBy = setByOfficialId
        });
    }
}
