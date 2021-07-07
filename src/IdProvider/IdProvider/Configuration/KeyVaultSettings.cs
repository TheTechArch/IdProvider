using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace IdProvider.Configuration
{
    /// <summary>
    /// The key vault settings used to fetch certificate information from key vault
    /// </summary>
    public class KeyVaultSettings
    {
        /// <summary>
        /// Uri to keyvault
        /// </summary>
        public string KeyVaultURI { get; set; }

        /// <summary>
        /// Name of the certificate secret
        /// </summary>
        public string MaskinPortenCertSecretId { get; set; } = "digdirtestcert";

    }

}
