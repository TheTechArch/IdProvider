using Altinn.Authorization.ServiceDefaults;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using IdProvider.Configuration;
using IdProvider.Services;
using IdProvider.Services.Implementation;
using IdProvider.Services.Interface;
using IdProvider.Services.Interfaces;
using System;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = AltinnHost.CreateWebApplicationBuilder("idprovider", args);
var services = builder.Services;

// Load secrets from the mockporten Key Vault as a configuration source. The
// vault URI is supplied per environment via kvSetting:KeyVaultURI (already
// configured in Azure). A secret named "GeneralSettings--TestIdpSharedPassword"
// binds to GeneralSettings:TestIdpSharedPassword (the Key Vault config provider
// maps "--" to ":"). See #1983.
var keyVaultUri = builder.Configuration["kvSetting:KeyVaultURI"];
if (!string.IsNullOrWhiteSpace(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUri),
        new DefaultAzureCredential());
}

services.AddOptions<GeneralSettings>()
    .BindConfiguration("GeneralSettings")
    .ValidateOnStart();

services.AddOptions<KeyVaultSettings>()
    .BindConfiguration("kvSetting")
    .ValidateOnStart();

services.AddControllersWithViews();

services.AddSingleton<IJwtSigningCertificateProvider, JwtSigningCertificateProvider>();
services.AddSingleton<IToken, TokenService>();
services.AddSingleton<ISharedAccessPasswordValidator, SharedAccessPasswordValidator>();

// Per-IP rate limiting (#1983). Two policies, both partitioned on the caller IP:
//   - "test-idp-login": the shared-password form (POST /Authorize), the prime
//     brute-force target, kept deliberately low.
//   - "test-idp-api": the unauthenticated, CPU-bound endpoints (POST /token,
//     POST /par, and the request_uri branch of GET /Authorize), previously
//     unthrottled. A higher ceiling that backstops a mass-calling flood without
//     throttling legitimate load-test traffic.
// Both limits are configurable so they can be tuned per environment.
var rateLimitSettings = builder.Configuration.GetSection("GeneralSettings").Get<GeneralSettings>() ?? new GeneralSettings();
services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("test-idp-login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitSettings.LoginRateLimitPerMinute,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
    options.AddPolicy("test-idp-api", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitSettings.ApiRateLimitPerMinute,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

services.AddLogging();
services.AddApplicationInsightsTelemetry(opts =>
{
    opts.ConnectionString = builder.Configuration["APPINSIGHTS_CONNECTIONSTRING"];
});

var app = builder.Build();
var env = app.Environment;

if (env.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();
