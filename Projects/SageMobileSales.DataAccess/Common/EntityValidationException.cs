using System;
using System.Globalization;

namespace SageMobileSales.DataAccess.Common
{
    public class EntityValidationException : Exception
    {
        public EntityValidationException(EntityValidationResult validationResult)
        {
            ValidationResult = validationResult;
        }

        public EntityValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public EntityValidationException(string message)
            : base(message)
        {
        }

        public EntityValidationException()
        {
        }

        public EntityValidationResult ValidationResult { get; set; }

        public override string Message
        {
            get
            {
                string result = string.Empty;
                bool firstItem = true;

                foreach (string key in ValidationResult.ModelState.Keys)
                {
                    if (!firstItem) result += "\n";

                    string errors = string.Join(", ", ValidationResult.ModelState[key].ToArray());
                    result += string.Format(CultureInfo.CurrentCulture, "{0} : {1}", key, errors);
                    firstItem = false;
                }

                return result;
            }
        }
    }
}