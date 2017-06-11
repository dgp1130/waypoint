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
//using Xamarin.FormsBook.Toolkit;


namespace Waypoint
{
	public partial class MainPage : ContentPage
	{
		int HEIGHT_REQUEST = 75;
		int WIDTH_REQUEST = 75;

		public MainPage()
		{
			InitializeComponent();

			List<Map> maps = new List<Map>();

            Assembly assembly = typeof(MainPage).GetTypeInfo().Assembly;
            maps.Add(new Map(assembly.GetManifestResourceStream("Waypoint.maps.scu.png"), new Size(905, 578)));
            maps.Add(new Map(assembly.GetManifestResourceStream("Waypoint.maps.yosemite.jpg"), new Size(1527, 1972)));
            maps.Add(new Map(assembly.GetManifestResourceStream("Waypoint.maps.montalvo.png"), new Size(980, 840)));
            maps.Add(new Map(assembly.GetManifestResourceStream("Waypoint.maps.ucsd.jpg"), new Size(1005, 1102)));

			foreach (var map in maps)
			{
				var image = new Image
				{
					Source = ImageSource.FromStream(() =>
					{
						// Hard copy stream to a new MemoryStream
						using (var memStream = new MemoryStream())
						{
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
			addImage(file);

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

			addImage(file);
		}

		private void addImage(MediaFile file)
		{
			Image image = new Image
			{
				Source = ImageSource.FromStream(() => file.GetStream()),
                HeightRequest = HEIGHT_REQUEST,
				WidthRequest = WIDTH_REQUEST
			};
			/*image.GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = new Command(() => map_Tapped(map)),
			});*/
			wrapLayout.Children.Add(image);
		}

        // Internal model class representing a Map
        private class Map
        {
            private readonly Stream image;
            public Stream Image { get { return image; } }

            private readonly Size size;
            public Size Size { get { return size; } }

            public Map(Stream image, Size size)
            {
                this.image = image;
                this.size = size;
            }
        }
    }
}
