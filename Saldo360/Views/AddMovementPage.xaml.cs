using Saldo360.Helpers;
using Saldo360.Models;
using Saldo360.Services;

namespace Saldo360.Views;

public partial class AddMovementPage : ContentPage
{
    readonly FinanceStore _store;
    readonly Action _onSaved;
    string _type = Categories.Expense;

    public AddMovementPage(FinanceStore store, Action onSaved)
    {
        InitializeComponent();
        _store = store;
        _onSaved = onSaved;

        ResponsiveLayout.Center(this, Root);

        CategoryPicker.ItemsSource = Categories.All;
        CategoryPicker.SelectedItem = "Outros";
        DateField.Date = DateTime.Today;
        SelectType(Categories.Expense);
    }

    void OnExpenseClicked(object? sender, EventArgs e) => SelectType(Categories.Expense);
    void OnIncomeClicked(object? sender, EventArgs e) => SelectType(Categories.Income);

    // Destaca o botão do tipo escolhido
    void SelectType(string type)
    {
        _type = type;
        Paint(ExpenseButton, type == Categories.Expense);
        Paint(IncomeButton, type == Categories.Income);
    }

    static void Paint(Button button, bool selected)
    {
        button.BackgroundColor = selected ? Palette.Primary : Palette.PrimarySoft;
        button.TextColor = selected ? Colors.White : Palette.Primary;
    }

    async void OnSave(object? sender, EventArgs e)
    {
        ErrorLabel.Text = "";

        if (string.IsNullOrWhiteSpace(DescriptionEntry.Text))
        {
            ErrorLabel.Text = "Informe uma descrição.";
            return;
        }

        if (!Brl.TryParse(AmountEntry.Text, out var amount) || amount <= 0)
        {
            ErrorLabel.Text = "Informe um valor maior que zero (ex.: 45,90).";
            return;
        }

        var date = DateField.Date is DateTime picked ? picked : DateTime.Today;

        _store.AddMovement(new Movement
        {
            Description = DescriptionEntry.Text.Trim(),
            Amount = amount,
            Type = _type,
            Category = CategoryPicker.SelectedItem?.ToString() ?? "Outros",
            Date = date
        });

        _onSaved();
        await Navigation.PopModalAsync();
    }

    async void OnCancel(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
