using Saldo360.Helpers;
using Saldo360.Services;

namespace Saldo360.Views;

public partial class ComparisonPage : ContentPage
{
    readonly ComparisonService _service;
    bool _loaded;

    public ComparisonPage(ComparisonService service)
    {
        InitializeComponent();
        _service = service;
        ResponsiveLayout.Center(this, Root);
    }

    // Busca as duas APIs na primeira vez que a tela abre
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_loaded)
            await LoadAsync();
    }

    async Task LoadAsync()
    {
        Busy.IsRunning = true;
        StatusLabel.Text = "Buscando IPCA (IBGE) e dólar (AwesomeAPI)...";
        try
        {
            var result = await _service.LoadAsync();

            BindableLayout.SetItemsSource(RowsList, result.Months);

            HeadlineLabel.Text = result.MonthsWithUsd == 0
                ? "Cotações do dólar indisponíveis agora"
                : $"O dólar subiu mais que a inflação em {result.MonthsUsdAbove} de {result.MonthsWithUsd} meses";

            AccumulatedLabel.Text = result.UsdAccumulated is decimal usd
                ? $"Acumulado: inflação {Brl.SignedPercent(result.IpcaAccumulated * 100m)} • dólar {Brl.SignedPercent(usd * 100m)}"
                : $"Acumulado da inflação: {Brl.SignedPercent(result.IpcaAccumulated * 100m)}";

            SummaryCard.IsVisible = true;
            TableCard.IsVisible = true;
            StatusLabel.Text = $"Atualizado às {DateTime.Now:HH:mm}";
            _loaded = true;
        }
        catch
        {
            StatusLabel.Text = "Não foi possível carregar. Confira a conexão e tente de novo.";
        }
        finally
        {
            Busy.IsRunning = false;
        }
    }

    async void OnRefresh(object? sender, EventArgs e) => await LoadAsync();
}
