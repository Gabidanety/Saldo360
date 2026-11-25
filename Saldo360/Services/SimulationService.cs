using Saldo360.Helpers;

namespace Saldo360.Services;

// Aqui as duas APIs "conversam":
//   1) o IBGE informa a inflação (IPCA) e o PERÍODO analisado;
//   2) esse período é enviado para a AwesomeAPI, que devolve o dólar naquelas datas;
//   3) com os dois resultados o simulador compara inflação x dólar e converte valores.
public class SimulationService
{
    readonly IbgeApiService _ibge;
    readonly CurrencyApiService _currency;

    public SimulationService(IbgeApiService ibge, CurrencyApiService currency)
    {
        _ibge = ibge;
        _currency = currency;
    }

    public async Task<MarketSnapshot> LoadAsync()
    {
        // Passo 1 (IBGE): inflação dos últimos meses
        var months = await _ibge.GetIpcaAsync();
        if (months.Count == 0)
            throw new InvalidOperationException("O IBGE não retornou dados.");

        var start = months[0].Month;                                // 1º dia do primeiro mês
        var end = months[^1].Month.AddMonths(1).AddDays(-1);        // último dia do último mês
        var inflation = months.Aggregate(1m, (total, m) => total * (1 + m.Value / 100m)) - 1m;

        // Passo 2 (AwesomeAPI): cotações de hoje
        var quotes = await _currency.GetQuotesAsync();
        var usd = quotes.FirstOrDefault(q => q.Code == "USD");
        var eur = quotes.FirstOrDefault(q => q.Code == "EUR");
        if (usd is null || eur is null)
            throw new InvalidOperationException("A AwesomeAPI não retornou as cotações.");

        // Passo 3 (AwesomeAPI de novo, agora com o período vindo do IBGE): dólar no início e no fim
        decimal? usdStart = null, usdEnd = null;
        try
        {
            var history = await _currency.GetHistoryAsync("USD-BRL", start, end);
            if (history.Count >= 2)
            {
                usdEnd = history[0].BidValue;       // dia mais recente do período
                usdStart = history[^1].BidValue;    // dia mais antigo do período
            }
        }
        catch
        {
            // Sem o histórico a simulação continua funcionando, só sem a comparação do dólar
        }

        return new MarketSnapshot
        {
            PeriodStart = start,
            PeriodEnd = end,
            InflationRate = inflation,
            UsdStart = usdStart,
            UsdEnd = usdEnd,
            UsdNow = usd.BidValue,
            EurNow = eur.BidValue
        };
    }
}

// Dados das duas APIs reunidos + as contas da simulação
public class MarketSnapshot
{
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }

    public decimal InflationRate { get; init; }   // IPCA acumulado no período (0,0231 = 2,31%)
    public decimal? UsdStart { get; init; }       // dólar no início do período
    public decimal? UsdEnd { get; init; }         // dólar no fim do período
    public decimal UsdNow { get; init; }          // dólar hoje
    public decimal EurNow { get; init; }          // euro hoje

    public bool HasUsdHistory => UsdStart.GetValueOrDefault() > 0 && UsdEnd.GetValueOrDefault() > 0;

    // Quanto o dólar variou no período (0,027 = +2,7%)
    public decimal? UsdChange => HasUsdHistory ? UsdEnd / UsdStart - 1 : null;

    // Variação do dólar descontada a inflação (a variação "real")
    public decimal? UsdRealChange => UsdChange is null ? null : (1 + UsdChange.Value) / (1 + InflationRate) - 1;

    public Simulation Simulate(decimal amount)
    {
        var corrected = amount * (1 + InflationRate);
        return new Simulation(
            amount,
            corrected,
            UsdNow > 0 ? corrected / UsdNow : 0,
            EurNow > 0 ? corrected / EurNow : 0);
    }
}

// Resultado da simulação para um valor
public record Simulation(decimal Original, decimal Corrected, decimal InUsd, decimal InEur);
