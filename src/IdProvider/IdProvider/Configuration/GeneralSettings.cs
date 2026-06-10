using System;

namespace IdProvider.Configuration
{
    /// <summary>
    /// General configuration settings
    /// </summary>
    public class GeneralSettings
    {
        /// <summary>
        /// Master kill switch for the Test-IDP. When false (default) every Test-IDP
        /// endpoint short-circuits and no tokens are issued. Must be explicitly
        /// enabled per environment. See issue #1983 / #1409.
        /// </summary>
        public bool TestIdpEnabled { get; set; } = false;

        /// <summary>
        /// The single shared access password that grants the right to use the
        /// Test-IDP. This is NOT a per-user credential - there is no username and
        /// no per-user password. Injected from Key Vault per environment; never
        /// committed. When empty the Test-IDP refuses all logins (fail-closed).
        /// See issue #1983 / #1409.
        /// </summary>
        public string TestIdpSharedPassword { get; set; } = string.Empty;

        /// <summary>
        /// When true, the token endpoint requires PKCE: an authorization code
        /// issued without a code_challenge cannot be redeemed. When false
        /// (default, safe rollout) PKCE is enforced only when a code_challenge
        /// was supplied at /Authorize. See issue #1983 / #1409.
        /// </summary>
        public bool RequirePkce { get; set; } = false;

        /// <summary>
        /// Number of consecutive failed shared-password attempts (globally, since
        /// it is a single shared secret) before the login is locked out.
        /// </summary>
        public int SharedPasswordMaxFailures { get; set; } = 5;

        /// <summary>
        /// Lockout duration in minutes once <see cref="SharedPasswordMaxFailures"/>
        /// is reached.
        /// </summary>
        public int SharedPasswordLockoutMinutes { get; set; } = 15;

        /// <summary>
        /// Per-IP request ceiling (per minute) for the Test-IDP login form
        /// (POST /Authorize). The shared password is the prime brute-force
        /// target, so this stays deliberately low. See issue #1983.
        /// </summary>
        public int LoginRateLimitPerMinute { get; set; } = 10;

        /// <summary>
        /// Per-IP request ceiling (per minute) for the unauthenticated,
        /// CPU-bound Test-IDP API endpoints (POST /token, POST /par and the
        /// <c>request_uri</c> branch of GET /Authorize). These do signing /
        /// verification work and were previously unthrottled, so this caps a
        /// mass-calling flood while staying high enough not to throttle
        /// legitimate load-test traffic. Raise per environment for heavier
        /// performance runs. See issue #1983.
        /// </summary>
        public int ApiRateLimitPerMinute { get; set; } = 600;

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

        public string IdProviderEndpoint { get; internal set; } = "https://mockporten.azurewebsites.net/";

        public string IssCode { get; set; } = "https://mockporten.azurewebsites.net/authorization";

        public string IssToken { get; set; } = "https://mockporten.azurewebsites.net/authorization";
    }
}
