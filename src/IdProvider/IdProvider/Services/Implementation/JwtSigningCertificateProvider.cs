using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using IdProvider.Configuration;
using IdProvider.Services.Interfaces;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Rest.Azure;

namespace IdProvider.Services
{
    /// <summary>
    /// Represents a <see cref="IJwtSigningCertificateProvider"/> that can obtain a certificate from a key vault using a key vault client.
    /// If there are no key vault settings available the logic will instead attempt to find a certificate on the file system as a fallback.
    /// </summary>
    /// <remarks>
    /// This service is intended to be used as a Singleton. Access to the <see cref="X509Certificate2"/> is locked using <see cref="SemaphoreSlim"/>.
    /// </remarks>
    public class JwtSigningCertificateProvider : IJwtSigningCertificateProvider
    {
        private readonly KeyVaultSettings _keyVaultSettings;
        private readonly CertificateSettings _certificateSettings;
        private readonly ILogger _logger;

        private List<X509Certificate2> _certificates;
        private DateTime _certificateUpdateTime;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Initialize a new instance of <see cref="JwtSigningCertificateProvider"/> with settings for accessing a key vault and file system.
        /// </summary>
        /// <param name="keyVaultSettings">Settings required to access a certificate stored in a key vault.</param>
        /// <param name="certificateSettings">Settings required to access a certificate stored on a file system.</param>
        public JwtSigningCertificateProvider(
            IOptions<KeyVaultSettings> keyVaultSettings,
            IOptions<CertificateSettings> certificateSettings, ILogger<JwtSigningCertificateProvider> logger)
        {
            _keyVaultSettings = keyVaultSettings.Value;
            _certificateSettings = certificateSettings.Value;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<List<X509Certificate2>> GetCertificates()
        {
            await _semaphore.WaitAsync();

            try
            {
                if (_certificateUpdateTime > DateTime.Now && _certificates != null)
                {
                    return _certificates;
                }

                _certificates = new List<X509Certificate2>();

                List<X509Certificate2> certificates = await GetAllCertificateVersions(
                    _keyVaultSettings.KeyVaultURI, _keyVaultSettings.MaskinPortenCertSecretId);
                _certificates.AddRange(certificates);
              

                // Reuse the same list of certificates for 1 hour.
                _certificateUpdateTime = DateTime.Now.AddHours(1);

                _certificates = _certificates.OrderByDescending(cer => cer.NotBefore).ToList();
                return _certificates;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<List<X509Certificate2>> GetAllCertificateVersions(string keyVaultUrl, string certificateName)
        {
            _logger.LogInformation("In method");
            List<X509Certificate2> certificates = new List<X509Certificate2>();

            CertificateClient certificateClient = new CertificateClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            AsyncPageable<CertificateProperties> certificatePropertiesPage = certificateClient.GetPropertiesOfCertificateVersionsAsync(certificateName);
            await foreach (CertificateProperties certificateProperties in certificatePropertiesPage)
            {
                _logger.LogInformation("In loop");
                if (certificateProperties.Enabled == true &&
                    (certificateProperties.ExpiresOn == null || certificateProperties.ExpiresOn >= DateTime.UtcNow))
                {
                    SecretClient secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());

                    KeyVaultSecret secret = await secretClient.GetSecretAsync(certificateProperties.Name, certificateProperties.Version);
                    X509Certificate2 certificateWithPrivateKey = new X509Certificate2(Convert.FromBase64String(secret.Value), (string)null, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
                    certificates.Add(certificateWithPrivateKey);
                }
            }
            return certificates;
        }
    }
}
