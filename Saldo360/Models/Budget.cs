using System.Text.Json.Serialization;
using Microsoft.Maui.Graphics;
using Saldo360.Helpers;

namespace Saldo360.Models;

// Limite mensal de gastos de uma categoria (é o que fica salvo)
public class Budget
{
    public string Category { get; set; } = "Outros";
    public decimal Limit { get; set; }
}

// Limite + quanto já foi gasto no mês (usado só para mostrar na tela)
public class BudgetStatus
{
    public string Category { get; set; } = "";
    public decimal Limit { get; set; }
    public decimal Spent { get; set; }

    public string Title => $"{Categories.IconOf(Category)} {Category}";
    public string SummaryText => $"{Brl.Format(Spent)} de {Brl.Format(Limit)}";

    double Ratio => Limit <= 0 ? 0 : (double)(Spent / Limit);
    public double Progress => Math.Min(Ratio, 1);

    public string PercentText => Ratio > 1 ? "Limite ultrapassado" : $"{Math.Round(Ratio * 100)}% utilizado";

    public Color BarColor => Ratio > 1 ? Palette.Expense : Ratio >= 0.8 ? Palette.Warning : Palette.Primary;
}
