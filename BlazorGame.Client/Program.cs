// FICHIER: BlazorGame.Client/Program.cs
// OBJET: Point d'entrée du client Blazor WebAssembly.
// NOTE V1: On garde la config minimale. Les services (API, Auth) arriveront dans les versions suivantes.

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorGame.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Composant racine <App/> dans index.html
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient pour les appels API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5198") });

await builder.Build().RunAsync();
