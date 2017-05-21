using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Waypoint
{
    class MapViewerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ImageSource map;
        public ImageSource Map
        {
            get
            {
                return map;
            }

            private set
            {
                if (map == value) return;

                map = value;
                onPropertyChanged("Map");
            }
        }

        public MapViewerViewModel(ImageSource map)
        {
            Map = map;
        }

        private void onPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
