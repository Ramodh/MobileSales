using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.Services;
using SageMobileSales.UILogic.Common;
using Windows.UI.Popups;
using Windows.ApplicationModel.Resources;

namespace SageMobileSales.UILogic.ViewModels
{
    internal class AddContactPageViewModel : ViewModel
    {
        private readonly IContactRepository _contactRepository;
        private readonly IContactService _contactService;
        private readonly INavigationService _navigationService;
        private Contact _contact;
        private CustomerDetails _customerAddress;
        private string _customerId;
        private bool _inProgress;
        private bool _isEnabled;
        private bool _isSaveEnabled;
        private string _log = string.Empty;

        public AddContactPageViewModel(INavigationService navigationService, IContactRepository contactRepository,
            IContactService contactService)
        {
            _navigationService = navigationService;
            _contact = new Contact();
            _isEnabled = true;
            _contactRepository = contactRepository;
            _contactService = contactService;
            TextChangedCommand = new DelegateCommand<object>(TextBoxTextChanged);
        }

        public DelegateCommand<object> TextChangedCommand { get; set; }

        /// <summary>
        ///     Holds Contact details
        /// </summary>
        [RestorableState]
        public Contact Contact
        {
            get { return _contact; }
            set
            {
                SetProperty(ref _contact, value);
                OnPropertyChanged("Contact");
            }
        }

        /// <summary>
        ///     Data loading indicator
        /// </summary>
        public bool InProgress
        {
            get { return _inProgress; }
            private set { SetProperty(ref _inProgress, value); }
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
                    _contact.IsValidationEnabled = IsEnabled;
                }
            }
        }

        /// <summary>
        ///     evaluate validation
        /// </summary>
        public bool IsSaveEnabled
        {
            get { return _isSaveEnabled; }
            set
            {
                SetProperty(ref _isSaveEnabled, value);
                OnPropertyChanged("IsSaveEnabled");
            }
        }

        /// <summary>
        ///     Holds Customer Details
        /// </summary>
        public CustomerDetails CustomerAddress
        {
            get { return _customerAddress; }
            set { SetProperty(ref _customerAddress, value); }
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
                IsSaveEnabled = true;
                _customerId = navigationParameter as string;
                _contact.CustomerId = _customerId;
                //  _customerAddress = navigationParameter as CustomerDetails;
                //  _contact.CustomerId = _customerAddress.CustomerId;
                _contact.ContactId = PageUtils.Pending + Guid.NewGuid();
                var errorsCollection =
                    RetrieveEntityStateValue<IDictionary<string, ReadOnlyCollection<string>>>("errorsCollection",
                        viewModelState);

                if (errorsCollection != null)
                {
                    _contact.SetAllErrors(errorsCollection);
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
            return _contact.ValidateProperties();
        }

        public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatedFrom(viewModelState, suspending);
            if (viewModelState != null)
            {
                AddEntityStateValue("errorsCollection", _contact.GetAllErrors(), viewModelState);
            }
        }

        /// <summary>
        ///     //Navigate to create quote page on appbar button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public async void SaveContactButton_Click(object sender, object parameter)
        {
            try
            {
                if ((string.IsNullOrEmpty(Contact.FirstName) && string.IsNullOrEmpty(Contact.LastName)) &&
                    IsSaveEnabled)
                {
                    MessageDialog msgDialog = new MessageDialog(
                          ResourceLoader.GetForCurrentView("Resources").GetString(" MesDialogAddContactSaveMessage"),
                          ResourceLoader.GetForCurrentView("Resources").GetString("MesDialogAddContactTitle"));
                        msgDialog.Commands.Add(new UICommand("Ok"));
                        await msgDialog.ShowAsync();
                }
                else if(IsSaveEnabled)
                {
                    if (ValidateForm())
                    {
                        string errorMessage = string.Empty;
                        IsSaveEnabled = false;
                        InProgress = true;
                        _contact.IsPending = true;
                        await _contactRepository.AddContactToDbAsync(_contact);
                        if (Constants.ConnectedToInternet())
                        {
                            await _contactService.PostContact(_contact);
                        }
                        InProgress = false;

                        _navigationService.GoBack();
                    }                
                }
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

            if (errors.Count > 0) _contact.Errors.SetAllErrors(errors);
        }

        /// <summary>
        ///     TextChanged event to filter items
        /// </summary>
        /// <param name="searchText"></param>
        private void TextBoxTextChanged(object args)
        {
            if (!string.IsNullOrEmpty(((TextBox) args).Text))
            {
                string searchTerm = ((TextBox) args).Text.Trim();
                string formated = string.Format("{0:(###) ###-####}",
                    Convert.ToDouble(searchTerm));
                Contact.PhoneWork = formated;
                OnPropertyChanged("PhoneWork");
            }
        }
    }
}