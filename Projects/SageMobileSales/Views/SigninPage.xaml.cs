using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.StoreApps;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SageMobileSales.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SigninPage : VisualStateAwarePage
    {
        public SigninPage()
        {
            InitializeComponent();
            // this.DataContext = SageMobileSales.UILogic.ViewModels.SigninPageViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }
    }
}