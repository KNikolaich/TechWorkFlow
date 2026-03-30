using System.Net.Http.Headers;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Frontend;
using Frontend.Services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress);

builder.Services.AddScoped(sp =>
{
    var client = new HttpClient { BaseAddress = apiBaseAddress };
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    return client;
});

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddMudServices();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();
builder.Services.AddScoped<IAuthServiceClient, AuthServiceClient>();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

await builder.Build().RunAsync();
