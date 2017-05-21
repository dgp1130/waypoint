using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Media;
using Plugin.Media.Abstractions;
//using Xam.Plugin.Media;
using Plugin.Compass;

namespace Waypoint
{
    public partial class MainPage : ContentPage
    {
		Image Image1;		 
        public MainPage()
        {
            InitializeComponent();

            Padding = new Thickness(0, 20, 0, 0);

            // Temporary button to navigate to MapViewer
            // When we have a grid of images, tapping one should perform the same action
            Button viewerButton = new Button
            {
                Text = "View map",
            };
            viewerButton.Clicked += viewerButton_Clicked;

			Button TakePictureButton = new Button
			{
				Text = "Take from camera",
			};
			TakePictureButton.Clicked += TakePictureButton_Clicked;
			Button UploadPictureButton = new Button
			{
				Text = "Pick a photo",
			};
			UploadPictureButton.Clicked += UploadPictureButton_Clicked;
			Image1 = new Image
			{
				HeightRequest = 240
			};
			Image CompassImage = new Image
			{
				HeightRequest = 50,
				Source = ImageSource.FromUri(new Uri("https://cdn2.iconfinder.com/data/icons/map-location-set/512/632503-compass_wind_rose-512.png"))
			};
			Label label = new Label
			{
				Text = "no heading yet"
			};

			this.Content = new StackLayout
			{
				Children = {
                    viewerButton,
					TakePictureButton,
					UploadPictureButton,
					Image1,
					CompassImage,
					label
				}
			};

			CrossCompass.Current.CompassChanged += (s, e) =>
			{
			    //Debug.WriteLine("*** Compass Heading = {0}", e.Heading);
			    
			    label.Text = $"Heading = {e.Heading}";
				//rotate compass image here
			   
			};

			CrossCompass.Current.Start();
        }

        private async void viewerButton_Clicked(object sender, EventArgs evt)
        {
            // Navigate to MapViewer with SCU test case
            await Navigation.PushAsync(new MapViewer(ImageSource.FromFile("test_case_scu.jpg")));
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
				await DisplayAlert("Error", "Photo was null", "OK");
				return;
			}
				//return;

			Image1.Source=ImageSource.FromStream(() => file.GetStream());
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
				return;

			Image1.Source=ImageSource.FromStream(() => file.GetStream());
		}
    }
}
