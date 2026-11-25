using Microsoft.Maui.Controls.Shapes;
using Saldo360.Helpers;

namespace Saldo360.Controls;

// Botão "☰" que abre o menu lateral (Flyout). Usado no topo das telas principais.
public class MenuButton : Border
{
    public MenuButton()
    {
        WidthRequest = 44;
        HeightRequest = 44;
        StrokeThickness = 0;
        BackgroundColor = Palette.PrimarySoft;
        StrokeShape = new RoundRectangle { CornerRadius = 14 };

        Content = new Label
        {
            Text = "☰",
            FontSize = 22,
            TextColor = Palette.Primary,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            if (Shell.Current is not null)
                Shell.Current.FlyoutIsPresented = true;
        };
        GestureRecognizers.Add(tap);

        SemanticProperties.SetDescription(this, "Abrir menu");
    }
}
