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


namespace Waypoint
{
    public partial class MainPage : ContentPage
    {
		Image Image1;		 
        public MainPage()
        {
            InitializeComponent();

            Padding = new Thickness(0, 20, 0, 0);

			List<Frame> frames = new List<Frame>();
			List<Map> maps = new List<Map>();

            Assembly assembly = typeof(MainPage).GetTypeInfo().Assembly;
            maps.Add(new Map(assembly.GetManifestResourceStream("Waypoint.maps.scu.jpg"), new Size(1313, 1135)));
            maps.Add(new Map(assembly.GetManifestResourceStream("Waypoint.maps.yosemite.jpg"), new Size(1527, 1972)));
            maps.Add(new Map(assembly.GetManifestResourceStream("Waypoint.maps.montalvo.png"), new Size(980, 840)));
            maps.Add(new Map(assembly.GetManifestResourceStream("Waypoint.maps.ucsd.jpg"), new Size(1005, 1102)));

			var grid = new Grid();
			grid.BackgroundColor = Color.White;
			grid.RowDefinitions.Add(new RowDefinition { Height = 100});
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 100 });
			grid.RowDefinitions.Add(new RowDefinition { Height = 100});
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 100 });
			grid.RowDefinitions.Add(new RowDefinition { Height = 100});
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 100 });

			foreach (var map in maps)
            {
                var frame = new Frame
                {
                    Content = new StackLayout
                    {
                        Children = {
                            new Image {
                                Source = ImageSource.FromStream(() => {
                                    // Hard copy stream to a new MemoryStream
                                    using (var memStream = new MemoryStream())
                                    {
                                        map.Image.CopyTo(memStream);
                                        return new MemoryStream(memStream.ToArray());
                                    }
                                }),
                            }
                        }
                    }
                };
                frame.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => map_Tapped(map)),
                });
                frames.Add(frame);
			}

			grid.Children.Add(frames[0], 0, 0);
	        grid.Children.Add(frames[1], 1, 0);
	        grid.Children.Add(frames[2], 2, 0);
	        grid.Children.Add(frames[3], 0, 1);

	        Content = new ScrollView
	        {

	            VerticalOptions = LayoutOptions.FillAndExpand,
	            HorizontalOptions = LayoutOptions.FillAndExpand,
	            Content = grid,
	        };

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
							TakePictureButton,
							UploadPictureButton,
						}
					}
				}
			};
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
