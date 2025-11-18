using System;

namespace IdProvider.Configuration
{
    /// <summary>
    /// General configuration settings
    /// </summary>
    public class GeneralSettings
    {
        /// <summary>
        /// Gets or sets the number of minutes the JSON Web Token and the cookie is valid.
        /// </summary>
        public int JwtValidityMinutes { get; set; }
        
        /// <summary>
        /// Gets or sets the number of hours to wait before a new certificate is being used to
        /// sign new JSON Web tokens.
        /// </summary>
        /// <remarks>
        /// The logic use the NotBefore property of a certificate. This means that uploading a
        /// certificate that has been valid for a few days might cause it to be used immediately.
        /// Take care not to upload "old" certificates.
        /// </remarks>
        public int JwtSigningCertificateRolloverDelayHours { get; set; }

        public string IdProviderEndpoint { get; internal set; } = "https://idprovider.azurewebsites.net/";

        public string IssCode { get; set; } = "https://idprovider.azurewebsites.net/authorization";

        public string IssToken { get; set; } = "https://idprovider.azurewebsites.net/authorization";
    }
}
