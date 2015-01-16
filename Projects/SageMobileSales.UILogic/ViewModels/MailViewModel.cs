using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;

namespace SageMobileSales.UILogic.ViewModels
{
    public class MailViewModel
    {
        private string _companyDetailsTable = string.Empty;
        private string _companyInfoDatesTable = string.Empty;
        private string _customerDetailsTable = string.Empty;
        private decimal _discountPercentageValue;
        private string _finalHtmlTable = string.Empty;
        private string _lineItemDetailsTable = string.Empty;
        private string _log = string.Empty;
        private decimal _subTotal;
        private string _subTotalDetailsTable = string.Empty;

        public async Task<StringBuilder> ReadTextFile()
        {
            var html = new StringBuilder();

            var file3 = await StorageFile.GetFileFromPathAsync(@"Htmls/QuoteEmail.html");
            var r = await FileIO.ReadTextAsync(file3);

            //var rs = Application.GetResourceStream(new Uri("abc.html", UriKind.Relative));
            //StreamReader sr = new StreamReader(rs.Stream);
            //string html = sr.ReadToEnd(); 

            return html;
        }

        /// <summary>
        ///     Creates Html Table to Show Complete Quote Data.
        /// </summary>
        /// <param name="customerDetails"></param>
        /// <param name="quoteDetails"></param>
        /// <param name="quoteLineItemsList"></param>
        /// <param name="subTotal"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public string BuildQuoteEmailContent(Tenant tenant, CustomerDetails customerDetails,
            Address customerMailingAddress, QuoteDetails quoteDetails,
            List<LineItemDetails> quoteLineItemsList, string subTotal, string total)
        {
            try
            {
                if (tenant != null && customerDetails != null && quoteDetails != null && quoteLineItemsList != null)
                {
                    if (quoteDetails.DiscountPercent > 0)
                    {
                        _discountPercentageValue = CalculateDiscountPercent(quoteDetails.DiscountPercent,
                            Convert.ToDecimal(subTotal));
                    }
                    _companyDetailsTable =
                        "<table style='border-bottom:1px solid lightgray' width='100%'><tr><td style='color:darkblue' align='left'><h1>" +
                        tenant.Name + "</h1></td><td style='color:gray' align='right'>" + quoteDetails.QuoteStatus +
                        "</td></tr></table>";

                    _companyInfoDatesTable =
                        "<table width='100%'><tr><td align='right'><table border='0' width='100%'><tr><td><table width='100%'><tr><td>" +
                        tenant.Name + "</td></tr><tr><td>" + tenant.AddressLine1 + "</td></tr><tr><td>" +
                        tenant.AddressLine2 + "</td></tr><tr><td>" +
                        (tenant.City != null ? tenant.City + ',' : string.Empty) +
                        (tenant.Region != null ? tenant.Region + "," : string.Empty) +
                        tenant.PostalCode +
                        "</td></tr></table></td><td style='padding:3px;' align='right'><table border='1px'><tr><td style='border-bottom:1px solid lightgray;'>Date:</td><td style='border-bottom:1px solid lightgray;'>" +
                        quoteDetails.CreatedOn.ToString("MMM d, yyyy") +
                        "</td></tr><tr><td style='border-bottom:1px solid lightgray;'>Quote Number:</td><td style='border-bottom:1px solid lightgray;'>" +
                        quoteDetails.QuoteNumber + "</td></tr></table></td></tr></table></td></tr></table>";

                    _customerDetailsTable = "<table border='0'><tr><td><b>Prepared for:</b></td><td>" +
                                            customerDetails.CustomerName + "</td></tr><tr><td></td><td>" +
                                            customerMailingAddress.AddressName + "</td></tr><tr><td></td><td>" +
                                            customerMailingAddress.Street1 + "</td></tr><tr><td></td><td>" +
                                            customerMailingAddress.City + "," + customerDetails.StateProvince + " " +
                                            customerMailingAddress.PostalCode + "</td></tr><tr><td></td><td>" +
                                            customerMailingAddress.Phone + "</td></tr></table><p><b>" +
                                            quoteDetails.QuoteDescription + "</b></p>";

                    _lineItemDetailsTable =
                        "<table border='1px' width='100%'><tr><th bgcolor='#d3d3d3'>Item</th><th bgcolor='#d3d3d3'>Item No.</th><th bgcolor='#d3d3d3'>Qty</th><th bgcolor='#d3d3d3'>Price</th><th bgcolor='#d3d3d3'>Total</th></tr>" +
                        CreateQuoteLineItemRows(quoteLineItemsList) + "</table>";

                    _subTotalDetailsTable =
                        "<table border='0' width='100%'><tr><td align='right'><table border='0'><tr><td>Subtotal:</td><td>" +
                        subTotal + "</td></tr><tr><td>Shipping:</td><td>" + quoteDetails.ShippingAndHandling +
                        "</td></tr><tr><td>Discount:</td><td>" + _discountPercentageValue +
                        "</td></tr><tr><td>Est. Tax:</td><td>" + quoteDetails.Tax +
                        "</td></tr><tr><td><b>Total:</b></td><td><b>" + total +
                        "</b></td></tr></table></td></tr></table>";

                    _finalHtmlTable = "<html>" + _companyDetailsTable + "</br>" + _companyInfoDatesTable + "</br>" +
                                      _customerDetailsTable + "</br>" + _lineItemDetailsTable + "</br>" +
                                      _subTotalDetailsTable + " </html>";
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return _finalHtmlTable;
        }

        /// <summary>
        ///     Creates Rows for QuoteLineItems.
        /// </summary>
        /// <param name="quoteLineItemsList"></param>
        /// <returns></returns>
        private string CreateQuoteLineItemRows(List<LineItemDetails> quoteLineItemsList)
        {
            var quoteLineItemDetailsTable = string.Empty;

            var quoteLineItem = string.Empty;
            foreach (var lineItem in quoteLineItemsList)
            {
                quoteLineItem += "<tr><td><b>" + lineItem.ProductName + "</b><br />" + lineItem.ProductDescription +
                                 "</td><td>" + lineItem.ProductSku + "</td><td>" + lineItem.LineItemQuantity +
                                 "</td><td>" + lineItem.LineItemPrice + "</td><td>" +
                                 Math.Round(lineItem.LineItemQuantity*lineItem.LineItemPrice, 2) + "</td></tr>";
                _subTotal += lineItem.Amount;
            }

            return quoteLineItem;
        }

        /// <summary>
        ///     Creates Html Table to Show Complete Quote Data.
        /// </summary>
        /// <param name="customerDetails"></param>
        /// <param name="quoteDetails"></param>
        /// <param name="quoteLineItemsList"></param>
        /// <param name="subTotal"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public string BuildOrderEmailContent(Tenant tenant, CustomerDetails customerDetails,
            Address customerMailingAddress, OrderDetails orderDetails,
            List<LineItemDetails> orderLineItemsList, string subTotal, string total)
        {
            try
            {
                if (tenant != null && customerDetails != null && orderDetails != null && orderLineItemsList != null)
                {
                    if (orderDetails.DiscountPercent > 0)
                    {
                        _discountPercentageValue = CalculateDiscountPercent(orderDetails.DiscountPercent,
                            Convert.ToDecimal(subTotal));
                    }
                    _companyDetailsTable =
                        "<table style='border-bottom:1px solid lightgray' width='100%'><tr><td style='color:darkblue' align='left'><h1>" +
                        tenant.Name + "</h1></td><td style='color:gray' align='right'>" + orderDetails.OrderStatus +
                        "</td></tr></table>";

                    _companyInfoDatesTable =
                        "<table width='100%'><tr><td align='right'><table border='0' width='100%'><tr><td><table width='100%'><tr><td>" +
                        tenant.Name + "</td></tr><tr><td>" + tenant.AddressLine1 + "</td></tr><tr><td>" +
                        tenant.AddressLine2 + "</td></tr><tr><td>" +
                        (tenant.City != null ? tenant.City + ',' : string.Empty) +
                        (tenant.Region != null ? tenant.Region + "," : string.Empty) +
                        tenant.PostalCode +
                        "</td></tr></table></td><td style='padding:3px;' align='right'><table border='1px'><tr><td style='border-bottom:1px solid lightgray;'>Approved:</td><td style='border-bottom:1px solid lightgray;'>" +
                        orderDetails.CreatedOn.ToString("MM/dd/yyyy") +
                        "</td></tr><tr><td style='border-bottom:1px solid lightgray;'>Updated:</td><td style='border-bottom:1px solid lightgray;'>" +
                        orderDetails.SubmittedDate.ToString("MM/dd/yyyy") +
                        "</td></tr><tr><td style='border-bottom:1px solid lightgray;'>Order Number:</td><td style='border-bottom:1px solid lightgray;'>" +
                        orderDetails.OrderNumber +
                        "</td></tr></tr></table></td></tr></table></td></tr></table>";

                    _customerDetailsTable = "<table border='0'><tr><td><b>Prepared for:</b></td><td>" +
                                            customerDetails.CustomerName + "</td></tr><tr><td></td><td>" +
                                            customerMailingAddress.AddressName + "</td></tr><tr><td></td><td>" +
                                            customerMailingAddress.Street1 + "</td></tr><tr><td></td><td>" +
                                            customerMailingAddress.City + "," + customerDetails.StateProvince + " " +
                                            customerMailingAddress.PostalCode + "</td></tr><tr><td></td><td>" +
                                            customerMailingAddress.Phone + "</td></tr></table><p><b>" +
                                            orderDetails.OrderDescription + "</b></p>";

                    _lineItemDetailsTable =
                        "<table border='1px' width='100%'><tr><th bgcolor='#d3d3d3'>Item</th><th bgcolor='#d3d3d3'>Item No.</th><th bgcolor='#d3d3d3'>Qty</th><th bgcolor='#d3d3d3'>Price</th><th bgcolor='#d3d3d3'>Total</th></tr>" +
                        CreateQuoteLineItemRows(orderLineItemsList) + "</table>";

                    _subTotalDetailsTable =
                        "<table border='0' width='100%'><tr><td align='right'><table border='0'><tr><td>Subtotal:</td><td>" +
                        _subTotal + "</td></tr><tr><td>Shipping:</td><td>" + orderDetails.ShippingAndHandling +
                        "</td></tr><tr><td>Discount:</td><td>" + _discountPercentageValue +
                        "</td></tr><tr><td>Est. Tax:</td><td>" + orderDetails.Tax +
                        "</td></tr><tr><td><b>Total:</b></td><td><b>" + total +
                        "</b></td></tr></table></td></tr></table>";

                    _finalHtmlTable = "<html>" + _companyDetailsTable + "</br>" + _companyInfoDatesTable + "</br>" +
                                      _customerDetailsTable + "</br>" + _lineItemDetailsTable + "</br>" +
                                      _subTotalDetailsTable + " </html>";
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return _finalHtmlTable;
        }

        private decimal CalculateDiscountPercent(decimal discountPercent, decimal subTotal)
        {
            // discountPercentage = Math.Round(((1 - ((SubTotal - DiscountPercentageValue) / SubTotal)) * 100), 2);
            return (Math.Round(((discountPercent/100)*subTotal), 2));
        }
    }
}