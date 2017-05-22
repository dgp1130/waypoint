using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Waypoint
{
    class MapViewerViewModel : INotifyPropertyChanged
    {
        private const string VISION_API_KEY = "80ff8edbe81f48b6bad6cfbf5d5aab25";
        private const string VISION_API_ENDPOINT = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0";
        private const string LANGUAGE = "en";

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

        public MapViewerViewModel(Stream mapStream)
        {
            // Convert Stream to byte[] so data is not lost when the original Stream is closed
            byte[] bytes;
            using (var memStream = new MemoryStream())
            {
                mapStream.CopyTo(memStream);
                bytes = memStream.ToArray();
            }

            // Convert byte[] to MemoryStream to instantiate an ImageSource
            Map = ImageSource.FromStream(() => new MemoryStream(bytes));

            // Analyze image to recognize text
            VisionServiceClient client = new VisionServiceClient(VISION_API_KEY, VISION_API_ENDPOINT);
            client.RecognizeTextAsync(new MemoryStream(bytes), LANGUAGE).ContinueWith(async task =>
            {
                OcrResults results = null;
                try
                {
                    results = await task;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error analyzing image: {ex.Message}");
                    return; // Abort
                }

                // Print recognized text to the console
                Debug.WriteLine($"Got {results.Regions?.Count()} results");
                foreach (var region in results.Regions)
                {
                    // Concatenate each region of lines into a single string
                    string text = region.Lines.ToList()
                        .SelectMany(line => line.Words)
                        .Select(word => word.Text)
                        .Aggregate((word1, word2) => word1 + " " + word2)
                    ;

                    Debug.WriteLine($"Text: {text}");
                    Debug.WriteLine($"Location: {region.Rectangle.Left}x{region.Rectangle.Top}");
                }
            });
        }

        private void onPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
