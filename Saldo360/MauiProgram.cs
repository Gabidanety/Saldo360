using Microsoft.Extensions.Logging;
using Saldo360.Services;
using Saldo360.Views;

namespace Saldo360;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Serviços: uma única instância para o app todo
        builder.Services.AddSingleton<FinanceStore>();
        builder.Services.AddSingleton<CurrencyApiService>();
        builder.Services.AddSingleton<IbgeApiService>();
        builder.Services.AddSingleton<SimulationService>();
        builder.Services.AddSingleton<ComparisonService>();

        // Páginas das abas (recebem os serviços pelo construtor)
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<MovementsPage>();
        builder.Services.AddTransient<PlanningPage>();
        builder.Services.AddTransient<EconomyPage>();
        builder.Services.AddTransient<SimulatorPage>();
        builder.Services.AddTransient<ComparisonPage>();
        builder.Services.AddTransient<AboutPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
