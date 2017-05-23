using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SkiaSharp.Views.Forms;
using SkiaSharp;

namespace Waypoint
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapViewer : ContentPage
    {
        private readonly MapViewerViewModel viewModel;

        public MapViewer(Stream map, Size mapSize)
        {
            InitializeComponent();

            // Use associated ViewModel for bindings
            this.viewModel = new MapViewerViewModel(map, mapSize);
            this.BindingContext = viewModel;

            // When the list of references updates, redraw the screen
            viewModel.PropertyChanged += (sender, evt) =>
            {
                if (evt.PropertyName == "References")
                {
                    canvasView.InvalidateSurface();
                }
            };
        }

        // Paint the canvas with the image and its reference lines
        private void paint(object sender, SKPaintSurfaceEventArgs evt)
        {
            SKCanvas canvas = evt.Surface.Canvas;
            Size canvasSize = new Size(evt.Info.Width, evt.Info.Height);
            Size origSize = viewModel.MapSize;
            
            using (var paint = new SKPaint())
            {
                paint.Color = SKColors.Red;

                // Reset image
                canvas.Clear(SKColors.White);

                // Get image scale based on the smaller fit of the two dimensions
                double widthScale = canvasSize.Width / origSize.Width;
                double heightScale = canvasSize.Height / origSize.Height;
                double scale = widthScale < heightScale ? widthScale : heightScale;
                Debug.WriteLine($"Scale: {scale}");

                // Compute the size of the image to draw by scaling it to the appropriate size
                Size scaledSize = origSize * scale;

                // Compute the origin of the image by offsetting it to center within the view
                Point origin = widthScale < heightScale
                    ? new Point(0.0, Math.Abs(canvasSize.Height - scaledSize.Height) / 2.0) // Fit to smaller width
                    : new Point(Math.Abs(canvasSize.Width - scaledSize.Width) / 2.0, 0.0) // Fit to smaller height
                ;
                Debug.WriteLine($"Origin: {origin}");

                // Compute the rectangle the image will use to draw itself
                var rect = SKRect.Create((float) origin.X, (float) origin.Y,
                        (float) scaledSize.Width, (float) scaledSize.Height);
                Debug.WriteLine($"Image rect: {rect}");

                // Draw base map image
                viewModel.Map.Position = 0; // Reset to beginning of stream
                using (var skStream = new SKManagedStream(viewModel.Map))
                using (var bitmap = SKBitmap.Decode(skStream))
                {
                    canvas.DrawBitmap(bitmap, rect);
                }

                // Draw each reference line for debugging
                foreach (var reference in viewModel.References)
                {
                    drawReferenceLine(canvas, origin, scaledSize, reference.scale(scale), paint);
                }
            }
        }

        // Draw a reference line on the canvas for debugging purposes
        private static void drawReferenceLine(SKCanvas canvas, Point origin, Size size, ReferenceRatio reference, SKPaint paint)
        {
            switch (reference.Axis)
            {
                case ReferenceRatio.PolarAxis.Latitude: // Draw horizontal latitude line
                    float y = (float) (origin.Y + reference.Pixel);
                    canvas.DrawLine((float) origin.X, y, (float) (origin.X + size.Width), y, paint);
                    canvas.DrawText(reference.Polar.ToString(), (float) ((size.Width / 2.0f) + origin.X), y, paint);
                    break;
                case ReferenceRatio.PolarAxis.Longitude: // Draw vertical longitude line
                    float x = (float) (origin.X + reference.Pixel);
                    canvas.DrawLine(x, (float) origin.Y, x, (float) (origin.Y + size.Height), paint);
                    canvas.DrawText(reference.Polar.ToString(), x, (float) ((size.Height / 2.0f) + origin.Y), paint);
                    break;
                default: throw new ArgumentException($"Unknown polar axis: {reference.Axis}. When did that get invented?!");
            }
        }
    }
}