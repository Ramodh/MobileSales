using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;
using SageMobileSales.UILogic.Common;

namespace SageMobileSales.Behaviors
{
    public class SeeMoreDataTemplateSelector : DataTemplateSelector
    {
        private Address address;
        private Contact contact;
        private FrequentlyPurchasedItem frequentlyPurchasedItem;
        private OrderDetails orderDetails;
        private QuoteDetails quoteDetails;
        private SalesHistory recentOrders;
        public DataTemplate GridItemsTemplate { get; set; }
        public DataTemplate ImageTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item,
            DependencyObject container)
        {
            if (item != null)

                if (item.GetType().Name == "QuoteDetails")
                {
                    quoteDetails = item as QuoteDetails;


                    if (quoteDetails.QuoteStatus == PageUtils.SeeMore)
                        return ImageTemplate;
                    return GridItemsTemplate;
                }
                else if (item.GetType().Name == "OrderDetails")
                {
                    orderDetails = item as OrderDetails;
                    if (orderDetails.OrderDescription == PageUtils.SeeMore)
                        return ImageTemplate;
                    return GridItemsTemplate;
                }
                else if (item.GetType().Name == "Address")
                {
                    address = item as Address;
                    if (address.PostalCode == PageUtils.SeeMore)
                        return ImageTemplate;
                    return GridItemsTemplate;
                }
                else if (item.GetType().Name == "Contact")
                {
                    contact = item as Contact;
                    if (contact.EmailPersonal == PageUtils.SeeMore)
                        return ImageTemplate;
                    return GridItemsTemplate;
                }
                else if (item.GetType().Name == "FrequentlyPurchasedItem")
                {
                    frequentlyPurchasedItem = item as FrequentlyPurchasedItem;
                    if (frequentlyPurchasedItem.ItemDescription == PageUtils.SeeMore)
                        return ImageTemplate;
                    return GridItemsTemplate;
                }
                else if (item.GetType().Name == "SalesHistory")
                {
                    recentOrders = item as SalesHistory;
                    if (recentOrders.InvoiceNumber == PageUtils.SeeMore)
                        return ImageTemplate;
                    return GridItemsTemplate;
                }
            return base.SelectTemplateCore(item, container);
        }
    }
}