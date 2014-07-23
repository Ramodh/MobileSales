using Windows.ApplicationModel.Resources;

namespace SageMobileSales.DataAccess.Common
{
    //Error Messages for Validation
    public static class ErrorMessages
    {
        public static string RequiredErrorMessage
        {
            get { return ResourceLoader.GetForCurrentView("Resources").GetString("ErrorRequired"); }
        }

        public static string RegexErrorMessage
        {
            get { return ResourceLoader.GetForCurrentView("Resources").GetString("ErrorRegex"); }
        }
    }
}