namespace SageMobileSales.DataAccess.Model
{
    public class FrequentlyPurchasedItems
    {
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
        public double QuantityYTD { get; set; }
        public double PriorYTD { get; set; }
    }
}