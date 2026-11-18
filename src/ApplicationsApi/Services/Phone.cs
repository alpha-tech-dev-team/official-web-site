using System.Text.RegularExpressions;

namespace ApplicationsApi.Services;

public static class Phone
{
    private static readonly string[] MobileCodes = { "25", "29", "33", "44" };


    public static string? Normalize(string? raw)
    {
        var d = Regex.Replace(raw ?? "", @"\D", "");

        if (d.Length == 11 && d.StartsWith("80")) d = "375" + d[2..];
        else if (d.Length == 10 && d.StartsWith("0")) d = "375" + d[1..];
        else if (d.Length == 9) d = "375" + d;
        else if (!(d.Length == 12 && d.StartsWith("375"))) return null;

        return MobileCodes.Contains(d.Substring(3, 2)) ? d : null;
    }
}