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
using Plugin.Geolocator;
using static Waypoint.ReferenceRatio;
using Plugin.Geolocator.Abstractions;
using Plugin.Compass;

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

        private Position position;
        public Position Position
        {
            get { return position; }
            private set
            {
                if (position == value) return;

                position = value;
                onPropertyChanged("Position");
            }
        }

        private double heading;
        public double Heading
        {
            get { return heading; }
            private set
            {
                if (heading == value) return;

                heading = value;
                onPropertyChanged("Heading");
            }
        }

        public MapViewerViewModel(Stream mapStream, Size mapSize)
        {
            // Convert Stream to byte[] so data is not lost when the original Stream is closed
            byte[] bytes;
            mapStream.Position = 0; // Reset to beginning
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

            // Initialize compass
            initCompass();

            // Initialize geolocation
            var _ = initGeolocation();

            // Initialize OCR on provided image
            ocrImage(bytes).ContinueWith(async task =>
            {
                List<ReferenceRatio> references = await task ?? new List<ReferenceRatio>();
                foreach (var reference in references)
                {
                    References.Add(reference);
                }
            });
        }

        // Initialize the compass
        private void initCompass()
        {
            // Propagate changes in the compass
            CrossCompass.Current.CompassChanged += (sender, evt) => Heading = evt.Heading;

            // Start listening to compass events
            CrossCompass.Current.Start();
        }

        // Initialize the geolocation system
        private async Task initGeolocation()
        {
            IGeolocator locator = CrossGeolocator.Current;
            locator.DesiredAccuracy = 50;

            // Get initial position
            try
            {
                Position = await locator.GetPositionAsync();
                Debug.WriteLine($"Polled position: {Position.Latitude}x{Position.Longitude}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to poll current location.");
            }

            // Propagate changes in the location
            locator.PositionChanged += (sender, evt) => Position = evt.Position;

            // Start listening to location events
            await locator.StartListeningAsync(0, 0.0);
        }

        private async Task<List<ReferenceRatio>> ocrImage(byte[] bytes)
        {
            // Analyze image to recognize text
            VisionServiceClient client = new VisionServiceClient(VISION_API_KEY, VISION_API_ENDPOINT);
            OcrResults results = null;
            try
            {
                results = await client.RecognizeTextAsync(new MemoryStream(bytes), LANGUAGE, false /* detectRotation */);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error analyzing image: {ex.Message}");
                return null; // Abort
            }

            // Print recognized text to the console
            Debug.WriteLine($"Got {results.Regions?.Count()} results");
            List<ReferenceRatio> references = new List<ReferenceRatio>();
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
                        references.Add(reference);
                    }
                }
            }

            return references;
        }

        private void onPropertyChanged(string propertyName)
        {
            Device.BeginInvokeOnMainThread(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }
    }
}
