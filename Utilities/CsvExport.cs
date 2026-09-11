using System.Text;

namespace UASU_VoucherApprovals.Utilities;

// Shared by every report's CSV export handler - each page just supplies
// its own headers/rows rather than reimplementing escaping. Not worth a
// NuGet package for something this small.
public static class CsvExport
{
    public static byte[] Build(IEnumerable<string> headers, IEnumerable<IEnumerable<object?>> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", headers.Select(Escape)));
        foreach (var row in rows)
            sb.AppendLine(string.Join(",", row.Select(Escape)));

        // UTF-8 BOM so Excel doesn't mis-decode the file when opened
        // directly - its default CSV import otherwise assumes ANSI.
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static string Escape(object? value)
    {
        var text = value switch
        {
            null => string.Empty,
            DateTime d => d.ToString("yyyy-MM-dd"),
            decimal m => m.ToString("F2"),
            bool b => b ? "Yes" : "No",
            _ => value.ToString() ?? string.Empty
        };

        return text.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0
            ? "\"" + text.Replace("\"", "\"\"") + "\""
            : text;
    }
}
