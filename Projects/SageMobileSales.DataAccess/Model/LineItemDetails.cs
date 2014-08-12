using System;

namespace SageMobileSales.DataAccess.Model
{
    public class LineItemDetails
    {
        public string LineId { get; set; }
        public string LineItemId { get; set; }
        public decimal LineItemPrice { get; set; }
        public int LineItemQuantity { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string Url { get; set; }
        public int ProductQuantity { get; set; }
        public string ProductSku { get; set; }
        public string CustomerName { get; set; }
        public decimal Amount
        {
            get { return Math.Round(LineItemQuantity*LineItemPrice, 2); }
        }
    }
}