using System;

namespace IdProvider.Configuration
{
    /// <summary>
    /// General configuration settings
    /// </summary>
    public class GeneralSettings
    {
       
        /// Gets or sets the AltinnParty cookie name
        /// </summary>
        public string AltinnPartyCookieName { get; set; }

        /// <summary>
        /// Gets the altinnParty cookie from kubernetes environment variables and appsettings if environment variable is not set
        /// </summary>
        public string GetAltinnPartyCookieName
        {
            get
            {
                return Environment.GetEnvironmentVariable("GeneralSettings__AltinnPartyCookieName") ?? AltinnPartyCookieName;
            }
        }

        /// <summary>
        /// Gets or sets the number of minutes the JSON Web Token and the cookie is valid.
        /// </summary>
        public int JwtValidityMinutes { get; set; }

        /// <summary>
        /// Gets or sets the hostname
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// Gets the jwt cookie validity time from kubernetes environment variables and appsettings if environment variable is not set
        /// </summary>
        public string GetHostName
        {
            get
            {
                return Environment.GetEnvironmentVariable("GeneralSettings__HostName") ?? HostName;
            }
        }

        /// <summary>
        /// Gets or sets the BaseUrl
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets the jwt cookie validity time from kubernetes environment variables and appsettings if environment variable is not set
        /// </summary>
        public string GetBaseUrl
        {
            get
            {
                return Environment.GetEnvironmentVariable("GeneralSettings__BaseUrl") ?? BaseUrl;
            }
        }

        /// <summary>
        /// Gets or sets URL of the well known configuration endpoint for Maskinporten.
        /// </summary>
        public string MaskinportenWellKnownConfigEndpoint { get; set; }

        /// <summary>
        /// Gets or sets URL of the well known configuration endpoint for Maskinporten from kubernetes or appsettings if no environment variable is set.
        /// </summary>
        public string GetMaskinportenWellKnownConfigEndpoint
        {
            get
            {
                return Environment.GetEnvironmentVariable("GeneralSettings__" + nameof(MaskinportenWellKnownConfigEndpoint)) ??
                       MaskinportenWellKnownConfigEndpoint;
            }
        }

        /// <summary>
        /// Gets url of the well known configuration endpoint for ID-porten from environment variable.
        /// </summary>
        public string IdPortenWellKnownConfigEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the url to the json file which holds the valid organisation entries (which inclides name, organisation number and org identifier)
        /// </summary>
        public string OrganisationRepositoryLocation { get; set; }

        /// <summary>
        /// Gets the url of the list of valid organisation entries (json)
        /// </summary>
        public string GetOrganisationRepositoryLocation
        {
            get
            {
                return Environment.GetEnvironmentVariable("GeneralSettings__" + nameof(OrganisationRepositoryLocation)) ??
                    OrganisationRepositoryLocation;
            }
        }

        /// <summary>
        /// Gets or sets the URL of the Altinn Open ID Connect well-known configuration endpoint.
        /// </summary>
        public string OpenIdWellKnownEndpoint { get; set; }

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
        public string IdProviderEndpoint { get; internal set; }
    }
}
