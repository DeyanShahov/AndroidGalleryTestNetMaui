using Microsoft.Maui.Controls;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AndroidGalleryTestNetMaui
{
    public partial class ImageDetailPage : ContentPage
    {
        private readonly string _imageUri;

        public ImageDetailPage(string imageUri)
        {
            InitializeComponent();
            _imageUri = imageUri;
            // Задаваме източник на изображението
            DetailImage.Source = ImageSource.FromUri(new Uri(_imageUri));
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private async void DeleteButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Проверяваме дали _imageUri е локален файл (с префикс "file://")
                if (_imageUri.StartsWith("file://"))
                {
                    string filePath = _imageUri.Substring("file://".Length);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        await DisplayAlert("Deleted", "Image deleted successfully.", "OK");
                        await Navigation.PopModalAsync();
                    }
                    else
                    {
                        await DisplayAlert("Error", "File does not exist.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Cannot delete remote images.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete image: {ex.Message}", "OK");
            }
        }
    }
} 