using Altinn.Authorization.ServiceDefaults;
using IdProvider.Configuration;
using IdProvider.Services;
using IdProvider.Services.Implementation;
using IdProvider.Services.Interface;
using IdProvider.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = AltinnHost.CreateWebApplicationBuilder("idprovider", args);
var services = builder.Services;

services.AddOptions<GeneralSettings>()
    .BindConfiguration("GeneralSettings")
    .ValidateOnStart();

services.AddOptions<KeyVaultSettings>()
    .BindConfiguration("kvSetting")
    .ValidateOnStart();

services.AddControllersWithViews();

services.AddSingleton<IJwtSigningCertificateProvider, JwtSigningCertificateProvider>();
services.AddSingleton<IToken, TokenService>();
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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();
