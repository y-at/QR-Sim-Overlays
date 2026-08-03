using Prism.Ioc;
using Prism.Regions;
using QRO.Views;
using System.Net.Http;
using System.Windows;
using QRO.ViewModels.Overlays;
using QRO.Views.Overlays;
using QRO.Enumerations;
using QRO.Services;
using QRO.Services.AI;
using QRO.Services.Factories;
using QRO.ViewModels;

namespace QRO
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register views for navigation
            containerRegistry.RegisterForNavigation<SidebarMainView, SidebarMainViewModel>();
            containerRegistry.RegisterForNavigation<OverlaySelectorView, OverlaySelectorViewModel>();
            containerRegistry.RegisterForNavigation<ControlPresetSelectView, ControlPresetSelectViewModel>();
            containerRegistry.RegisterForNavigation<GraphicsPresetSelectView, GraphicsPresetSelectViewModel>();
            containerRegistry.RegisterForNavigation<RaceEngineerChatView, RaceEngineerChatViewModel>();
            containerRegistry.RegisterForNavigation<InputTraces, InputTracesViewModel>();
            containerRegistry.RegisterForNavigation<AttitudeView, AttitudeViewModel>();
            containerRegistry.RegisterForNavigation<WindDirectionCompassView, WindDirectionCompassViewModel>();

            // Register services
            containerRegistry.RegisterSingleton<IStorageService, StorageService>();
            containerRegistry.RegisterSingleton<IOverlayWindowFactory, OverlayWindowFactory>();
            containerRegistry.RegisterSingleton<IOverlayWindowManagerService, OverlayWindowManagerService>();
            containerRegistry.RegisterSingleton<ITelemetryServiceManager, TelemetryServiceManager>();
            containerRegistry.RegisterSingleton<ISimulatorProcessMonitor, SimulatorProcessMonitor>();
            containerRegistry.RegisterSingleton<HttpClient>();
            containerRegistry.RegisterSingleton<ICredentialStorageService, CredentialStorageService>();
            containerRegistry.RegisterSingleton<IAIChatClientFactory, AIChatClientFactory>();
            containerRegistry.RegisterSingleton<IRaceEngineerContextBuilder, RaceEngineerContextBuilder>();
            containerRegistry.RegisterSingleton<OpenAIChatClient>();
            containerRegistry.RegisterSingleton<AnthropicChatClient>();
            containerRegistry.RegisterSingleton<IThemeService, ThemeService>();

            // Apply the saved theme now, before the shell window is created, to avoid a flash of the wrong theme.
            var themeService = Container.Resolve<IThemeService>();
            themeService.ApplyTheme(themeService.LoadSavedTheme());
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(UIRegions.SidebarRegion, typeof(SidebarMainView));
            regionManager.RegisterViewWithRegion(UIRegions.ContentRegion, typeof(OverlaySelectorView));

            var telemetryManager = Container.Resolve<ITelemetryServiceManager>();
            telemetryManager.AutoStartEnabled = true;
            
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Stop and clean up telemetry service
            var telemetryManager = Container.Resolve<ITelemetryServiceManager>();
            telemetryManager?.Dispose();

            // Close all overlay windows
            var overlayManager = Container.Resolve<IOverlayWindowManagerService>();
            if (overlayManager != null)
            {
                foreach (OverlayName openOverlays in overlayManager.CurrentOpenOverlays)
                {
                    overlayManager.CloseOverlay(openOverlays);
                }
            }

            base.OnExit(e);
        }
    }
}
