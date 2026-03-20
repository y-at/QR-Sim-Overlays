using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QRO.Views.Overlays
{
    public partial class WindDirectionCompassView : Window
    {
        public WindDirectionCompassView(WindDirectionCompassViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
            this.MouseLeftButtonDown += (s, e) =>
            {
                this.DragMove();
            };
            this.Topmost = true;
        }
    }
}
