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
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;


namespace Waypoint
{
    public partial class MainPage : ContentPage
    {
		Image Image1;		 
        public MainPage()
        {
            InitializeComponent();

            Padding = new Thickness(0, 20, 0, 0);

			Label testOutput = new Label 
			{
				Text = "test output"
			};

			//StackLayout 

			List<Frame> frames = new List<Frame>();
			List<String> urls = new List<String>();

			TapGestureRecognizer tapGesture = new TapGestureRecognizer();
			tapGesture.Tapped += viewerButton_Clicked;

			urls.Add("http://www.yosemite.ca.us/maps/yosemite_national_park_map.jpg");
			urls.Add("http://sites.ieee.org/scv-eds/files/2013/06/Picture1.jpg");
			urls.Add("https://s-media-cache-ak0.pinimg.com/originals/72/a7/01/72a70155020b7a0663b7310058b68ef6.jpg");
			urls.Add("http://media.montalvoarts.org/uploads/images/2007/October/grounds129.png");
			urls.Add("http://www.mappery.com/maps/UC-San-Diego-Map.jpg");

			var grid = new Grid();
			grid.BackgroundColor = Color.White;
			grid.RowDefinitions.Add(new RowDefinition { Height = 100});
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 100 });
			grid.RowDefinitions.Add(new RowDefinition { Height =100});
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 100 });
			grid.RowDefinitions.Add(new RowDefinition { Height = 100});
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 100 });

			for (int i = 0; i < urls.Count; i++)
			{
				frames.Add(new Frame
				{
					Content = new StackLayout{
						Children = {
							new Image { Source = ImageSource.FromUri(new Uri(urls[i])) }
						}
					}
				});
				frames[i].GestureRecognizers.Add(tapGesture);
			}

			grid.Children.Add(frames[0], 0, 0);
	        grid.Children.Add(frames[1], 1, 0);
	        grid.Children.Add(frames[2], 2, 0);
	        grid.Children.Add(frames[3], 0, 1);
	        grid.Children.Add(frames[4], 1, 1);

	        Content = new ScrollView
	        {

	            VerticalOptions = LayoutOptions.FillAndExpand,
	            HorizontalOptions = LayoutOptions.FillAndExpand,
	            Content = grid,


	        };

            // Temporary button to navigate to MapViewer
            // When we have a grid of images, tapping one should perform the same action
            Button viewerButton = new Button
            {
                Text = "View map",
				VerticalOptions = LayoutOptions.FillAndExpand,
	            HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            viewerButton.Clicked += viewerButton_Clicked;

			Button TakePictureButton = new Button
			{
				Text = "Take from camera",
				VerticalOptions = LayoutOptions.FillAndExpand,
	            HorizontalOptions = LayoutOptions.FillAndExpand,
			};
			TakePictureButton.Clicked += TakePictureButton_Clicked;
			Button UploadPictureButton = new Button
			{
				Text = "Pick a photo",
				VerticalOptions = LayoutOptions.FillAndExpand,
	            HorizontalOptions = LayoutOptions.FillAndExpand,
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
					new ScrollView
					{
						VerticalOptions = LayoutOptions.FillAndExpand,
						HorizontalOptions = LayoutOptions.FillAndExpand,
						Content = grid,
					},
					new StackLayout {
						Orientation = StackOrientation.Horizontal,
						Children = {
							viewerButton,
							TakePictureButton,
							UploadPictureButton,
						}
					}
					//Image1,
					//CompassImage,
					//label
				}
			};
			//this.Content = grid;

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
            Assembly assembly = typeof(MainPage).GetTypeInfo().Assembly;
            using (Stream stream = assembly.GetManifestResourceStream("Waypoint.test_case_scu.jpg"))
            {
                await Navigation.PushAsync(new MapViewer(stream, new Size(1313, 1135) /* hard code size temporarily */));
            }
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
