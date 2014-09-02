using System;
using System.Runtime.Serialization;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.DataAccess.Model;

namespace SageMobileSales.UILogic.ViewModels
{
    [DataContract]
    public class QuoteLineItemViewModel : ViewModel
    {
        private readonly string _imageUri;
        private readonly string _lineItemId;
        private readonly decimal _lineItemPrice;
        private readonly INavigationService _navigationService;
        private readonly string _productName;
        private readonly int _productQuantity;
        private readonly string _productSku;
        private readonly string _quoteId;
        private int _lineItemQuantity;
        private string _productId;


        public QuoteLineItemViewModel(INavigationService navigationService, LineItemDetails quoteLineItemDetails)
        {
            _navigationService = navigationService;
            if (quoteLineItemDetails == null)
            {
                throw new ArgumentNullException("quoteLineItem", "quoteLineItem cannot be null");
            }

            _lineItemId = quoteLineItemDetails.LineItemId;
            _lineItemQuantity = quoteLineItemDetails.LineItemQuantity;
            _lineItemPrice = quoteLineItemDetails.LineItemPrice;
            _imageUri = quoteLineItemDetails.Url;
            _productName = quoteLineItemDetails.ProductName;
            _productQuantity = quoteLineItemDetails.ProductQuantity;
            _productSku = quoteLineItemDetails.ProductSku;
            _quoteId = quoteLineItemDetails.LineId;
        }

        public string QuoteId
        {
            get { return _quoteId; }
        }

        public string ProductId
        {
            get { return _productId; }
        }

        public string ProductName
        {
            get { return _productName != null ? _productName.Trim() : string.Empty; }
        }

        public string LineItemId
        {
            get { return _lineItemId; }
        }

        public decimal LineItemPrice
        {
            get { return _lineItemPrice; }
        }

        public int LineItemQuantity
        {
            get { return _lineItemQuantity; }
            set
            {
                if (SetProperty(ref _lineItemQuantity, value))
                {
                    OnPropertyChanged("Amount");
                    //OnPropertyChanged("DiscountedPrice");
                    //OnPropertyChanged("FullPriceDouble");
                    //OnPropertyChanged("DiscountedPriceDouble");
                }
            }
        }

        public string ImageUri
        {
            get { return _imageUri; }
        }

        public decimal Amount
        {
            get { return Math.Round(LineItemQuantity*_lineItemPrice, 2); }
        }

        public int ProductQuantity
        {
            get { return _productQuantity; }
        }

        public string ProductSku
        {
            get { return _productSku; }
        }

        //public string FullPrice
        //{
        //    get { return _currencyFormatter.FormatDouble(FullPriceDouble); }
        //}

        //public double DiscountPercentage
        //{
        //    get { return _discountPercentage; }
        //}

        //public ImageSource Image
        //{
        //    get { return new BitmapImage(_imageUri); }
        //}

        /// <summary>
        ///     Grid View Item Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public async void GridViewRecentOrderItemClick(object sender, object parameter)
        {
            var arg = sender as QuoteLineItemViewModel;

            if (arg != null)
                _navigationService.Navigate("RecentOrders", arg);
        }
    }
}