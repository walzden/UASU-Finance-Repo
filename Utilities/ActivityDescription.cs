using System.Globalization;
using UASU_VoucherApprovals.Models;

namespace UASU_VoucherApprovals.Utilities;

// Builds the voucher description for one voucher that pays several
// activities, with each activity's date beside its title, e.g.
//   "Oct 2026 union activities: Finance sub-committee meeting (9 Oct); ..."
// Vouchers.Description holds at most 1000 characters (SQL/029), so when the
// full wording is too long it falls back to "type: dates" - the activity
// list linked to the voucher still holds the full detail.
public static class ActivityDescription
{
    public const int MaxLength = 1000;

    public static (string Text, bool Shortened) Build(IEnumerable<OpenActivityLine> lines)
    {
        var sorted = lines.OrderBy(l => l.Activity_Date).ThenBy(l => l.Activity_ID).ToList();
        // Invariant so months always abbreviate the same way ("Sep", not en-GB's "Sept").
        var culture = CultureInfo.InvariantCulture;
        var prefix = $"{sorted[0].Activity_Date.ToString("MMM yyyy", culture)} union activities: ";

        var full = prefix + string.Join("; ", sorted.Select(l => $"{l.Title} ({ShortDate(l.Activity_Date, culture)})"));
        if (full.Length <= MaxLength)
            return (full, false);

        var byCategory = sorted
            .GroupBy(l => l.Category)
            .Select(g => $"{g.Key}: {string.Join(", ", g.Select(l => ShortDate(l.Activity_Date, culture)))}");
        var compact = prefix + string.Join("; ", byCategory);

        return (compact.Length <= MaxLength ? compact : compact[..(MaxLength - 1)] + "…", true);
    }

    private static string ShortDate(DateTime d, CultureInfo culture) => d.ToString("d MMM", culture);
}
