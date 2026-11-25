using Saldo360.Helpers;
using Saldo360.Models;
using Saldo360.Services;

namespace Saldo360.Views;

public partial class MovementsPage : ContentPage
{
    readonly FinanceStore _store;

    public MovementsPage(FinanceStore store)
    {
        InitializeComponent();
        _store = store;
        ResponsiveLayout.Center(this, Root);
    }

    // Também roda quando voltamos da tela de detalhes (ex.: depois de excluir)
    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadItems();
    }

    void LoadItems() => MovementsList.ItemsSource = _store.GetMovements();

    async void OnAdd(object? sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new AddMovementPage(_store, LoadItems));
    }

    // Navegação 2: Stack (abre os detalhes empilhando uma página, com botão voltar)
    async void OnItemTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is Movement movement)
            await Navigation.PushAsync(new MovementDetailPage(_store, movement));
    }
}
