namespace Saldo360.Models;

// Tipos e categorias usados no app, com o ícone (emoji) de cada uma.
public static class Categories
{
    public const string Income = "Receita";
    public const string Expense = "Despesa";

    static readonly Dictionary<string, string> Icons = new()
    {
        ["Alimentação"] = "🍽️",
        ["Transporte"] = "🚌",
        ["Moradia"] = "🏠",
        ["Educação"] = "📚",
        ["Saúde"] = "💊",
        ["Lazer"] = "🎮",
        ["Assinaturas"] = "📺",
        ["Salário"] = "💰",
        ["Outros"] = "🧾",
    };

    // Todas as categorias (para escolher ao lançar)
    public static string[] All => Icons.Keys.ToArray();

    // Só as categorias de gasto (para definir limites)
    public static string[] ExpenseNames => Icons.Keys.Where(c => c != "Salário").ToArray();

    public static string IconOf(string category) =>
        Icons.TryGetValue(category, out var icon) ? icon : "🧾";
}
