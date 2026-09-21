using System.Globalization;

namespace UASU_VoucherApprovals.Utilities;

// One line of a voucher description: what was done and when.
public record DescriptionItem(DateTime Date, string Title, string Category);

// Builds the voucher description for one voucher that pays several debts /
// activities, with each one's date beside its title, e.g.
//   "Oct 2026 union activities: Finance sub-committee meeting (9 Oct); ..."
// Vouchers.Description holds at most 1000 characters (SQL/029), so when the
// full wording is too long it falls back to "type: dates" - the debts linked
// to the voucher still hold the full detail.
public static class ActivityDescription
{
    public const int MaxLength = 1000;

    public static (string Text, bool Shortened) Build(IEnumerable<DescriptionItem> items)
    {
        var sorted = items.OrderBy(i => i.Date).ThenBy(i => i.Title).ToList();
        // Invariant so months always abbreviate the same way ("Sep", not en-GB's "Sept").
        var culture = CultureInfo.InvariantCulture;
        var prefix = $"{Period(sorted[0].Date, sorted[^1].Date, culture)} union activities: ";

        var full = prefix + string.Join("; ", sorted.Select(i => $"{i.Title} ({ShortDate(i.Date, culture)})"));
        if (full.Length <= MaxLength)
            return (full, false);

        var byCategory = sorted
            .GroupBy(i => i.Category)
            .Select(g => $"{g.Key}: {string.Join(", ", g.Select(i => ShortDate(i.Date, culture)))}");
        var compact = prefix + string.Join("; ", byCategory);

        return (compact.Length <= MaxLength ? compact : compact[..(MaxLength - 1)] + "…", true);
    }

    // "Sep 2026", or "Feb-Mar 2026" / "Nov 2025-Mar 2026" when the items span months.
    private static string Period(DateTime first, DateTime last, CultureInfo culture)
    {
        if (first.Year == last.Year && first.Month == last.Month)
            return first.ToString("MMM yyyy", culture);
        if (first.Year == last.Year)
            return $"{first.ToString("MMM", culture)}-{last.ToString("MMM yyyy", culture)}";
        return $"{first.ToString("MMM yyyy", culture)}-{last.ToString("MMM yyyy", culture)}";
    }

    private static string ShortDate(DateTime d, CultureInfo culture) => d.ToString("d MMM", culture);
}
