using Saldo360.Helpers;
using Saldo360.Models;
using Saldo360.Services;

namespace Saldo360.Views;

public partial class MovementDetailPage : ContentPage
{
    readonly FinanceStore _store;
    readonly Movement _item;

    public MovementDetailPage(FinanceStore store, Movement item)
    {
        InitializeComponent();
        _store = store;
        _item = item;

        ResponsiveLayout.Center(this, Root);

        IconLabel.Text = item.Icon;
        DescriptionLabel.Text = item.Description;
        AmountLabel.Text = item.AmountText;
        AmountLabel.TextColor = item.AmountColor;
        TypeLabel.Text = item.Type;
        CategoryLabel.Text = item.Category;
        DateLabel.Text = item.Date.ToString("dd/MM/yyyy");
    }

    async void OnDelete(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlert("Excluir", "Deseja excluir este registro?", "Excluir", "Cancelar");
        if (!confirm) return;

        _store.RemoveMovement(_item);
        await Navigation.PopAsync();
    }
}
