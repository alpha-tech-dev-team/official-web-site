using System.Text.RegularExpressions;

namespace ApplicationsApi.Documents;

public static class TemplateEngine
{
    // Ищет метки вида {{ имя }}
    private static readonly Regex Placeholder =
        new(@"\{\{\s*([\w.\-]+)\s*\}\}", RegexOptions.Compiled);

    public static string Render(string template, IReadOnlyDictionary<string, string> values) =>
        Placeholder.Replace(template, match =>
        {
            var name = match.Groups[1].Value;
            return values.TryGetValue(name, out var value) ? value : "—";
        });
}