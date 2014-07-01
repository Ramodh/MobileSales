using System.ComponentModel.DataAnnotations;
using SageMobileSales.DataAccess.Common;
using SQLite;

namespace SageMobileSales.DataAccess.Entities
{
    [Table("Address")]
    public class Address : ValidatableBindableBase
    {
        [PrimaryKey, Unique, AutoIncrement]
        public int Id { get; set; }

        [Required(ErrorMessageResourceType = typeof (ErrorMessages), ErrorMessageResourceName = "RequiredErrorMessage")]
        public string AddressName { get; set; }

        public string CustomerId { get; set; }
        public string AddressId { get; set; }
        public string AddressType { get; set; }

        [Required(ErrorMessageResourceType = typeof (ErrorMessages), ErrorMessageResourceName = "RequiredErrorMessage")]
        public string Street1 { get; set; }

        public string Street2 { get; set; }
        public string Street3 { get; set; }
        public string Street4 { get; set; }

        [Required(ErrorMessageResourceType = typeof (ErrorMessages), ErrorMessageResourceName = "RequiredErrorMessage")]
        public string City { get; set; }

        [Required(ErrorMessageResourceType = typeof (ErrorMessages), ErrorMessageResourceName = "RequiredErrorMessage")]
        public string StateProvince { get; set; }

        [Required(ErrorMessageResourceType = typeof (ErrorMessages), ErrorMessageResourceName = "RequiredErrorMessage")]
        public string PostalCode { get; set; }

        public string Country { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Url { get; set; }
        public bool IsPending { get; set; }
    }
}