using System.Windows;
using System.Windows.Input;
using QRO.ViewModels.Overlays;
using QRO.Views.Overlays;

namespace QRO.Views
{
    public partial class InputTraces : Window
    {
        public InputTraces(InputTracesViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            this.MouseLeftButtonDown += (s, e) => this.DragMove();
            OverlayWindowInteraction.EnableResize(this, ResizeGripHandle);

            this.Topmost = true;
        }
    }
}