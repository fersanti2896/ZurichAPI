namespace ZurichAPI.Shared.Helpers;

public static class TextHelper
{
    /// <summary>
    /// Convierte la primera letra de cada palabra en mayúscula.
    /// </summary>
    public static string CapitalizeEachWord(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        return string.Join(' ', input.Trim().ToLower().Split(' ')
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .Select(w => char.ToUpper(w[0]) + w.Substring(1)));
    }
}
