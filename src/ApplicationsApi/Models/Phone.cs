namespace ApplicationsApi.Models;

public static class Phone
{
    private static readonly string[] Codes = ["25", "29", "33", "44"];

    public static string? Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var digits = new string(raw.Where(char.IsDigit).ToArray());

        if (digits.StartsWith("375")) digits = digits[3..];
        else if (digits.StartsWith("80")) digits = digits[2..];

        if (digits.Length != 9 || !Codes.Contains(digits[..2])) return null;

        return "375" + digits;
    }
}
