using System.ComponentModel.DataAnnotations;

namespace UASU_VoucherApprovals.Models;

// Labour Relations (Accounts) Regulations reg. 8(2)/9(2): the treasurer
// must balance the cash book monthly and certify that it agrees with
// actual cash in hand and cash at the bank. Book_* figures are computed
// by CertificationService from Payments, not typed in - only the
// physically-verified Actual_* figures are user input.
public class CertificationInputModel
{
    [Required]
    [Range(2000, 2100)]
    public int Period_Year { get; set; }

    [Required]
    [Range(1, 12)]
    public int Period_Month { get; set; }

    [Required]
    public decimal Actual_Cash_On_Hand { get; set; }

    [Required]
    public decimal Actual_Bank_Balance { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class CertificationRow
{
    public string Certification_ID { get; set; } = string.Empty;
    public int Period_Year { get; set; }
    public int Period_Month { get; set; }
    public decimal Book_Cash_Balance { get; set; }
    public decimal Book_Bank_Balance { get; set; }
    public decimal Actual_Cash_On_Hand { get; set; }
    public decimal Actual_Bank_Balance { get; set; }
    public string CertifiedByName { get; set; } = string.Empty;
    public DateTime Certified_Date { get; set; }
    public string? Notes { get; set; }

    public decimal CashVariance => Actual_Cash_On_Hand - Book_Cash_Balance;
    public decimal BankVariance => Actual_Bank_Balance - Book_Bank_Balance;
    public bool Balances => CashVariance == 0 && BankVariance == 0;
}

// One-time starting point folded into every GetBookBalancesAsync call -
// not a transaction, so it's edited in place rather than logged like
// MonthlyCertifications. Seeded at zero until real figures are on hand.
public class OpeningBalanceInputModel
{
    [Required]
    public DateTime As_Of_Date { get; set; }

    [Required]
    public decimal Cash_Balance { get; set; }

    [Required]
    public decimal Bank_Balance { get; set; }
}
