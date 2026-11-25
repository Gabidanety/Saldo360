using Saldo360.Helpers;
using Saldo360.Models;
using Saldo360.Services;

namespace Saldo360.Views;

public partial class PlanningPage : ContentPage
{
    readonly FinanceStore _store;

    public PlanningPage(FinanceStore store)
    {
        InitializeComponent();
        _store = store;
        ResponsiveLayout.Center(this, Root);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Refresh();
    }

    void Refresh()
    {
        // Limites
        var budgets = _store.GetBudgetStatuses();
        BindableLayout.SetItemsSource(BudgetList, budgets);
        NoBudgetLabel.IsVisible = budgets.Count == 0;

        // Meta
        var goal = _store.GetGoal();
        GoalLayout.IsVisible = goal is not null;
        NoGoalLayout.IsVisible = goal is null;

        if (goal is not null)
        {
            GoalNameLabel.Text = goal.Name;
            GoalProgress.Progress = goal.Progress;
            GoalAmountLabel.Text = goal.AmountText;
            GoalPercentLabel.Text = goal.PercentText;
        }
    }

    // ---------- Limites ----------

    async void OnAddBudget(object? sender, EventArgs e)
    {
        var category = await DisplayActionSheet("Categoria do limite", "Cancelar", null, Categories.ExpenseNames);
        if (string.IsNullOrEmpty(category) || category == "Cancelar") return;

        await EditBudgetAsync(category);
    }

    async void OnBudgetTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is BudgetStatus budget)
            await EditBudgetAsync(budget.Category);
    }

    async Task EditBudgetAsync(string category)
    {
        var text = await DisplayPromptAsync(
            $"Limite — {category}",
            "Quanto você quer gastar por mês? (0 remove o limite)",
            "Salvar", "Cancelar",
            placeholder: "Ex.: 300,00",
            keyboard: Keyboard.Numeric);

        if (text is null) return; // cancelou

        if (!Brl.TryParse(text, out var limit) || limit < 0)
        {
            await DisplayAlert("Saldo360", "Informe um valor válido (ex.: 300,00).", "OK");
            return;
        }

        _store.SetBudget(category, limit);
        Refresh();
    }

    // ---------- Meta ----------

    async void OnCreateGoal(object? sender, EventArgs e)
    {
        var name = await DisplayPromptAsync("Nova meta", "Qual o nome da meta?", "Próximo", "Cancelar",
            placeholder: "Ex.: Viagem");
        if (string.IsNullOrWhiteSpace(name)) return;

        var text = await DisplayPromptAsync("Nova meta", "Quanto você quer juntar?", "Criar", "Cancelar",
            placeholder: "Ex.: 2000,00", keyboard: Keyboard.Numeric);
        if (text is null) return;

        if (!Brl.TryParse(text, out var target) || target <= 0)
        {
            await DisplayAlert("Saldo360", "Informe um valor maior que zero.", "OK");
            return;
        }

        _store.SaveGoal(new Goal { Name = name.Trim(), TargetAmount = target });
        Refresh();
    }

    async void OnAddToGoal(object? sender, EventArgs e)
    {
        var goal = _store.GetGoal();
        if (goal is null) return;

        var text = await DisplayPromptAsync("Adicionar à meta", "Quanto você guardou?", "Adicionar", "Cancelar",
            placeholder: "Ex.: 150,00", keyboard: Keyboard.Numeric);
        if (text is null) return;

        if (!Brl.TryParse(text, out var value) || value <= 0)
        {
            await DisplayAlert("Saldo360", "Informe um valor maior que zero.", "OK");
            return;
        }

        goal.CurrentAmount += value;
        _store.SaveGoal(goal);
        Refresh();
    }

    async void OnRemoveGoal(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlert("Excluir meta", "Deseja excluir esta meta?", "Excluir", "Cancelar");
        if (!confirm) return;

        _store.SaveGoal(null);
        Refresh();
    }
}
