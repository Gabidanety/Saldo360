using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Saldo360.Helpers;

namespace Saldo360.Services;

// API 2: IBGE (SIDRA) — IPCA, a inflação oficial do Brasil.
// Tabela 1737, variável 63 = variação mensal (%), últimos 6 meses, Brasil.
public class IbgeApiService
{
    const string Url =
        "https://servicodados.ibge.gov.br/api/v3/agregados/1737/periodos/-6/variaveis/63?localidades=N1[all]";

    static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };
    static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    // Devolve os meses do mais antigo para o mais recente
    public async Task<List<IpcaMonth>> GetIpcaAsync()
    {
        var data = await Http.GetFromJsonAsync<List<IbgeVariable>>(Url, Options);
        return Parse(data);
    }

    public static List<IpcaMonth> Parse(List<IbgeVariable>? data)
    {
        // A série vem assim: { "202509": "0.48", "202510": "0.09", ... }
        var serie = data?.FirstOrDefault()?.Resultados.FirstOrDefault()?.Series.FirstOrDefault()?.Serie;

        var months = new List<IpcaMonth>();
        if (serie is null) return months;

        foreach (var item in serie.OrderBy(p => p.Key))
        {
            // Meses sem divulgação vêm como "-" ou "..." e são ignorados
            if (item.Key.Length != 6) continue;
            if (!decimal.TryParse(item.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)) continue;

            var year = int.Parse(item.Key[..4]);
            var month = int.Parse(item.Key[4..]);
            months.Add(new IpcaMonth(new DateTime(year, month, 1), value));
        }

        return months;
    }
}

public record IpcaMonth(DateTime Month, decimal Value)
{
    static readonly CultureInfo Pt = new("pt-BR");

    public string MonthText
    {
        get
        {
            var text = Month.ToString("MMMM 'de' yyyy", Pt);
            return char.ToUpper(text[0]) + text[1..];
        }
    }

    public string ValueText => Brl.Percent(Value);
}

// Classes que espelham o JSON do IBGE (só os campos que usamos)
public class IbgeVariable { public List<IbgeResult> Resultados { get; set; } = new(); }
public class IbgeResult { public List<IbgeSeries> Series { get; set; } = new(); }
public class IbgeSeries { public Dictionary<string, string> Serie { get; set; } = new(); }
