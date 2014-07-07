namespace SageMobileSales.ServiceAgents.JsonHelpers
{
    public class OrderJson
    {
        public string AuthorizationCode { get; set; }
        public string QuoteId { get; set; }
        public string Status { get; set; }
        public string ExpirationMonth { get; set; }
        public string TaxAmount { get; set; }
        public string TransactionDate { get; set; }
        public string CreditCardLast4 { get; set; }
        public string PaymentMethod { get; set; }
        public string ExpirationYear { get; set; }
        public string InitialOrderStatus { get; set; }
        public string AmountPaid { get; set; }
        public string PaymentReference { get; set; }
    }
}