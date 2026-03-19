using Prism.Ioc;
using Prism.Regions;
using QRO.Views;
using System.Windows;
using iRacing_Quick_Release.Services;
using iRacing_Quick_Release.ViewModels.Overlays;
using iRacing_Quick_Release.Views;
using iRacing_Quick_Release.Views.Overlays;
using QRO.Enumerations;
using QRO.Services;
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
            containerRegistry.RegisterForNavigation<RaceEngineerChatView, RaceEngineerChatView>();
            containerRegistry.RegisterForNavigation<InputTraces, InputTracesViewModel>();
            containerRegistry.RegisterForNavigation<AttitudeView, AttitudeViewModel>();
            containerRegistry.RegisterForNavigation<WindDirectionCompassView, WindDirectionCompassViewModel>();

            // Register services
            containerRegistry.RegisterSingleton<IOverlayWindowFactory, OverlayWindowFactory>();
            containerRegistry.RegisterSingleton<IOverlayWindowManagerService, OverlayWindowManagerService>();
            containerRegistry.RegisterSingleton<ITelemetryServiceManager, TelemetryServiceManager>();
            containerRegistry.RegisterSingleton<ISimulatorProcessMonitor, SimulatorProcessMonitor>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(UIRegions.SidebarRegion, typeof(SidebarMainView));
            regionManager.RegisterViewWithRegion(UIRegions.ContentRegion, typeof(OverlaySelectorView));

            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Stop and clean up telemetry service
            var telemetryManager = Container.Resolve<ITelemetryServiceManager>();
            telemetryManager?.Stop();

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
