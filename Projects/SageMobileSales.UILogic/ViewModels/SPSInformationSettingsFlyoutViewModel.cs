using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.UILogic.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SageMobileSales.UILogic.ViewModels
{
    class SPSInformationSettingsFlyoutViewModel : BindableBase, IFlyoutViewModel
    {
        private Action _closeFlyout;
        private bool _isOpened;
        private string _id;
        private string _key;
        public DelegateCommand SaveCommand { get; set; }
        private ApplicationDataContainer settingsLocal;

        public SPSInformationSettingsFlyoutViewModel()
        {
            SaveCommand = DelegateCommand.FromAsyncHandler(Save);
            RestoreValues();
        }

        [RestorableState]
        public bool IsOpened
        {
            get { return _isOpened; }
            private set { SetProperty(ref _isOpened, value); }
        }

        public Action CloseFlyout
        {
            get { return _closeFlyout; }
            set { SetProperty(ref _closeFlyout, value); }
        }

        private void Close()
        {
            IsOpened = false;
        }

        public string MerchantId
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        public string MerchantKey
        {
            get { return _key; }
            set { SetProperty(ref _key, value); }
        }

        private void RestoreValues()
        {
            settingsLocal = ApplicationData.Current.LocalSettings;
            settingsLocal.CreateContainer("SageSalesContainer", ApplicationDataCreateDisposition.Always);
            if (!string.IsNullOrEmpty(settingsLocal.Containers["SageSalesContainer"].Values["MerchantId"] as string))
                PageUtils.MerchantId = MerchantId = settingsLocal.Containers["SageSalesContainer"].Values["MerchantId"] as string;

            if (!string.IsNullOrEmpty(settingsLocal.Containers["SageSalesContainer"].Values["MerchantKey"] as string))
                PageUtils.MerchantKey = MerchantKey = settingsLocal.Containers["SageSalesContainer"].Values["MerchantKey"] as string;
        }

        private async Task Save()
        {
            settingsLocal = ApplicationData.Current.LocalSettings;
            settingsLocal.CreateContainer("SageSalesContainer", ApplicationDataCreateDisposition.Always);

            settingsLocal.Containers["SageSalesContainer"].Values["MerchantId"] = PageUtils.MerchantId = MerchantId;
            settingsLocal.Containers["SageSalesContainer"].Values["MerchantKey"] = PageUtils.MerchantKey = MerchantKey;

            //this.CloseFlyout = () => new SettingsFlyout().Hide();
        }
    }
}
