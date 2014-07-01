using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SageMobileSales.UILogic.ViewModels
{
   public class ContactsPageViewModel: ViewModel
    {

       private INavigationService _navigationService;
       private IContactRepository _contactRepository;
       private string _log=string.Empty;


       private List<Contact> _customerContactList;
       private string _customerId;

       public List<Contact> CustomerContactList
       {
           get { return _customerContactList; }
           private set{SetProperty(ref _customerContactList, value);}
       }

       public ContactsPageViewModel(INavigationService navigationService, IContactRepository contactRepository)
       {
           _navigationService = navigationService;
           _contactRepository = contactRepository;
       }

       public async override void OnNavigatedTo(object navigationParameter, Windows.UI.Xaml.Navigation.NavigationMode navigationMode, Dictionary<string, object> viewModelState)
       {
           try
           {
               _customerId = navigationParameter as string;
               CustomerContactList = await _contactRepository.GetContactDetailsAsync(_customerId);
               base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);
           }
           catch (Exception ex)
           {
               _log = AppEventSource.Log.WriteLine(ex);
               AppEventSource.Log.Error(_log);
           }
       }
       /// <summary>
       ///  //Navigate to Add Contact page on appbar button click
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="parameter"></param>
       public void AddContactsButton_Click(object sender, object parameter)
       {
         
               _navigationService.Navigate("AddContact", _customerId);          
          
       }
    }
}
