using Saldo360.Helpers;
using Saldo360.Services;

namespace Saldo360.Views;

public partial class AboutPage : ContentPage
{
    readonly FinanceStore _store;

    public AboutPage(FinanceStore store)
    {
        InitializeComponent();
        _store = store;
        ResponsiveLayout.Center(this, Root);
        VersionLabel.Text = $"Versão {AppInfo.Current.VersionString}";
    }

    async void OnClearData(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            "Apagar dados",
            "Isso remove todas as movimentações, limites e a meta. Deseja continuar?",
            "Apagar", "Cancelar");
        if (!confirm) return;

        _store.ClearAll();
        await DisplayAlert("Saldo360", "Dados apagados.", "OK");
    }
}
