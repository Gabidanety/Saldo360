using Saldo360.Helpers;
using Saldo360.Services;

namespace Saldo360.Views;

public partial class DashboardPage : ContentPage
{
    readonly FinanceStore _store;
    readonly CurrencyApiService _currencyApi;
    bool _currencyLoaded;

    public DashboardPage(FinanceStore store, CurrencyApiService currencyApi)
    {
        InitializeComponent();
        _store = store;
        _currencyApi = currencyApi;
        ResponsiveLayout.Center(this, Root);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LoadSummary();

        if (!_currencyLoaded)
            await LoadCurrencyAsync();
    }

    void LoadSummary()
    {
        var summary = _store.GetMonthSummary();
        BalanceLabel.Text = Brl.Format(summary.Balance);
        IncomeLabel.Text = Brl.Format(summary.Income);
        ExpenseLabel.Text = Brl.Format(summary.Expenses);

        var categories = _store.GetExpensesByCategory();
        BindableLayout.SetItemsSource(CategoryList, categories);
        NoDataLabel.IsVisible = categories.Count == 0;
    }

    async Task LoadCurrencyAsync()
    {
        CurrencyStatus.Text = "Atualizando cotações...";
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
            _currencyLoaded = true;
        }
        catch
        {
            CurrencyStatus.Text = "Não foi possível carregar. Verifique sua conexão.";
        }
    }

    async void OnRefreshCurrency(object? sender, EventArgs e) => await LoadCurrencyAsync();

    // Navegação 3: Modal (a tela de lançamento abre por cima de tudo)
    async void OnAddMovement(object? sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new AddMovementPage(_store, LoadSummary));
    }
}
