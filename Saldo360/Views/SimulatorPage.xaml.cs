using Saldo360.Helpers;
using Saldo360.Services;

namespace Saldo360.Views;

public partial class SimulatorPage : ContentPage
{
    readonly SimulationService _service;
    MarketSnapshot? _snapshot;

    public SimulatorPage(SimulationService service)
    {
        InitializeComponent();
        _service = service;
        ResponsiveLayout.Center(this, Root);
    }

    // Já busca os dados das duas APIs ao abrir a tela
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_snapshot is null)
            await LoadAsync();
    }

    async Task<bool> LoadAsync()
    {
        Busy.IsRunning = true;
        StatusLabel.Text = "Buscando IPCA (IBGE) e câmbio (AwesomeAPI)...";
        try
        {
            _snapshot = await _service.LoadAsync();
            PeriodLabel.Text = $"Período analisado: {_snapshot.PeriodStart:dd/MM/yyyy} a {_snapshot.PeriodEnd:dd/MM/yyyy}";
            StatusLabel.Text = "Dados carregados das duas APIs.";
            return true;
        }
        catch
        {
            StatusLabel.Text = "Não foi possível carregar os dados. Confira a conexão e tente de novo.";
            return false;
        }
        finally
        {
            Busy.IsRunning = false;
        }
    }

    async void OnSimulate(object? sender, EventArgs e)
    {
        ErrorLabel.Text = "";

        if (!Brl.TryParse(AmountEntry.Text, out var amount) || amount <= 0)
        {
            ErrorLabel.Text = "Informe um valor maior que zero (ex.: 1000,00).";
            return;
        }

        if (_snapshot is null && !await LoadAsync())
            return;

        ShowResult(_snapshot!, amount);
    }

    void ShowResult(MarketSnapshot data, decimal amount)
    {
        var result = data.Simulate(amount);

        // 1) Inflação (IBGE)
        CorrectedLabel.Text = Brl.Format(result.Corrected);
        InflationLabel.Text =
            $"{Brl.Format(amount)} + {Brl.SignedPercent(data.InflationRate * 100m)} de inflação (IPCA, IBGE)";

        // 2) Dólar no período (AwesomeAPI, usando as datas do IBGE)
        if (data.UsdChange is decimal change && data.UsdRealChange is decimal real)
        {
            UsdRangeLabel.Text = $"{Brl.Rate(data.UsdStart!.Value)} → {Brl.Rate(data.UsdEnd!.Value)}";
            UsdChangeLabel.Text = $"{Brl.SignedPercent(change * 100m)} no período";
            UsdChangeLabel.TextColor = change >= 0 ? Palette.Primary : Palette.Expense;
            UsdRealLabel.Text = real >= 0
                ? $"Descontada a inflação, o dólar ficou {Brl.Percent(real * 100m)} mais caro."
                : $"Descontada a inflação, o dólar ficou {Brl.Percent(-real * 100m)} mais barato.";
        }
        else
        {
            UsdRangeLabel.Text = "—";
            UsdChangeLabel.Text = "";
            UsdRealLabel.Text = "O histórico do dólar não está disponível agora.";
        }

        // 3) Valor corrigido convertido
        UsdValueLabel.Text = Brl.Foreign(result.InUsd, "US$");
        EurValueLabel.Text = Brl.Foreign(result.InEur, "€");

        ResultPanel.IsVisible = true;
    }
}
