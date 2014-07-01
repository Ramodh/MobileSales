using Microsoft.Practices.Prism.StoreApps;
using SageMobileSales.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// TODO: Connect the Search Results Page to your in-app search.
// The Search Results Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234240

namespace SageMobileSales.Views
{
    /// <summary>
    /// This page displays search results when a global search is directed to this application.
    /// </summary>
    public sealed partial class SearchResultsPage : VisualStateAwarePage
    { 
       public SearchResultsPage()
        {
            this.InitializeComponent();         
        }
    }
}
