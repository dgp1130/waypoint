using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Media;
using Plugin.Media.Abstractions;

namespace Waypoint
{
    public partial class MainPage : ContentPage
    {
		Image Image1;
        public MainPage()
        {
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
            InitializeComponent();
			this.Content = new StackLayout
			{
				Children = {
					TakePictureButton,
					UploadPictureButton
				}
			};
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
				return;

			Image1.Source=ImageSource.FromStream(() => file.GetStream());
		}

		private async void TakePictureButton_Clicked(object sender, EventArgs e)
		{
			await CrossMedia.Current.Initialize();

			if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
			{
				await DisplayAlert("No Camera", "No Camera available.", "OK");
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
