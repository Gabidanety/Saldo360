using System.Text.Json.Serialization;
using Saldo360.Helpers;

namespace Saldo360.Models;

public class Goal
{
    public string Name { get; set; } = "Minha meta";
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }

    [JsonIgnore] public double Progress => TargetAmount <= 0 ? 0 : Math.Min((double)(CurrentAmount / TargetAmount), 1);
    [JsonIgnore] public string AmountText => $"{Brl.Format(CurrentAmount)} de {Brl.Format(TargetAmount)}";
    [JsonIgnore] public string PercentText => $"{Math.Round(Progress * 100)}% da meta";
}
