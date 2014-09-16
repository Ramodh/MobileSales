using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.Services;
using SageMobileSales.UILogic.Common;

namespace SageMobileSales.UILogic.ViewModels
{
    internal class CreateShippingAddressPageViewModel : ViewModel
    {
        private readonly IAddressRepository _addressRepository;
        private readonly INavigationService _navigationService;
        private readonly IQuoteRepository _quoteRepository;
        private readonly IQuoteService _quoteService;
        private Address _address;
        private bool _isEnabled;
        private string _log = string.Empty;
        private Quote _quote;
        private string _quoteId;

        public CreateShippingAddressPageViewModel(INavigationService navigationService,
            IAddressRepository addressRepository, IQuoteService quoteService, IQuoteRepository quoteRepository)
        {
            _address = new Address();
            _navigationService = navigationService;
            _isEnabled = true;
            _addressRepository = addressRepository;
            _quoteService = quoteService;
            _quoteRepository = quoteRepository;
        }

        /// <summary>
        ///     Holds Address
        /// </summary>
        [RestorableState]
        public Address Address
        {
            get { return _address; }
            set { SetProperty(ref _address, value); }
        }

        /// <summary>
        ///     evaluate validation
        /// </summary>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (SetProperty(ref _isEnabled, value))
                {
                    // Enable/Disable validation based on the value of this property
                    _address.IsValidationEnabled = IsEnabled;
                }
            }
        }

        /// <summary>
        ///     Load add contact page
        /// </summary>
        /// <param name="navigationParameter"></param>
        /// <param name="navigationMode"></param>
        /// <param name="viewModelState"></param>
        public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode,
            Dictionary<string, object> viewModelState)
        {
            try
            {
                _quoteId = navigationParameter as string;
                _address.AddressId = PageUtils.Pending + Guid.NewGuid();
                //   _address.AddressType = "Shipping";
                _address.Country = PageUtils.Country;
                // _address.CustomerId = _quote.CustomerId;
                // _quoteDetails = navigationParameter as QuoteDetails;
                var errorsCollection =
                    RetrieveEntityStateValue<IDictionary<string, ReadOnlyCollection<string>>>("errorsCollection",
                        viewModelState);

                if (errorsCollection != null)
                {
                    _address.SetAllErrors(errorsCollection);
                }
                base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        ///     check user input is valid or not
        /// </summary>
        public bool ValidateForm()
        {
            return _address.ValidateProperties();
        }

        public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatedFrom(viewModelState, suspending);
            if (viewModelState != null)
            {
                AddEntityStateValue("errorsCollection", _address.GetAllErrors(), viewModelState);
            }
        }

        /// <summary>
        ///     Saves Address and Navigates to QuoteDetails on save click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public async void SaveAddressButton_Click(object sender, object parameter)
        {
            if (ValidateForm())
            {
                string errorMessage = string.Empty;

                try
                {
                    _quote = await _quoteRepository.GetQuoteAsync(_quoteId);

                    _address.CustomerId = _quote.CustomerId;
                    _address.AddressType = PageUtils.Other;
                    _address.IsPending = true;
                    await _addressRepository.AddAddressToDbAsync(_address);
                    _quote.AddressId = _address.AddressId;
                    await _quoteRepository.UpdateQuoteToDbAsync(_quote);

                    if (Constants.ConnectedToInternet())
                    {
                        Address address = await _quoteService.UpdateQuoteShippingAddress(_quote, _address.AddressId);

                        if ((_quote.QuoteId.Contains(PageUtils.Pending)))
                        {
                            if (address.AddressId.Contains(PageUtils.Pending))
                            {
                                _quote.AddressId = null;
                                await _quoteRepository.UpdateQuoteToDbAsync(_quote);
                            }
                            else if (address != null)
                            {
                                _quote.AddressId = address.AddressId;
                                _quote = await _quoteService.PostDraftQuote(_quote);
                            }
                            //if (quote != null)
                            //    await _quoteService.UpdateQuoteShippingAddress(quote, _address);
                            //await _quoteService.PostQuoteShippingAddress(_quote, _address);
                        }
                        else
                        {
                            if (address.AddressId.Contains(PageUtils.Pending))
                            {
                                _quote.AddressId = null;
                                await _quoteRepository.UpdateQuoteToDbAsync(_quote);
                            }
                            else if (address != null)
                            {
                                _quote.AddressId = address.AddressId;
                                await _quoteService.UpdateQuoteShippingAddressKey(_quote);
                            }
                        }
                    }
                    _navigationService.Navigate("QuoteDetails", _quoteId);
                }
                catch (EntityValidationException ex)
                {
                    DisplayValidationErrors(ex.ValidationResult);
                }
                catch (Exception ex)
                {
                    _log = AppEventSource.Log.WriteLine(ex);
                    AppEventSource.Log.Error(_log);
                }
            }
        }

        /// <summary>
        ///     Display validation errors if any
        /// </summary>
        /// <param name="modelValidationResults"></param>
        private void DisplayValidationErrors(EntityValidationResult modelValidationResults)
        {
            var errors = new Dictionary<string, ReadOnlyCollection<string>>();

            // Property keys format: address.{Propertyname}
            foreach (string propkey in modelValidationResults.ModelState.Keys)
            {
                string propertyName = propkey.Substring(propkey.IndexOf('.') + 1);

                errors.Add(propertyName, new ReadOnlyCollection<string>(modelValidationResults.ModelState[propkey]));
            }

            if (errors.Count > 0) _address.Errors.SetAllErrors(errors);
        }
    }
}