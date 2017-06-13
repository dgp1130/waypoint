using PCLStorage;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waypoint
{
    /// <summary>
    /// Static utility class for accessing permanant storage
    /// </summary>
    public static class Storage
    {
        // Name of folder containing images
        private const string IMAGE_FOLDER = "images";

        // Retrieve the folder which contains all images in app data. The folder is automatically created if it does not already exist.
        private static async Task<IFolder> GetImagesFolder()
        {
            return await FileSystem.Current.LocalStorage.CreateFolderAsync(IMAGE_FOLDER, CreationCollisionOption.OpenIfExists);
        }

        // Save the given content under the provided file name within the images folder
        public static async Task SaveImage(string fileName, Stream content)
        {
            IFolder imagesFolder = await GetImagesFolder();
            IFile file = await imagesFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            using (Stream stream = await file.OpenAsync(FileAccess.ReadAndWrite))
            {
                await content.CopyToAsync(stream);
            }
        }

        // Get all files within the images folder
        public static async Task<List<IFile>> GetImageFiles()
        {
            return new List<IFile>(await (await GetImagesFolder()).GetFilesAsync());
        }
    }
}
