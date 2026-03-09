using Prism.Ioc;
using Prism.Regions;
using QRO.Views;
using System.Windows;
using iRacing_Quick_Release.Services;
using iRacing_Quick_Release.ViewModels.Overlays;
using iRacing_Quick_Release.Views;
using iRacing_Quick_Release.Views.Overlays;
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
            containerRegistry.RegisterForNavigation<InputTraces, InputTracesViewModel>();
            containerRegistry.RegisterForNavigation<AttitudeView, AttitudeViewModel>();
            containerRegistry.RegisterForNavigation<WindDirectionCompassView, WindDirectionCompassViewModel>();

            // Register services
            containerRegistry.RegisterSingleton<IOverlayWindowFactory, OverlayWindowFactory>();
            containerRegistry.RegisterSingleton<IOverlayWindowManagerService, OverlayWindowManagerService>();
            containerRegistry.RegisterSingleton<ITelemetryServiceManager, TelemetryServiceManager>();
        }

        protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
        {
            base.ConfigureRegionAdapterMappings(regionAdapterMappings);
            // Add custom region adapters here if needed
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(UIRegions.SidebarRegion, typeof(SidebarMainView));
            regionManager.RegisterViewWithRegion(UIRegions.ContentRegion, typeof(OverlaySelectorView));
        }
    }
}
