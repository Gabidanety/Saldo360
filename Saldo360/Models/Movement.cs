using System.Text.Json.Serialization;
using Microsoft.Maui.Graphics;
using Saldo360.Helpers;

namespace Saldo360.Models;

public class Movement
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = Categories.Expense;
    public string Category { get; set; } = "Outros";
    public DateTime Date { get; set; } = DateTime.Today;

    // Propriedades só para exibir na tela (não são salvas)
    [JsonIgnore] public bool IsIncome => Type == Categories.Income;
    [JsonIgnore] public string Icon => Categories.IconOf(Category);
    [JsonIgnore] public string AmountText => (IsIncome ? "+ " : "- ") + Brl.Format(Amount);
    [JsonIgnore] public Color AmountColor => IsIncome ? Palette.Primary : Palette.Expense;
    [JsonIgnore] public string SubText => $"{Category} • {Date:dd/MM/yyyy}";
}
