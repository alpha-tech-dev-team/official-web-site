using System.Globalization;
using System.Text.Json;
using ApplicationsApi.Models;

namespace ApplicationsApi.Documents;

public static class ApplicationValues
{
    public static Dictionary<string, string> From(Application a)
    {
        var values = new Dictionary<string, string>
        {
            ["number"] = a.Number ?? "без номера",
            ["applicantName"] = a.ApplicantName,
            ["phone"] = a.Phone,
            ["phoneFormatted"] = FormatPhone(a.Phone),
            ["status"] = a.Status.ToString(),
            ["createdAt"] = FormatDate(a.CreatedAt),
            ["submittedAt"] = FormatDate(a.SubmittedAt),
            ["generatedAt"] = FormatDate(DateTime.UtcNow),
        };

        foreach (var (key, value) in a.Data)
        {
            values[$"data.{key}"] = value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? ""
                : value.ToString();
        }

        return values;
    }

    private static string FormatPhone(string p) =>
        p.Length == 12 ? $"+{p[..3]} {p[3..5]} {p[5..8]}-{p[8..10]}-{p[10..12]}" : p;

    private static string FormatDate(DateTime? date) =>
        date is null
            ? "—"
            : date.Value.AddHours(3).ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
}