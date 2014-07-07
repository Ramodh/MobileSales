using System;
using System.Net.Http;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace SageMobileSales.Views
{
    public sealed partial class ImageplaceHolderUserControl : UserControl
    {
        /// <summary>
        ///     ImagePlaceholder
        /// </summary>
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof (ImageSource), typeof (ImageplaceHolderUserControl),
                new PropertyMetadata(default(ImageSource), PlaceHolderChanged));

        /// <summary>
        ///     Binding Real time Images to Source
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof (string), typeof (ImageplaceHolderUserControl),
                new PropertyMetadata(default(ImageSource), SourceChanged));

        public ImageplaceHolderUserControl()
        {
            InitializeComponent();
        }

        public ImageSource Placeholder
        {
            get { return (ImageSource) GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public string Source
        {
            get { return (string) GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        private static void PlaceHolderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ImageplaceHolderUserControl).Image.Source = (ImageSource) e.NewValue;
        }

        /// <summary>
        ///     Async call to bind real time images and replacing placeholders
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static async void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ImageplaceHolderUserControl) d;
            var bitmapImage = new BitmapImage();

            // HttpClientRequest Exception will be caused by failure of internet or invalid request/response
            var httpClient = new HttpClient();
            try
            {
                //if (Constants.ConnectedToInternet())
                //{
                byte[] contentBytes = null;
                if (!string.IsNullOrEmpty(control.Source))
                {
                    contentBytes = await httpClient.GetByteArrayAsync(control.Source);

                    var randomAccessStream = new InMemoryRandomAccessStream();
                    var dataWriter = new DataWriter(randomAccessStream);
                    dataWriter.WriteBytes(contentBytes);
                    await dataWriter.StoreAsync();
                    randomAccessStream.Seek(0);

                    await bitmapImage.SetSourceAsync(randomAccessStream);
                    //bitmapImage.ImageOpened += (sender, args) => control.LoadImage(bitmapImage);
                    control.LoadImage(bitmapImage);
                }
                //else
                //{
                //    bitmapImage.UriSource=new Uri("ms-appx:/Assets/imagePlaceholder_100x100.png", UriKind.RelativeOrAbsolute);
                //    control.Placeholder = bitmapImage;
                //    //bitmapImage.UriSource = new Uri("ms-appx:/Assets/imagePlaceholder_100x100.png", UriKind.RelativeOrAbsolute);
                //    ////bitmapImage.ImageOpened += (sender, args) => control.LoadImage(bitmapImage);
                //    //control.LoadImage(bitmapImage);
                //}
            }
            catch
            {
            }
        }


        /// <summary>
        ///     Loading images with animation
        /// </summary>
        /// <param name="source"></param>
        private void LoadImage(ImageSource source)
        {
            ImageFadeOut.Completed += (s, e) =>
            {
                Image.Source = source;
                ImageFadeIn.Begin();
            };
            ImageFadeOut.Begin();
        }
    }
}