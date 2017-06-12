using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Compass;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using FFImageLoading;

namespace Waypoint
{
	public partial class MainPage : ContentPage
	{
		private const int HEIGHT_REQUEST = 75;
		private const int WIDTH_REQUEST = 75;

        private static readonly List<string> PRELOADED_FILES = new List<string>()
        {
            "Waypoint.maps.scu.png", "Waypoint.maps.yosemite.jpg", "Waypoint.maps.montalvo.png", "Waypoint.maps.ucsd.jpg"
        };

		public MainPage()
		{
			InitializeComponent();

            // Get preloaded files as Maps
            Assembly assembly = typeof(MainPage).GetTypeInfo().Assembly;
            List<Task<Map>> mapTasks = PRELOADED_FILES
                .Select(uri => assembly.GetManifestResourceStream(uri)) // Convert file URI to Stream
                .Select(stream => Map.FromStream(stream)) // Convert Stream to Map object
            .ToList();

            // Wait for all Map objects to be constructed
            Task.WhenAll(mapTasks).ContinueWith(async task =>
            {
                // Get list of Maps
                var maps = new List<Map>(await task);

                // Run on UI thread to update view
                Device.BeginInvokeOnMainThread(() =>
                {
                    foreach (var map in maps)
                    {
                        var image = new Image
                        {
                            Source = ImageSource.FromStream(() =>
                            {
                                // Hard copy stream to a new MemoryStream
                                using (var memStream = new MemoryStream())
                                {
                                    map.Image.Seek(0, SeekOrigin.Begin);
                                    map.Image.CopyTo(memStream);
                                    return new MemoryStream(memStream.ToArray());
                                }
                            }),
                            HeightRequest = HEIGHT_REQUEST,
                            WidthRequest = WIDTH_REQUEST
                        };
                        image.GestureRecognizers.Add(new TapGestureRecognizer
                        {
                            Command = new Command(() => map_Tapped(map)),
                        });
                        wrapLayout.Children.Add(image);
                    }
                });
            });
        }

		private async void map_Tapped(Map map)
		{
			await Navigation.PushAsync(new MapViewer(map.Image, map.Size));
		}

		private async void UploadPictureButton_Clicked(object sender, EventArgs e)
		{
			if (!CrossMedia.Current.IsPickPhotoSupported)
			{
				await DisplayAlert("No upload", "Picking a photo is not supported", "OK");
				return;
			}

			var file = await CrossMedia.Current.PickPhotoAsync();
			if (file == null)
			{
				await DisplayAlert("Error", "No photo selected", "OK");
				return;
			}

			var _ = addImage(file);
		}

		private async void TakePictureButton_Clicked(object sender, EventArgs e)
		{
			await CrossMedia.Current.Initialize();

			if (!CrossMedia.Current.IsCameraAvailable)
			{
				await DisplayAlert("No Camera", "camera is not available.", "OK");
				return;
			}

			if (!CrossMedia.Current.IsTakePhotoSupported)
			{
				await DisplayAlert("No Camera", "take photo is not supported", "OK");
				return;
			}

			var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
			{
				SaveToAlbum = true,
				Name = "test.jpg"
			});

			if (file == null)
			{
				await DisplayAlert("Error", "No photo taken", "OK");
				return;
			}

			var _ = addImage(file);
		}

		private async Task addImage(MediaFile file)
		{
			Image image = new Image
			{
				Source = ImageSource.FromStream(() => file.GetStream()),
                HeightRequest = HEIGHT_REQUEST,
				WidthRequest = WIDTH_REQUEST
			};

            Map map = await Map.FromStream(file.GetStream());
			image.GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = new Command(() => map_Tapped(map)),
			});

			wrapLayout.Children.Add(image);
		}

        // Internal model class representing a Map
        private class Map
        {
            private readonly Stream image;
            public Stream Image { get { return image; } }

            private readonly Size size;
            public Size Size { get { return size; } }

            private Map(Stream image, Size size)
            {
                this.image = image;
                this.size = size;
            }

            public static async Task<Map> FromStream(Stream image)
            {
                // Hard copy stream to get the image size and then create and return a map from it
                using (var memStream = new MemoryStream())
                {
                    image.CopyTo(memStream);
                    return new Map(image, await ImageUtils.GetImageSize(new MemoryStream(memStream.ToArray())));
                }
            }
        }
    }
}
