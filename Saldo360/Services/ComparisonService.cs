using System.Globalization;
using Microsoft.Maui.Graphics;
using Saldo360.Helpers;

namespace Saldo360.Services;

// Segunda funcionalidade em que as APIs trocam dados:
//   1) o IBGE entrega a lista de meses e o IPCA de cada um;
//   2) a lista de meses é enviada para a AwesomeAPI, que devolve o fechamento do dólar de cada mês;
//   3) o app junta os dois lados e compara, mês a mês, inflação x dólar.
public class ComparisonService
{
    readonly IbgeApiService _ibge;
    readonly CurrencyApiService _currency;

    public ComparisonService(IbgeApiService ibge, CurrencyApiService currency)
    {
        _ibge = ibge;
        _currency = currency;
    }

    public async Task<ComparisonResult> LoadAsync()
    {
        // Passo 1 (IBGE): meses e inflação de cada um
        var ipca = await _ibge.GetIpcaAsync();
        if (ipca.Count == 0)
            throw new InvalidOperationException("O IBGE não retornou dados.");

        // Passo 2 (AwesomeAPI): usa os meses do IBGE como intervalo.
        // Começa um mês antes para ter o fechamento do mês anterior (base da variação).
        var start = ipca[0].Month.AddMonths(-1);
        var end = ipca[^1].Month.AddMonths(1).AddDays(-1);
        var closes = await _currency.GetMonthlyClosesAsync("USD-BRL", start, end);

        // Passo 3: variação do dólar em cada mês = fechamento do mês / fechamento do mês anterior
        var months = new List<MonthComparison>();
        foreach (var item in ipca)
        {
            var key = item.Month.ToString("yyyyMM", CultureInfo.InvariantCulture);
            var previousKey = item.Month.AddMonths(-1).ToString("yyyyMM", CultureInfo.InvariantCulture);

            decimal? usdChange = null;
            if (closes.TryGetValue(key, out var close) && closes.TryGetValue(previousKey, out var previous) && previous > 0)
                usdChange = (close / previous - 1) * 100m;

            months.Add(new MonthComparison(item.Month, item.Value, usdChange));
        }

        return new ComparisonResult { Months = months };
    }
}

// Um mês com a inflação (IBGE) e a variação do dólar (AwesomeAPI), ambos em %
public record MonthComparison(DateTime Month, decimal Ipca, decimal? Usd)
{
    static readonly CultureInfo Pt = new("pt-BR");

    public string MonthText
    {
        get
        {
            var text = Month.ToString("MMM/yyyy", Pt);
            return char.ToUpper(text[0]) + text[1..];
        }
    }

    public string IpcaText => Brl.SignedPercent(Ipca);
    public string UsdText => Usd is null ? "—" : Brl.SignedPercent(Usd.Value);
    public Color UsdColor => Usd is null ? Palette.Muted : Usd >= 0 ? Palette.Primary : Palette.Expense;

    // Frase que resume o mês: qual foi maior, o dólar ou a inflação
    public string VerdictText => Usd is null
        ? "Sem cotação do dólar neste mês"
        : Usd > Ipca ? "Dólar subiu mais que a inflação" : "Inflação foi maior que o dólar";
}

public class ComparisonResult
{
    public List<MonthComparison> Months { get; init; } = new();

    public int MonthsWithUsd => Months.Count(m => m.Usd.HasValue);
    public int MonthsUsdAbove => Months.Count(m => m.Usd.HasValue && m.Usd > m.Ipca);

    // Acumulado dos meses (multiplica 1 + variação/100)
    public decimal IpcaAccumulated => Months.Aggregate(1m, (total, m) => total * (1 + m.Ipca / 100m)) - 1m;

    public decimal? UsdAccumulated =>
        Months.Count > 0 && MonthsWithUsd == Months.Count
            ? Months.Aggregate(1m, (total, m) => total * (1 + m.Usd!.Value / 100m)) - 1m
            : null;
}
