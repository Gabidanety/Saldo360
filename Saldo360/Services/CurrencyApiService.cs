using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.Graphics;
using Saldo360.Helpers;

namespace Saldo360.Services;

// API 1: AwesomeAPI — cotação de dólar e euro em reais (não precisa de chave).
public class CurrencyApiService
{
    const string Url = "https://economia.awesomeapi.com.br/json/last/USD-BRL,EUR-BRL";

    static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };
    static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public async Task<List<CurrencyQuote>> GetQuotesAsync()
    {
        // A resposta é um objeto: { "USDBRL": {...}, "EURBRL": {...} }
        var data = await Http.GetFromJsonAsync<Dictionary<string, CurrencyQuote>>(Url, Options);
        return data?.Values.ToList() ?? new();
    }

    // Histórico diário de um par de moedas em um período (usado pelo simulador).
    // A API devolve do dia mais recente para o mais antigo.
    public async Task<List<CurrencyQuote>> GetHistoryAsync(string pair, DateTime start, DateTime end)
    {
        var from = start.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var to = end.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var url = $"https://economia.awesomeapi.com.br/json/daily/{pair}/360?start_date={from}&end_date={to}";

        return await Http.GetFromJsonAsync<List<CurrencyQuote>>(url, Options) ?? new();
    }

    // Cotação de fechamento de cada mês do período, usada no comparativo.
    // Devolve um dicionário "AAAAMM" -> valor (ex.: "202510" -> 5,34).
    public async Task<Dictionary<string, decimal>> GetMonthlyClosesAsync(string pair, DateTime start, DateTime end)
    {
        var history = await GetHistoryAsync(pair, start, end);
        return ToMonthlyCloses(history);
    }

    public static Dictionary<string, decimal> ToMonthlyCloses(List<CurrencyQuote> history)
    {
        return history
            .Where(q => q.Date is not null && q.BidValue > 0)
            .GroupBy(q => q.Date!.Value.ToString("yyyyMM", CultureInfo.InvariantCulture))
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(q => q.Date).First().BidValue); // último dia com cotação do mês
    }
}

public class CurrencyQuote
{
    public string Code { get; set; } = "";       // "USD" ou "EUR"
    public string Name { get; set; } = "";
    public string Bid { get; set; } = "0";        // valor de compra (vem como texto)
    public string PctChange { get; set; } = "0";  // variação do dia em %
    public string Timestamp { get; set; } = "0";  // hora da cotação (segundos ou milissegundos)

    // Data da cotação no horário de Brasília (a API às vezes manda segundos, às vezes milissegundos)
    [JsonIgnore] public DateTime? Date
    {
        get
        {
            if (!long.TryParse(Timestamp, out var t) || t <= 0) return null;
            var time = t > 100_000_000_000 ? DateTimeOffset.FromUnixTimeMilliseconds(t) : DateTimeOffset.FromUnixTimeSeconds(t);
            return time.UtcDateTime.AddHours(-3);
        }
    }

    [JsonIgnore] public decimal BidValue => Parse(Bid);
    [JsonIgnore] public decimal ChangeValue => Parse(PctChange);

    [JsonIgnore] public string RateText => Brl.Rate(BidValue);
    [JsonIgnore] public string ChangeText => (ChangeValue >= 0 ? "▲ " : "▼ ") + Brl.Percent(Math.Abs(ChangeValue));
    [JsonIgnore] public Color ChangeColor => ChangeValue >= 0 ? Palette.Primary : Palette.Expense;

    static decimal Parse(string text) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : 0;
}
