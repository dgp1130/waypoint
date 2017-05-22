using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Waypoint
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapViewer : ContentPage
    {
        public MapViewer(Stream map)
        {
            InitializeComponent();

            // Use associated ViewModel for bindings
            this.BindingContext = new MapViewerViewModel(map);
        }
    }
}