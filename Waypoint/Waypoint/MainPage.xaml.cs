using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Media;

namespace Waypoint
{
    public partial class MainPage : ContentPage
    {
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
			Image Image1 = new Image
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

		private void UploadPictureButton_Clicked(object sender, EventArgs e)
		{

		}

		private async void TakePictureButton_Clicked(object sender, EventArgs e)
		{
			await CrossMedia.Current.Initialize();

			if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
			{
				await DisplayAlert("No Camera", "No Camera available.", "OK");
				return;
			}
		}
    }
}
