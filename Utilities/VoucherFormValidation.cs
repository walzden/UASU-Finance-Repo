using Microsoft.AspNetCore.Mvc.ModelBinding;
using UASU_VoucherApprovals.Models;

namespace UASU_VoucherApprovals.Utilities;

// Shared between Vouchers/Create and Vouchers/Pending (the "fix a typo
// before approval" page) - both post the same VoucherInputModel shape
// and need to mirror the same CK_Voucher_PayeeCategory / CK_Vouchers_
// TravelDetails database constraints, so the user gets a friendly
// validation message instead of a raw SQL error either way.
public static class VoucherFormValidation
{
    public static void NormalizePayeeFields(VoucherInputModel input, ModelStateDictionary modelState)
    {
        // Blank out whichever payee fields don't apply to the chosen
        // category, since the HTML form posts all three regardless.
        switch (input.Payee_Category)
        {
            case "Official":
                input.Supplier_Link = null;
                input.Manual_Payee_Name = null;
                if (string.IsNullOrWhiteSpace(input.Official_Link))
                    modelState.AddModelError(nameof(input.Official_Link), "Select an official.");
                break;
            case "Supplier":
                input.Official_Link = null;
                input.Manual_Payee_Name = null;
                if (string.IsNullOrWhiteSpace(input.Supplier_Link))
                    modelState.AddModelError(nameof(input.Supplier_Link), "Select a supplier.");
                break;
            default: // One-Time, Donor, Income Source
                input.Official_Link = null;
                input.Supplier_Link = null;
                if (string.IsNullOrWhiteSpace(input.Manual_Payee_Name))
                    modelState.AddModelError(nameof(input.Manual_Payee_Name), "Enter a payee name.");

                // Reg. 6(1)(e) only applies to money paid OUT - a Donor/
                // Income Source entry (money coming IN) reuses this same
                // field for who gave the union money, not a recipient.
                if (input.Transaction_Type == "Expense" && string.IsNullOrWhiteSpace(input.Manual_Payee_Address))
                    modelState.AddModelError(nameof(input.Manual_Payee_Address),
                        "Enter the payee's address (required on expense payments per the Labour Relations (Accounts) Regulations).");
                break;
        }
    }

    public static void NormalizeTravelFields(VoucherInputModel input, ModelStateDictionary modelState)
    {
        if (!input.Is_Travel_Expense)
        {
            // The form posts these regardless of whether the checkbox is
            // ticked - blank them out so a stray value left in a hidden
            // field never gets saved.
            input.Traveler_Name = null;
            input.Travel_From = null;
            input.Travel_To = null;
            input.Travel_Mode = null;
            input.Travel_Date = null;
            input.Travel_Reason = null;
            return;
        }

        if (string.IsNullOrWhiteSpace(input.Traveler_Name))
            modelState.AddModelError(nameof(input.Traveler_Name), "Enter who travelled.");
        if (string.IsNullOrWhiteSpace(input.Travel_From))
            modelState.AddModelError(nameof(input.Travel_From), "Enter where they travelled from.");
        if (string.IsNullOrWhiteSpace(input.Travel_To))
            modelState.AddModelError(nameof(input.Travel_To), "Enter where they travelled to.");
        if (string.IsNullOrWhiteSpace(input.Travel_Mode))
            modelState.AddModelError(nameof(input.Travel_Mode), "Enter the mode of transport.");
        if (input.Travel_Date is null)
            modelState.AddModelError(nameof(input.Travel_Date), "Enter the date of travel.");
        if (string.IsNullOrWhiteSpace(input.Travel_Reason))
            modelState.AddModelError(nameof(input.Travel_Reason), "Enter the reason for the journey.");
    }

    // Income-only auto-payment fields. Non-Income vouchers never show
    // this section on the form, so blank it out regardless of what a
    // stray posted value says. For Income + Mark_As_Received, Payment_
    // Mode is required (mirrors PaymentInputModel's own [Required]) -
    // Reference_No/Bank_Account stay optional, same as Record Payment.
    public static void NormalizeIncomePaymentFields(VoucherInputModel input, ModelStateDictionary modelState)
    {
        if (input.Transaction_Type != "Income")
        {
            input.Mark_As_Received = false;
            input.Payment_Mode = null;
            input.Payment_Reference_No = null;
            input.Payment_Bank_Account = null;
            return;
        }

        if (!input.Mark_As_Received)
        {
            input.Payment_Mode = null;
            input.Payment_Reference_No = null;
            input.Payment_Bank_Account = null;
            return;
        }

        if (string.IsNullOrWhiteSpace(input.Payment_Mode))
            modelState.AddModelError(nameof(input.Payment_Mode), "Select how this income was received.");
    }
}
