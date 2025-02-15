namespace AndroidGalleryTestNetMaui
{
    public class ImageLoader
    {
        private readonly Action<string> setErrorMessage;
        private readonly Action<bool> setBusy;
        private readonly Action<List<string>> setImagesList;

        public ImageLoader(Action<string> setErrorMessage, Action<bool> setBusy, Action<List<string>> setImagesList)
        {
            this.setErrorMessage = setErrorMessage;
            this.setBusy = setBusy;
            this.setImagesList = setImagesList;
        }

        public async Task LoadImagesAsync()
        {
            setBusy(true);
            try
            {
#if WINDOWS
                    await LoadWindowsImagesAsync();
#elif __ANDROID__
                    await LoadAndroidImagesAsync();
#endif
            }
            catch (Exception ex)
            {
                setErrorMessage($"Error: {ex.Message}");
            }
            finally
            {
                setBusy(false);
            }
        }

#if WINDOWS
        private async Task LoadWindowsImagesAsync()
        {
            var imageDirectory = @"C:\Users\redfo\Downloads\AI Girls\AI Daenerys Targaryen";

            if (!Directory.Exists(imageDirectory))
            {
                setErrorMessage($"Error: Directory not found: {imageDirectory}");
                return;
            }

            var supportedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var images = Directory.EnumerateFiles(imageDirectory)
                .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLower()))
                .Select(f => $"file://{f}")
                .ToList();

            setImagesList(images);
        }
#elif __ANDROID__
        private async Task LoadAndroidImagesAsync()
        {
            var testPermission = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            if (Permissions.ShouldShowRationale<Permissions.StorageRead>())
            {
                setErrorMessage("Error: Storage permission is required to access storage.");
            }
            testPermission = await Permissions.RequestAsync<Permissions.StorageRead>();
            if (testPermission != PermissionStatus.Granted)
            {
                setErrorMessage("Error: Storage permission is required to load images.");
                return;
            }
        
            var images = new List<string>();
        
            if (OperatingSystem.IsAndroidVersionAtLeast(29))
            {
                LoadAndroidModernImages(images);
            }
            else
            {
                LoadAndroidLegacyImages(images);
            }
        
            setImagesList(images);
        
            //foreach (var imagePath in images)
            //{
            //    System.Diagnostics.Debug.WriteLine(imagePath);
            //}
        }
        
        private void LoadAndroidModernImages(List<string> images)
        {
            string[] projection = {
                Android.Provider.IBaseColumns.Id,
                Android.Provider.MediaStore.IMediaColumns.Data,
                Android.Provider.MediaStore.IMediaColumns.RelativePath
            };
        
            string selection = $"{Android.Provider.MediaStore.IMediaColumns.Data} LIKE ? AND {Android.Provider.MediaStore.IMediaColumns.MimeType} LIKE ?";
            string[] selectionArgs = new[] { "/storage/emulated/0/Pictures/FashionApp/MasksImages/%", "image/%" };
            string sortOrder = $"{Android.Provider.MediaStore.IMediaColumns.DateAdded} DESC";
        
            using var cursor = Android.App.Application.Context.ContentResolver?.Query(
                Android.Provider.MediaStore.Images.Media.ExternalContentUri,
                projection,
                selection,
                selectionArgs,
                sortOrder);
        
            if (cursor == null)
            {
                setErrorMessage("Error: Cursor is null. Query failed.");
                return;
            }
        
            if (!cursor.MoveToFirst())
            {
                setErrorMessage("Info: No images found in the specified directory.");
                return;
            }
        
            int idColumn = cursor.GetColumnIndex(Android.Provider.IBaseColumns.Id);
            int pathColumn = cursor.GetColumnIndex(Android.Provider.MediaStore.IMediaColumns.RelativePath);
            System.Diagnostics.Debug.WriteLine($"Searching in path: {cursor.GetString(pathColumn)}");
        
            do
            {
                string? id = cursor.GetString(idColumn);
                Android.Net.Uri? contentUri = Android.Net.Uri.WithAppendedPath(
                    Android.Provider.MediaStore.Images.Media.ExternalContentUri,
                    id);
                images.Add(contentUri.ToString());
                System.Diagnostics.Debug.WriteLine($"Added image URI: {contentUri}");
            } while (cursor.MoveToNext());
        }
        
        private void LoadAndroidLegacyImages(List<string> images)
        {
            string path = "/storage/emulated/0/Pictures/FashionApp/MasksImages";
            if (Directory.Exists(path))
            {
                var files = Directory.EnumerateFiles(path, "*.*")
                    .Where(f => f.EndsWith(".jpg") || f.EndsWith(".jpeg") ||
                               f.EndsWith(".png") || f.EndsWith(".gif"));
        
                foreach (var file in files)
                {
                    images.Add($"file://{file}");
                }
            }
            else
            {
                setErrorMessage("Error: Directory not found.");
            }
        }
#endif
    }
} 