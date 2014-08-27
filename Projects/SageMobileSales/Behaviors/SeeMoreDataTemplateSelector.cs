using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;
using SageMobileSales.UILogic.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SageMobileSales.Behaviors
{
    public class SeeMoreDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate GridItemsTemplate { get; set; }
        public DataTemplate ImageTemplate { get; set; }
        QuoteDetails quoteDetails;
        OrderDetails orderDetails;
        Contact contact;
        FrequentlyPurchasedItem frequentlyPurchasedItem;
        Address address;
        protected override DataTemplate SelectTemplateCore(object item,
                                                           DependencyObject container)
        {
            if (item != null)

                if (item.GetType().Name == "QuoteDetails")
                {

                    quoteDetails = item as QuoteDetails;


                    if (quoteDetails.QuoteStatus == PageUtils.SeeMore)
                        return ImageTemplate;
                    else
                        return GridItemsTemplate;
                }
                else if (item.GetType().Name == "OrderDetails")
                {
                    orderDetails = item as OrderDetails;
                    if (orderDetails.OrderDescription == PageUtils.SeeMore)
                        return ImageTemplate;
                    else
                        return GridItemsTemplate;
                }
                else if (item.GetType().Name == "Address")
                {
                    address = item as Address;
                    if (address.PostalCode == PageUtils.SeeMore)
                        return ImageTemplate;
                    else
                        return GridItemsTemplate;
                }
                else if (item.GetType().Name == "Contact")
                {
                    contact = item as Contact;
                    if (contact.EmailPersonal == PageUtils.SeeMore)
                        return ImageTemplate;
                    else
                        return GridItemsTemplate;
                }
                else if (item.GetType().Name == "FrequentlyPurchaseItem")
                {
                    frequentlyPurchasedItem = item as FrequentlyPurchasedItem;
                    if (frequentlyPurchasedItem.ItemDescription == PageUtils.SeeMore)
                        return ImageTemplate;
                    else
                        return GridItemsTemplate;
                }
            return base.SelectTemplateCore(item, container);
        }
    }
}
