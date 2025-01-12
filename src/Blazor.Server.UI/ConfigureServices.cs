using System.Text.Json.Serialization;
using System.Text.Json;
using Blazor.Analytics;
using Blazor.Server.UI.Services;
using Blazor.Server.UI.Services.Navigation;
using Blazor.Server.UI.Services.Notifications;
using BlazorDownloadFile;
using MudBlazor.Services;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using MudExtensions.Services;
using CleanArchitecture.Blazor.Application.Features.Identity.Notification;
using Blazor.Server.UI.Components.Shared;

namespace Blazor.Server.UI;

public static class ConfigureServices
{
    public static IServiceCollection AddBlazorUIServices(this IServiceCollection services)
    {
        services.AddRazorPages();
        services.AddServerSideBlazor(
            options =>
            {
                options.DetailedErrors = true;
                options.DisconnectedCircuitMaxRetained = 100;
                options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
                options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
                options.MaxBufferedUnacknowledgedRenderBatches = 10;
            }
            ).AddHubOptions(options =>
            {
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                options.EnableDetailedErrors = false;
                options.HandshakeTimeout = TimeSpan.FromSeconds(15);
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.MaximumParallelInvocationsPerClient = 100;
                options.MaximumReceiveMessageSize = 64 * 1024;
                options.StreamBufferCapacity = 10;
            })
            .AddCircuitOptions(option => { option.DetailedErrors = true; });
        services.AddMudBlazorDialog();
        services.AddHotKeys2();
        services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
            config.SnackbarConfiguration.PreventDuplicates = false;
            config.SnackbarConfiguration.NewestOnTop = true;
            config.SnackbarConfiguration.ShowCloseIcon = true;
            config.SnackbarConfiguration.VisibleStateDuration = 4000;
            config.SnackbarConfiguration.HideTransitionDuration = 500;
            config.SnackbarConfiguration.ShowTransitionDuration = 500;
            config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
        });

        services.AddMudExtensions();
        services.AddScoped<LayoutService>();
        services.AddBlazorDownloadFile();
        services.AddScoped<IUserPreferencesService, UserPreferencesService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<INotificationService, InMemoryNotificationService>();
        services.AddGoogleAnalytics("G-PRYNCB61NV");
        services.AddHealthChecks();
        services.AddScoped<INotificationHandler<UpdateUserProfileCommand>, SideMenu>();
        services.AddScoped<INotificationHandler<UpdateUserProfileCommand>, UserMenu>();
        return services;
    }
}
