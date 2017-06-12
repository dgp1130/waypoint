using FFImageLoading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Waypoint
{
    public static class ImageUtils
    {
        // Read the Stream given as an image and get its file dimensions
        public static async Task<Size> GetImageSize(Stream stream)
        {
            // Create a new task source
            var taskSource = new TaskCompletionSource<Size>();

            // Load Stream as an image and callback when ready
            ImageService.Instance.LoadStream((token) => Task.FromResult(stream)).Success((imageInfo, result) =>
            {
                // Set the task result as the original Size of the image
                taskSource.TrySetResult(new Size(imageInfo.OriginalWidth, imageInfo.OriginalHeight));
            }).Preload();

            // Wait for the Task to complete
            return await taskSource.Task;
        }
    }
}
