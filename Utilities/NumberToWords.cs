using System.Text;
using System.Text.RegularExpressions;

namespace UASU_VoucherApprovals.Utilities;

// Converts a currency amount into words for the "Amount in words" line
// on Form R (Payment Voucher) - e.g. 122900.00 -> "One Hundred Twenty
// Two Thousand Nine Hundred Shillings Only". Not worth a NuGet package
// for something this small, same reasoning as CsvExport.
public static class NumberToWords
{
    private static readonly string[] Ones =
    {
        "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
        "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"
    };
    private static readonly string[] Tens =
    {
        "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
    };

    public static string ConvertShillings(decimal amount)
    {
        var whole = (long)Math.Floor(amount);
        var cents = (int)Math.Round((amount - whole) * 100);

        var result = $"{ConvertWhole(whole)} Shillings";
        if (cents > 0)
            result += $" and {ConvertWhole(cents)} Cents";

        return result + " Only";
    }

    private static string ConvertWhole(long number)
    {
        if (number == 0) return "Zero";
        if (number < 0) return "Negative " + ConvertWhole(-number);

        var sb = new StringBuilder();
        AppendGroup(sb, ref number, 1_000_000_000, "Billion");
        AppendGroup(sb, ref number, 1_000_000, "Million");
        AppendGroup(sb, ref number, 1_000, "Thousand");
        AppendUnderThousand(sb, (int)number);

        // AppendGroup/AppendUnderThousand each leave a trailing space -
        // collapse the doubled-up spacing where groups join rather than
        // track exact spacing inline.
        return Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
    }

    private static void AppendGroup(StringBuilder sb, ref long number, long unit, string unitName)
    {
        if (number < unit) return;
        var count = number / unit;
        number %= unit;
        AppendUnderThousand(sb, (int)count);
        sb.Append(' ').Append(unitName).Append(' ');
    }

    private static void AppendUnderThousand(StringBuilder sb, int number)
    {
        if (number == 0) return;

        if (number >= 100)
        {
            sb.Append(Ones[number / 100]).Append(" Hundred ");
            number %= 100;
        }

        if (number >= 20)
        {
            sb.Append(Tens[number / 10]).Append(' ');
            number %= 10;
        }
        else if (number >= 10)
        {
            sb.Append(Ones[number]).Append(' ');
            return;
        }

        if (number is > 0 and < 10)
            sb.Append(Ones[number]).Append(' ');
    }
}
