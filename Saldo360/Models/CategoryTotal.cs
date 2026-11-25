using Saldo360.Helpers;

namespace Saldo360.Models;

// Total gasto em uma categoria no mês (usado nas barras do Dashboard)
public class CategoryTotal
{
    public string Category { get; set; } = "";
    public decimal Total { get; set; }
    public double Progress { get; set; } // 0 a 1, em relação à maior categoria

    public string Title => $"{Categories.IconOf(Category)} {Category}";
    public string TotalText => Brl.Format(Total);
}
