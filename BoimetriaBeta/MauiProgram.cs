using BoimetriaBeta.Services;
using BoimetriaBeta.ViewModels;
using BoimetriaBeta.Views;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace BoimetriaBeta;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        RegisterServices(builder.Services);
        RegisterViewModels(builder.Services);
        RegisterViews(builder.Services);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IModelService, ModelService>();
        services.AddSingleton<IImagePickerService, ImagePickerService>();
        services.AddSingleton<IImageProcessingService, ImageProcessingService>();
        services.AddSingleton<IDialogService, DialogService>();
    }

    static void RegisterViewModels(IServiceCollection services)
    {
        services.AddSingleton<MainViewModel>();
        services.AddTransient<IdentificationViewModel>();
        services.AddTransient<ReportsViewModel>();
        services.AddTransient<SettingsViewModel>();
    }

    static void RegisterViews(IServiceCollection services)
    {
        services.AddSingleton<MainPage>();
        services.AddTransient<IdentificationPage>();
        services.AddTransient<ReportsPage>();
        services.AddTransient<SettingsPage>();
    }
}
