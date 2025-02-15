using Microsoft.Maui.Controls.PlatformConfiguration;

namespace AndroidGalleryTestNetMaui
{
    public partial class MainPage : ContentPage
    {
        public static readonly BindableProperty ImagesListProperty =
            BindableProperty.Create(nameof(ImagesList), typeof(List<string>), typeof(MainPage), new List<string>());

        public List<string> ImagesList
        {
            get => (List<string>)GetValue(ImagesListProperty);
            set => SetValue(ImagesListProperty, value);
        }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set
            {
                if (isBusy != value)
                {
                    isBusy = value;
                    OnPropertyChanged(nameof(IsBusy));
                }
            }
        }

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadImages();
        }

        private async void LoadImages()
        {
            IsBusy = true;
            try
            {
#if WINDOWS
                var imageDirectory = @"C:\Users\redfo\Downloads\AI Girls\AI Daenerys Targaryen";

                if (!Directory.Exists(imageDirectory))
                {
                    await DisplayAlert("Error", $"Directory not found: {imageDirectory}", "OK");
                    return;
                }

                var supportedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                ImagesList = Directory.GetFiles(imageDirectory)
                    .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLower()))
                    .Select(f => $"file://{f}")
                    .ToList();      
#elif __ANDROID__    
                var testPermision = PermissionStatus.Unknown;
                testPermision = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
                if (Permissions.ShouldShowRationale<Permissions.StorageRead>())
                {
                    await DisplayAlert("Permission", "Need permission to access photos", "OK");
                }
                testPermision = await Permissions.RequestAsync<Permissions.StorageRead>();
                if (testPermision != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permission required", "Storage permission is required to load images", "OK");
                    return;
                }
                
                var images = new List<string>();
                
                // Проверка за версията на Android
                if (OperatingSystem.IsAndroidVersionAtLeast(29)) // Android 10 или по-нова версия
                {
                    // Android MediaStore query
                    string[] projection = {
                    Android.Provider.MediaStore.Images.ImageColumns.Id,
                    Android.Provider.MediaStore.IMediaColumns.Data,
                    Android.Provider.MediaStore.IMediaColumns.RelativePath
                    };
                
                    string selection = $"{Android.Provider.MediaStore.IMediaColumns.Data} LIKE ? AND {Android.Provider.MediaStore.IMediaColumns.MimeType} LIKE ?";
                    string[] selectionArgs = new[] { "/storage/emulated/0/Pictures/FashionApp/MasksImages/%", "image/%" };
                    string sortOrder = $"{Android.Provider.MediaStore.IMediaColumns.DateAdded} DESC";
                
                    using var cursor = Android.App.Application.Context.ContentResolver.Query(
                        Android.Provider.MediaStore.Images.Media.ExternalContentUri,
                        projection,
                        selection,
                        selectionArgs,
                        sortOrder);
                
                    if (cursor == null)
                    {
                        await DisplayAlert("Error", "Cursor is null. Query failed.", "OK");
                        return;
                    }
                
                    if (!cursor.MoveToFirst())
                    {
                        await DisplayAlert("Info", "No images found in the specified directory.", "OK");
                        return;
                    }
                
                    int idColumn = cursor.GetColumnIndex(Android.Provider.MediaStore.Images.ImageColumns.Id);
                    int pathColumn = cursor.GetColumnIndex(Android.Provider.MediaStore.IMediaColumns.RelativePath);
                    System.Diagnostics.Debug.WriteLine($"Searching in path: {cursor.GetString(pathColumn)}");
                    do
                    {
                        string id = cursor.GetString(idColumn);
                        Android.Net.Uri contentUri = Android.Net.Uri.WithAppendedPath(
                            Android.Provider.MediaStore.Images.Media.ExternalContentUri,
                            id);
                        images.Add(contentUri.ToString());
                        System.Diagnostics.Debug.WriteLine($"Added image URI: {contentUri}");
                    } while (cursor.MoveToNext());                
                }
                else // За Android 9 и по-стари версии
                {
                    // Директен достъп до файловата система
                    string path = "/storage/emulated/0/Pictures/FashionApp/MasksImages";
                    if (Directory.Exists(path))
                    {
                        var files = Directory.GetFiles(path, "*.*")
                            .Where(f => f.EndsWith(".jpg") || f.EndsWith(".jpeg") || 
                                   f.EndsWith(".png") || f.EndsWith(".gif"));
                        
                        foreach (var file in files)
                        {
                            images.Add($"file://{file}");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Directory not found", "OK");
                        return;
                    }
                }
                
                ImagesList = images;
                
                if (!ImagesList.Any())
                {
                    await DisplayAlert("Info", "No images found in directory", "OK");
                    return;
                }
                    
                // Добавяме дебъг информация
                System.Diagnostics.Debug.WriteLine($"Loaded {ImagesList.Count} images:");
                foreach (var imagePath in ImagesList)
                {
                    System.Diagnostics.Debug.WriteLine(imagePath);
                }

            }
            catch (Exception ex)
            {
                 await DisplayAlert("Error", $"An error occurred: {ex.Message}\nStack: {ex.StackTrace}", "OK");      
#endif
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
