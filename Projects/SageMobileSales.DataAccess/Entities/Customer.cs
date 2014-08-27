using System;
using SQLite;

namespace SageMobileSales.DataAccess.Entities
{
    [Table("Customer")]
    public class Customer
    {
        [PrimaryKey, Unique, AutoIncrement]
        public int Id { get; set; }

        public Decimal CreditAvailable { get; set; }
        public Decimal CreditLimit { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public bool IsCreditLimitUsed { get; set; }
        public bool IsOnCreditHold { get; set; }
        //public string PaymentMethod { get; set; }
        public string PaymentTerms { get; set; }
        public string EntityStatus { get; set; }
        public decimal YearToDate { get; set; }
        public decimal MonthToDate { get; set; }
    }
}