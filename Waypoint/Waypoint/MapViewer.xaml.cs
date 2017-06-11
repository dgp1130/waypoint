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
using Plugin.Compass;
using System.Reflection;
using Plugin.Geolocator.Abstractions;

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
                switch (evt.PropertyName)
                {
                    case "Orientation":
                    case "References":
                    case "Heading":
                    case "Position":
                        canvasView.InvalidateSurface();
                        break;
                    default: // Do nothing
                        break;
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

                // Compute the size of the image to draw by scaling it to the appropriate size
                Size scaledSize = origSize * scale;

                // Compute the origin of the image by offsetting it to center within the view
                Point origin = widthScale < heightScale
                    ? new Point(0.0, Math.Abs(canvasSize.Height - scaledSize.Height) / 2.0) // Fit to smaller width
                    : new Point(Math.Abs(canvasSize.Width - scaledSize.Width) / 2.0, 0.0) // Fit to smaller height
                ;

                // Compute the rectangle the image will use to draw itself
                var rect = SKRect.Create((float) origin.X, (float) origin.Y,
                        (float) scaledSize.Width, (float) scaledSize.Height);

                // Draw base map image
                viewModel.Map.Position = 0; // Reset to beginning of stream
                using (var skStream = new SKManagedStream(viewModel.Map))
                using (var bitmap = SKBitmap.Decode(skStream))
                {
                    canvas.DrawBitmap(bitmap, rect);
                }

                // Draw compass in upper-right corner
                Assembly assembly = typeof(MapViewer).GetTypeInfo().Assembly;
                Size compassSize = new Size(200, 200);
                rotateCanvas(canvas, (float) -viewModel.Heading, (float) (canvasSize.Width - (compassSize.Width / 2.0)), (float) (compassSize.Height / 2.0), () =>
                {
                    // Load compass image from PCL assembly, because SkiaSharp does not like images in the platform-specific projects
                    using (Stream compassStream = assembly.GetManifestResourceStream("Waypoint.res.img.compass.png"))
                    using (var skStream = new SKManagedStream(compassStream))
                    using (var bitmap = SKBitmap.Decode(skStream))
                    {
                        canvas.DrawBitmap(bitmap, SKRect.Create((float)(canvasSize.Width - compassSize.Width), 0.0f,
                            (float)compassSize.Width, (float)compassSize.Height));
                    }
                });

                // Separate latitudes and longitudes
                List<ReferenceRatio> lngs = viewModel.References.Where(reference => reference.Axis == ReferenceRatio.PolarAxis.Longitude).ToList();
                List<ReferenceRatio> lats = viewModel.References.Where(reference => reference.Axis == ReferenceRatio.PolarAxis.Latitude).ToList();

                // Only compute waypoint if there is at least 2 lines of each and a current position
                Position position = viewModel.Position; // Cache locally so it does not change mid-computation
                if (lngs.Count >= 2 && lats.Count >= 2 && position != null)
                {
                    // Compute X ratio
                    double xRatio = lngs
                        .Pairs() // Pair lines
                        .Select(pair => (pair.Item1.Polar - pair.Item2.Polar) / (pair.Item1.Pixel - pair.Item2.Pixel)) // Map each pair to a ratio
                        .Average() // Average ratio into one value
                    ;

                    // Compute Y ratio
                    double yRatio = lats
                        .Pairs() // Pair lines
                        .Select(pair => (pair.Item1.Polar - pair.Item2.Polar) / (pair.Item1.Pixel - pair.Item2.Pixel)) // Map each pair to a ratio
                        .Average() // Average ratio into one value
                    ;

                    // Compute origin longitude
                    var lng = lngs[0]; // Use first longitude reference
                    double dLng = lng.Pixel * xRatio;
                    double lngOrigin = lng.Polar - dLng;

                    // Compute origin latitude
                    var lat = lats[0]; // Use first latitude reference
                    double dLat = lat.Pixel * yRatio;
                    double latOrigin = lat.Polar - dLat;

                    // Compute pixel coordinates for waypoint
                    double dWaypointLat = position.Latitude - latOrigin;
                    double dWaypointLng = position.Longitude - lngOrigin;
                    double waypointX = dWaypointLng / xRatio;
                    double waypointY = dWaypointLat / yRatio;

                    // Draw waypoint, scaled to match the drawn image
                    drawWaypoint(canvas, origin, new Point(waypointX * scale, waypointY * scale));
                }

                // Draw each reference line for debugging
                foreach (var reference in viewModel.References.ToList())
                {
                    drawReferenceLine(canvas, origin, scaledSize, reference.Scale(scale), paint);
                }
            }
        }

        // Rotates the canvas and invokes the given Action. Rotates the canvas back to its original position once complete
        private static void rotateCanvas(SKCanvas canvas, float degrees, float px, float py, Action callback)
        {
            canvas.RotateDegrees(degrees, px, py); // Rotate canvas
            callback(); // Invoke callback
            canvas.RotateDegrees(-degrees, px, py); // Unrotate canvas to original position
        }

        // Draw "You are here" waypoint
        private static void drawWaypoint(SKCanvas canvas, Point origin, Point position)
        {
            // Compute draw position
            Assembly assembly = typeof(MapViewer).GetTypeInfo().Assembly;
            Size waypointSize = new Size(100, 100);
            Point drawPosition = new Point(origin.X + position.X, origin.Y + position.Y);

            // Allocate memory
            using (Stream waypointStream = assembly.GetManifestResourceStream("Waypoint.res.img.waypoint.png"))
            using (var skStream = new SKManagedStream(waypointStream))
            using (var bitmap = SKBitmap.Decode(skStream))
            {
                // Draw to canvas
                canvas.DrawBitmap(bitmap, SKRect.Create((float) (drawPosition.X - (waypointSize.Width / 2.0)),
                    (float) (drawPosition.Y - waypointSize.Height), (float) waypointSize.Width, (float) waypointSize.Height));
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