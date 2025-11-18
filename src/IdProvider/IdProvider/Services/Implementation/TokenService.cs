using IdProvider.Configuration;
using IdProvider.Models;
using IdProvider.Services.Interface;
using IdProvider.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;

namespace IdProvider.Services.Implementation
{
    public class TokenService: IToken
    {
        private readonly IJwtSigningCertificateProvider _certificateProvider;
        private readonly GeneralSettings _generalSettings;
        private readonly JwtSecurityTokenHandler _validator;

        public TokenService(IOptions<GeneralSettings> generalSettings,IJwtSigningCertificateProvider certificateProvider)
        {
            _generalSettings = generalSettings.Value;
            _certificateProvider = certificateProvider;
            _validator = new JwtSecurityTokenHandler();
        }

        public async Task<string> GetAuthorizationCode(OidcAuthorizationModel oidcAuthorizationModel)
        {
            string[] userClaims = oidcAuthorizationModel.UserClaims.Split(",");
            
            string issuer = _generalSettings.IssCode;
            List<Claim> claims = new List<Claim>();

            if(userClaims.Count() > 0)
            {
                foreach(string claim in userClaims)
                {
                    string[] claimSplit = claim.Split(":");
                    if(claimSplit.Count() == 2)
                    {
                        claims.Add(new Claim(claimSplit[0], claimSplit[1]));
                    }
                }
            }

            if(!string.IsNullOrEmpty(oidcAuthorizationModel.Nonce))
            {
                claims.Add(new Claim("nonce", oidcAuthorizationModel.Nonce));
            }

            ClaimsIdentity identity = new ClaimsIdentity("authorizationcode");
            identity.AddClaims(claims);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            return await GenerateToken(principal, DateTime.Now.AddMinutes(5));
        }

        public static ClaimsPrincipal GetClaimsPrincipal(string sub, string pid, string locale, string nonce, string sid, string aud, string[] acr, string[] amr, DateTimeOffset auth_time)
        {
            List<Claim> claims = new List<Claim>();
            string issuer = "www.idporten.no";
            claims.Add(new Claim("iss", issuer, ClaimValueTypes.String, issuer));
            claims.Add(new Claim("sub", sub, ClaimValueTypes.String, issuer));
            claims.Add(new Claim("pid", pid, ClaimValueTypes.String, issuer));
            claims.Add(new Claim("locale", locale, ClaimValueTypes.String, issuer));
            claims.Add(new Claim("nonce", nonce, ClaimValueTypes.String, issuer));
            claims.Add(new Claim("sid", sid, ClaimValueTypes.String, issuer));
            claims.Add(new Claim("aud", aud, ClaimValueTypes.String, issuer));
            claims.Add(new Claim("acr", string.Join(" ", acr), ClaimValueTypes.String, issuer));
            claims.Add(new Claim("auth_time", auth_time.ToUnixTimeSeconds().ToString(), ClaimValueTypes.DateTime, issuer));
            if (amr != null && amr.Length != 0)
            {
                var amrr = amr
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Distinct()
                .ToArray();

                if (amrr.Length > 0)
                {
                    string amrJson = JsonSerializer.Serialize(amrr); // e.g. ["TestID","pwd"]
                    claims.Add(new Claim("amr", amrJson, JsonClaimValueTypes.JsonArray));
                }
            }

            ClaimsIdentity identity = new ClaimsIdentity("mock");
            identity.AddClaims(claims);
            ClaimsPrincipal principal = new(identity);

            return principal;
        }


        public async Task<string> GetTokenFromCode(string code)
        {
           JwtSecurityToken codeToken = await ValidateAuthorizationCode(code);
            string issuer = _generalSettings.IssToken;
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim("iss", issuer));
            claims.AddRange(codeToken.Claims);
            ClaimsIdentity identity = new ClaimsIdentity("token");
            identity.AddClaims(claims);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            return await GenerateToken(principal, DateTime.Now.AddMinutes(5));
        }

        private async Task<string> GenerateToken(ClaimsPrincipal principal, DateTime? expires = null)
        {
            List<X509Certificate2> certificates = await _certificateProvider.GetCertificates();

            X509Certificate2 certificate = GetLatestCertificateWithRolloverDelay(
                certificates, _generalSettings.JwtSigningCertificateRolloverDelayHours);

            TimeSpan tokenExpiry = new TimeSpan(0, _generalSettings.JwtValidityMinutes, 0);
            if (expires == null)
            {
                expires = DateTime.UtcNow.AddSeconds(tokenExpiry.TotalSeconds);
            }

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(principal.Identity),
                Expires = expires,
                SigningCredentials = new X509SigningCredentials(certificate)
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            string serializedToken = tokenHandler.WriteToken(token);

            return serializedToken;
        }

        private X509Certificate2 GetLatestCertificateWithRolloverDelay(
    List<X509Certificate2> certificates, int rolloverDelayHours)
        {
            // First limit the search to just those certificates that have existed longer than the rollover delay.
            var rolloverCutoff = DateTime.Now.AddHours(-rolloverDelayHours);
            var potentialCerts =
                certificates.Where(c => c.NotBefore < rolloverCutoff).ToList();

            // If no certs could be found, then widen the search to any usable certificate.
            if (!potentialCerts.Any())
            {
                potentialCerts = certificates.Where(c => c.NotBefore < DateTime.Now).ToList();
            }

            // Of the potential certs, return the newest one.
            return potentialCerts
                .OrderByDescending(c => c.NotBefore)
                .FirstOrDefault();
        }

        private async Task<JwtSecurityToken> ValidateAuthorizationCode(string originalToken)
        {
            List<X509Certificate2> certificates = await _certificateProvider.GetCertificates();

            X509Certificate2 certificate = GetLatestCertificateWithRolloverDelay(
                certificates, _generalSettings.JwtSigningCertificateRolloverDelayHours);
            SecurityKey key = new X509SecurityKey(certificate);

            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = false,
                ValidateAudience = false,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            _validator.ValidateToken(originalToken, validationParameters, out _);

            JwtSecurityToken token = _validator.ReadJwtToken(originalToken);
            return token;
        }
    }
}
