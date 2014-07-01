using Microsoft.Practices.Prism.StoreApps;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SageMobileSales.UILogic.ViewModels
{
    [DataContract]
    public class QuoteLineItemViewModel : ViewModel
    {

        private string _productId;
        private string _productName;                
        private int _lineItemQuantity;
        private decimal _lineItemPrice;        
        private string _imageUri;
        private int _productQuantity;
        private string _productSku;
        private string _lineItemId;
        private string _quoteId;


        public QuoteLineItemViewModel(LineItemDetails quoteLineItemDetails)
        {
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
            get { return _productName!=null?_productName.Trim():string.Empty; }
        }

        public string LineItemId
        {
            get
            {
                return _lineItemId;
            }            
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

        public decimal Amount { get { return Math.Round(LineItemQuantity * _lineItemPrice, 2); } }

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
    }
}
