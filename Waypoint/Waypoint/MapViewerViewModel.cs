using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using static Waypoint.ReferenceRatio;

namespace Waypoint
{
    class MapViewerViewModel : INotifyPropertyChanged
    {
        private const string VISION_API_KEY = "80ff8edbe81f48b6bad6cfbf5d5aab25";
        private const string VISION_API_ENDPOINT = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0";
        private const string LANGUAGE = "en";

        public event PropertyChangedEventHandler PropertyChanged;

        private Stream map;
        public Stream Map
        {
            get { return map; }
            private set
            {
                if (map == value) return;

                map = value;
                onPropertyChanged("Map");
            }
        }

        private Size mapSize;
        public Size MapSize
        {
            get { return mapSize; }
            private set
            {
                if (mapSize == value) return;

                mapSize = value;
                onPropertyChanged("MapSize");
            }
        }

        private ObservableCollection<ReferenceRatio> references = new ObservableCollection<ReferenceRatio>();
        public ObservableCollection<ReferenceRatio> References { get { return references; } }

        public MapViewerViewModel(Stream mapStream, Size mapSize)
        {
            // Convert Stream to byte[] so data is not lost when the original Stream is closed
            byte[] bytes;
            using (var memStream = new MemoryStream())
            {
                mapStream.CopyTo(memStream);
                bytes = memStream.ToArray();
            }

            // Convert byte[] to MemoryStream
            Map = new MemoryStream(bytes);
            MapSize = mapSize;

            // Bind observable collection changes to notify listening views
            References.CollectionChanged += (sender, evt) => onPropertyChanged("References");

            // Analyze image to recognize text
            VisionServiceClient client = new VisionServiceClient(VISION_API_KEY, VISION_API_ENDPOINT);
            client.RecognizeTextAsync(new MemoryStream(bytes), LANGUAGE, false /* detectRotation */).ContinueWith(async task =>
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
                    // Flatten lines of words into a list of words
                    List<Word> words = region.Lines.ToList().SelectMany(line => line.Words).ToList();
                    Debug.WriteLine($"Words: {words.Select(word => word.Text).Aggregate((word1, word2) => word1 + " " + word2)}");

                    // Look for latitude or longitude reference text
                    for (int i = 0; i < words.Count; ++i)
                    {
                        if (words[i].Text.ToLower() == "latitude" || words[i].Text.ToLower() == "longitude")
                        {
                            // Parse axis
                            PolarAxis axis = words[i].Text.ToLower() == "latitude" ? PolarAxis.Latitude : PolarAxis.Longitude;
                            
                            // Parse pixel value
                            int pixel;
                            try
                            {
                                pixel = int.Parse(words[i + 1].Text);
                            }
                            catch
                            {
                                Debug.WriteLine($"Failed to parse pixel value: {words[i + 1].Text}");
                                continue;
                            }

                            // Parse polar value
                            float polar;
                            try
                            {
                                polar = float.Parse(words[i + 2].Text);
                            }
                            catch
                            {
                                Debug.WriteLine($"Failed to parse polar value: {words[i + 2].Text}");
                                continue;
                            }

                            // Create reference ratio from detected values
                            var reference = new ReferenceRatio(pixel, polar, axis);
                            Debug.WriteLine($"Created reference: {reference}");
                            References.Add(reference);
                        }
                    }
                }
            });
        }

        private void onPropertyChanged(string propertyName)
        {
            Device.BeginInvokeOnMainThread(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }
    }
}
