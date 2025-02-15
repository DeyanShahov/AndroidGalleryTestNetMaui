using Microsoft.Maui.Controls.PlatformConfiguration;
using System;

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

        private string errorMessage = string.Empty;

        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                if (errorMessage != value)
                {
                    errorMessage = value;
                    OnPropertyChanged(nameof(ErrorMessage));
                    ErrorLabel.Text = value;
                    ErrorLabel.IsVisible = !string.IsNullOrEmpty(value);
                }
            }
        }

        private readonly ImageLoader imageLoader;
        private readonly SingleImageLoader singleImageLoader;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            imageLoader = new ImageLoader(
                setErrorMessage: (msg) => ErrorMessage = msg,
                setBusy: (busy) => IsBusy = busy,
                setImagesList: (images) => ImagesList = images
            );
            singleImageLoader = new SingleImageLoader(
                setErrorMessage: (msg) => ErrorMessage = msg,
                setImage: (uri) => SelectedBodyImage.Source = Microsoft.Maui.Controls.ImageSource.FromUri(new Uri(uri))
            );
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = imageLoader.LoadImagesAsync();
            _ = singleImageLoader.LoadSingleImageAsync("open_jacket_mask.png");
        }
    }
}
