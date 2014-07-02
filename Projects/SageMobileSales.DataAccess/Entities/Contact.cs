using System.ComponentModel.DataAnnotations;
using SageMobileSales.DataAccess.Common;
using SQLite;

namespace SageMobileSales.DataAccess.Entities
{
    [Table("Contact")]
    public class Contact : ValidatableBindableBase
    {
        private const string NamesRegexPattern = @"^[a-zA-Z0-9]*$";

        private const string NumbersRegexPattern = @"\A\p{N}+([\p{N}\-][\p{N}]+)*\z";

        private const string EmailRegexPattern = @"^(?("")(""[^""]+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[A-Za-z0-9]{2,24}))$";

        [PrimaryKey, Unique, AutoIncrement]
        public int Id { get; set; }

        public string ContactId { get; set; }
        public string CustomerId { get; set; }

        // [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "RequiredErrorMessage")]
       // [RegularExpression(NamesRegexPattern, ErrorMessageResourceType = typeof (ErrorMessages),ErrorMessageResourceName = "RegexErrorMessage")]
        public string FirstName { get; set; }

        //[Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "RequiredErrorMessage")]
        //[RegularExpression(NamesRegexPattern, ErrorMessageResourceType = typeof (ErrorMessages),ErrorMessageResourceName = "RegexErrorMessage")]
        public string LastName { get; set; }

        public string Title { get; set; }

        [RegularExpression(NumbersRegexPattern, ErrorMessageResourceType = typeof (ErrorMessages),
            ErrorMessageResourceName = "RegexErrorMessage")]
        public string PhoneMobile { get; set; }

        [RegularExpression(NumbersRegexPattern, ErrorMessageResourceType = typeof (ErrorMessages),
            ErrorMessageResourceName = "RegexErrorMessage")]
        public string PhoneWork { get; set; }

        public string PhoneHome { get; set; }

        [RegularExpression(EmailRegexPattern, ErrorMessageResourceType = typeof (ErrorMessages),
            ErrorMessageResourceName = "RegexErrorMessage")]
        public string EmailWork { get; set; }

        public string EmailPersonal { get; set; }
        public string Url { get; set; }
        //To check data is posted
        public bool IsPending { get; set; }
    }
}