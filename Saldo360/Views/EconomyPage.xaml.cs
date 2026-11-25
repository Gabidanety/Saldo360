using Saldo360.Helpers;
using Saldo360.Services;

namespace Saldo360.Views;

public partial class EconomyPage : ContentPage
{
    readonly CurrencyApiService _currencyApi;
    readonly IbgeApiService _ibgeApi;
    bool _loadedOnce;

    public EconomyPage(CurrencyApiService currencyApi, IbgeApiService ibgeApi)
    {
        InitializeComponent();
        _currencyApi = currencyApi;
        _ibgeApi = ibgeApi;
        ResponsiveLayout.Center(this, Root);
    }

    // Carrega as duas APIs na primeira vez que a aba abre
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_loadedOnce) return;
        _loadedOnce = true;

        await LoadCurrenciesAsync();
        await LoadIpcaAsync();
    }

    // ---------- API 1: AwesomeAPI (dólar e euro) ----------

    async Task LoadCurrenciesAsync()
    {
        CurrencyStatus.Text = "Atualizando...";
        try
        {
            var quotes = await _currencyApi.GetQuotesAsync();
            var usd = quotes.FirstOrDefault(q => q.Code == "USD");
            var eur = quotes.FirstOrDefault(q => q.Code == "EUR");

            if (usd is not null)
            {
                UsdValue.Text = usd.RateText;
                UsdChange.Text = usd.ChangeText;
                UsdChange.TextColor = usd.ChangeColor;
            }
            if (eur is not null)
            {
                EurValue.Text = eur.RateText;
                EurChange.Text = eur.ChangeText;
                EurChange.TextColor = eur.ChangeColor;
            }

            CurrencyStatus.Text = $"Atualizado às {DateTime.Now:HH:mm}";
        }
        catch
        {
            CurrencyStatus.Text = "Falha ao consultar as moedas. Confira a conexão e tente de novo.";
        }
    }

    async void OnLoadCurrencies(object? sender, EventArgs e) => await LoadCurrenciesAsync();

    // ---------- API 2: IBGE (IPCA) ----------

    async Task LoadIpcaAsync()
    {
        IpcaStatus.Text = "Consultando o IBGE...";
        try
        {
            var months = await _ibgeApi.GetIpcaAsync();

            if (months.Count == 0)
            {
                IpcaStatus.Text = "O IBGE não retornou dados agora. Tente novamente mais tarde.";
                return;
            }

            var last = months[^1];
            IpcaLastLabel.Text = last.ValueText;
            IpcaLastMonthLabel.Text = $"Variação de {last.MonthText}";

            // Acumulado: multiplica os índices mensais (1 + variação/100)
            var accumulated = months.Aggregate(1m, (total, m) => total * (1 + m.Value / 100m)) - 1m;
            IpcaAccumulatedLabel.Text = $"Acumulado em {months.Count} meses: {Brl.Percent(accumulated * 100m)}";

            // Lista do mais recente para o mais antigo
            BindableLayout.SetItemsSource(IpcaList, months.AsEnumerable().Reverse().ToList());

            IpcaStatus.Text = "Fonte: IBGE / SIDRA";
        }
        catch
        {
            IpcaStatus.Text = "Falha ao consultar o IBGE. Confira a conexão e tente de novo.";
        }
    }

    async void OnLoadIpca(object? sender, EventArgs e) => await LoadIpcaAsync();

    // Vai para o Simulador (item do menu lateral)
    async void OnOpenSimulator(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//SimulatorPage");

    // Vai para o comparativo mensal (item do menu lateral)
    async void OnOpenComparison(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//ComparisonPage");
}
