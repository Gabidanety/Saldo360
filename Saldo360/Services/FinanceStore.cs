using System.Text.Json;
using Microsoft.Maui.Storage;
using Saldo360.Models;

namespace Saldo360.Services;

public record MonthSummary(decimal Income, decimal Expenses)
{
    public decimal Balance => Income - Expenses;
}

// Guarda os dados do app (sem banco de dados).
// Tudo fica em listas na memória e é salvo como JSON em Preferences,
// então os dados continuam lá quando o app é fechado e aberto de novo.
public class FinanceStore
{
    const string MovementsKey = "movements";
    const string BudgetsKey = "budgets";
    const string GoalKey = "goal";

    readonly List<Movement> _movements;
    readonly List<Budget> _budgets;
    Goal? _goal;

    public FinanceStore()
    {
        _movements = Load<List<Movement>>(MovementsKey) ?? new();
        _budgets = Load<List<Budget>>(BudgetsKey) ?? new();
        _goal = Load<Goal>(GoalKey);
    }

    // ---------- Movimentações ----------

    public List<Movement> GetMovements() =>
        _movements.OrderByDescending(m => m.Date).ThenByDescending(m => m.Id).ToList();

    public void AddMovement(Movement item)
    {
        item.Id = _movements.Count == 0 ? 1 : _movements.Max(m => m.Id) + 1;
        _movements.Add(item);
        Save(MovementsKey, _movements);
    }

    public void RemoveMovement(Movement item)
    {
        _movements.RemoveAll(m => m.Id == item.Id);
        Save(MovementsKey, _movements);
    }

    // ---------- Resumos do mês ----------

    IEnumerable<Movement> ThisMonth()
    {
        var today = DateTime.Today;
        return _movements.Where(m => m.Date.Month == today.Month && m.Date.Year == today.Year);
    }

    public MonthSummary GetMonthSummary()
    {
        var month = ThisMonth().ToList();
        var income = month.Where(m => m.Type == Categories.Income).Sum(m => m.Amount);
        var expenses = month.Where(m => m.Type == Categories.Expense).Sum(m => m.Amount);
        return new MonthSummary(income, expenses);
    }

    public List<CategoryTotal> GetExpensesByCategory()
    {
        var totals = ThisMonth()
            .Where(m => m.Type == Categories.Expense)
            .GroupBy(m => m.Category)
            .Select(g => new CategoryTotal { Category = g.Key, Total = g.Sum(m => m.Amount) })
            .OrderByDescending(t => t.Total)
            .ToList();

        var max = totals.Count == 0 ? 0 : totals.Max(t => t.Total);
        foreach (var t in totals)
            t.Progress = max == 0 ? 0 : (double)(t.Total / max);

        return totals;
    }

    // ---------- Limites por categoria ----------

    public List<BudgetStatus> GetBudgetStatuses()
    {
        var spentByCategory = ThisMonth()
            .Where(m => m.Type == Categories.Expense)
            .GroupBy(m => m.Category)
            .ToDictionary(g => g.Key, g => g.Sum(m => m.Amount));

        return _budgets
            .Select(b => new BudgetStatus
            {
                Category = b.Category,
                Limit = b.Limit,
                Spent = spentByCategory.GetValueOrDefault(b.Category)
            })
            .OrderBy(b => b.Category)
            .ToList();
    }

    // Limite 0 (ou menor) remove o limite da categoria
    public void SetBudget(string category, decimal limit)
    {
        _budgets.RemoveAll(b => b.Category == category);
        if (limit > 0)
            _budgets.Add(new Budget { Category = category, Limit = limit });
        Save(BudgetsKey, _budgets);
    }

    // ---------- Meta de economia ----------

    public Goal? GetGoal() => _goal;

    public void SaveGoal(Goal? goal)
    {
        _goal = goal;
        Save(GoalKey, goal);
    }

    // ---------- Apagar tudo ----------

    public void ClearAll()
    {
        _movements.Clear();
        _budgets.Clear();
        _goal = null;
        Preferences.Default.Remove(MovementsKey);
        Preferences.Default.Remove(BudgetsKey);
        Preferences.Default.Remove(GoalKey);
    }

    // ---------- Salvar / carregar ----------

    static T? Load<T>(string key)
    {
        try
        {
            var json = Preferences.Default.Get(key, "");
            return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default; // dado corrompido: começa vazio
        }
    }

    static void Save(string key, object? value)
    {
        if (value is null) Preferences.Default.Remove(key);
        else Preferences.Default.Set(key, JsonSerializer.Serialize(value));
    }
}
