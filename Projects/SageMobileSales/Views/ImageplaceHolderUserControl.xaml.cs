using Q42.WinRT.Data;
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
            DependencyProperty.Register("Placeholder", typeof(ImageSource), typeof(ImageplaceHolderUserControl),
                new PropertyMetadata(default(ImageSource), PlaceHolderChanged));

        /// <summary>
        ///     Binding Real time Images to Source
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(Uri), typeof(ImageplaceHolderUserControl),
                new PropertyMetadata(default(ImageSource), SourceChanged));

        public ImageplaceHolderUserControl()
        {
            InitializeComponent();
        }

        public ImageSource Placeholder
        {
            get { return (ImageSource)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        private static void PlaceHolderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ImageplaceHolderUserControl).Image.Source = (ImageSource)e.NewValue;
        }

        /// <summary>
        ///     Async call to bind real time images and replacing placeholders
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static async void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ImageplaceHolderUserControl)d;
            var bitmapImage = new BitmapImage();
            Uri newCacheUri = (Uri)d.GetValue(SourceProperty);
            // HttpClientRequest Exception will be caused by failure of internet or invalid request/response
            var httpClient = new HttpClient();
            try
            {
                //if (Constants.ConnectedToInternet())
                //{
                byte[] contentBytes = null;

                if (newCacheUri != null)
                {


                    //Get image from cache (download and set in cache if needed)
                    var cacheUri = await WebDataCache.GetLocalUriAsync(newCacheUri);

                    // Check if the wanted image uri has not changed while we were loading
                    if (newCacheUri != (Uri)d.GetValue(SourceProperty)) return;

#if NETFX_CORE
                    //Set cache uri as source for the image
                    control.LoadImage(new BitmapImage(cacheUri));
                    //control.Source = newCacheUri;

#elif WINDOWS_PHONE
                        BitmapImage bimg = new BitmapImage();

                        using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            using (IsolatedStorageFileStream stream = iso.OpenFile(cacheUri.PathAndQuery, FileMode.Open, FileAccess.Read))
                            {
                                bimg.SetSource(stream);
                            }
                        }
                        //Set cache uri as source for the image
                        image.Source = bimg;
#endif

                }
            }
            catch (Exception ex)
            {
                control.LoadImage(new BitmapImage(newCacheUri));
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