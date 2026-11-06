using System.Text.RegularExpressions;

namespace ApplicationsApi.Documents;

public static class TemplateEngine
{
    private static readonly Regex Placeholder =
        new(@"\{\{\s*([\w.\-]+)\s*\}\}", RegexOptions.Compiled);

    public static string Render(
        string template,
        IReadOnlyDictionary<string, string> values,
        IReadOnlySet<string>? preserve = null) =>
        Placeholder.Replace(template, match =>
        {
            var name = match.Groups[1].Value;

            if (preserve?.Contains(name) == true)
                return match.Value;

            return values.TryGetValue(name, out var value) ? value : "—";
        });
}
