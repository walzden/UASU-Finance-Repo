using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using UASU_VoucherApprovals.Models;
using UASU_VoucherApprovals.Services;
using UASU_VoucherApprovals.Utilities;

namespace UASU_VoucherApprovals.Pages.Banking;

[Authorize(Policy = "TreasuryAdmin")]
public class WithdrawalsModel : PageModel
{
    private readonly IBankingService _bankingService;

    public WithdrawalsModel(IBankingService bankingService)
    {
        _bankingService = bankingService;
    }

    [BindProperty]
    public BankWithdrawalInputModel Input { get; set; } = new();

    public IEnumerable<WithdrawalListRow> Withdrawals { get; set; } = Enumerable.Empty<WithdrawalListRow>();

    public string? StatusMessage { get; set; }
    public bool StatusIsError { get; set; }

    public async Task OnGetAsync()
    {
        Withdrawals = await _bankingService.GetWithdrawalsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Withdrawals = await _bankingService.GetWithdrawalsAsync();
            return Page();
        }

        try
        {
            var withdrawalId = await _bankingService.CreateWithdrawalAsync(Input);
            StatusMessage = $"Withdrawal {withdrawalId} recorded.";
            Input = new BankWithdrawalInputModel();

            // See Vouchers/Create.cshtml.cs for why this is also needed -
            // the asp-for tag helpers read from ModelState before the
            // model, so resetting Input alone wouldn't clear the form.
            ModelState.Clear();
        }
        catch (SqlException ex)
        {
            StatusIsError = true;
            StatusMessage = ex.Message;
        }

        Withdrawals = await _bankingService.GetWithdrawalsAsync();
        return Page();
    }

    public async Task<IActionResult> OnGetExportCsvAsync()
    {
        var rows = await _bankingService.GetWithdrawalsAsync();
        var csv = CsvExport.Build(
            new[] { "Withdrawal", "Date", "Reference", "Bank Account", "Amount", "Allocated to Payments", "Charges", "Remaining" },
            rows.Select(w => new object?[]
            {
                w.Withdrawal_ID, w.Withdrawal_Date, w.Reference_No, w.Bank_Account,
                w.Amount, w.Allocated, w.Charges, w.Remaining
            }));

        return File(csv, "text/csv", "BankWithdrawals.csv");
    }
}
