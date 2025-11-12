namespace OIS.Domain;

public static class Security
{
    public static string MaskGuid(Guid id)
    {
        var s = id.ToString("N");
        return s.Length >= 8 ? s[..8] : "***";
    }

    public static string MaskString(string? s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        if (s.Length <= 4) return "***";
        return s[..2] + "***" + s[^2..];
    }
}

