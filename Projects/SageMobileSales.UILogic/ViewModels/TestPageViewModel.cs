﻿using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;

namespace SageMobileSales.UILogic.ViewModels
{
    public class TestPageViewModel : ViewModel
    {
        private INavigationService _navigationService;

        public TestPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            //flyoutSvc = flyoutService;
            //_eventAggregator = eventAggregator;
            // SignInCommand = DelegateCommand.FromAsyncHandler(LoginAsync);

            // eventAggregator.GetEvent<SageMobileSales.Events.CloudSettingChangedEvent>().Subscribe(OnSettingsChanged);
            // ConfigureVisualState();
        }
    }
}