using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Sage.Authorisation.WinRT.Storage
{
    /// <summary>
    /// Certificate metadata is a structure which is used to parse the JSON response from GetClientCredential
    /// </summary>
    internal class CertificateMetadata
    {
        /// <summary>
        /// Gets or sets the expiry of the credential.
        /// </summary>
        /// <value>
        /// The expiry.
        /// </value>
        public  DateTime expiry { get; set; }

        /// <summary>
        /// Gets or sets the friendly_name of the credential.
        /// </summary>
        /// <value>
        /// The friendly_name.
        /// </value>
        public string friendly_name { get; set; }

        /// <summary>
        /// Gets or sets the credential, Base64 encoded
        /// </summary>
        /// <value>
        /// The credential.
        /// </value>
        public string credential { get; set; }

        /// <summary>
        /// Gets or sets the type of the credential, it should be PFX for Windows 8 
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string type { get; set; }

        /// <summary>
        /// Gets or sets the format. Should be PKCS12
        /// </summary>
        /// <value>
        /// The format.
        /// </value>
        public string format { get; set; }

        /// <summary>
        /// Shows the value of the object properties
        /// </summary>
        public override string ToString()
        {
            return String.Format("ClientCredentialResponse: Credential={0}, Type={1}, Format={2}.", credential, type, format);
        }
    }
}
