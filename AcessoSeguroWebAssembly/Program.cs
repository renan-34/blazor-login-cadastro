using AcessoSeguroWebAssembly;
using AcessoSeguroWebAssembly.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Headers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient base
builder.Services.AddScoped(sp =>
{
    var client = new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7172") // 🔁 endereço da API backend
    };

    return client;
});

// Registrar o serviço de autenticação
builder.Services.AddScoped<AuthService>();

await builder.Build().RunAsync();


