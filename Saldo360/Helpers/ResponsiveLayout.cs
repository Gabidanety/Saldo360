namespace Saldo360.Helpers;

// Deixa o conteúdo ocupar a tela toda no celular,
// mas limita a largura e centraliza em tablets / janelas grandes.
public static class ResponsiveLayout
{
    public const double MaxWidth = 640;

    public static void Center(ContentPage page, View content)
    {
        content.HorizontalOptions = LayoutOptions.Center;

        page.SizeChanged += (_, _) =>
        {
            if (page.Width > 0)
                content.WidthRequest = Math.Min(page.Width, MaxWidth);
        };
    }
}
