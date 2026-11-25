using System.Globalization;

namespace Saldo360.Helpers;

// Formata e lê valores em Real (R$) sempre no padrão brasileiro.
public static class Brl
{
    static readonly CultureInfo Pt = new("pt-BR");

    public static string Format(decimal value) => value.ToString("C", Pt);

    // Cotações precisam de mais casas decimais (ex.: R$ 5,2279)
    public static string Rate(decimal value) => value.ToString("C4", Pt);

    public static string Percent(decimal value) => value.ToString("0.00", Pt) + "%";

    // Ex.: 2.31 -> "+2,31%"  /  -0.11 -> "-0,11%"
    public static string SignedPercent(decimal percent) => (percent >= 0 ? "+" : "") + Percent(percent);

    // Valor em outra moeda. Ex.: Foreign(191.5m, "US$") -> "US$ 191,50"
    public static string Foreign(decimal value, string symbol) => $"{symbol} {value.ToString("N2", Pt)}";

    public static bool TryParse(string? text, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;

        text = text.Replace("R$", "").Trim();

        // "45,90" usa cultura pt-BR; "45.90" usa ponto decimal (invariante)
        var culture = text.Contains(',') ? Pt : CultureInfo.InvariantCulture;
        return decimal.TryParse(text, NumberStyles.Number, culture, out value);
    }
}
