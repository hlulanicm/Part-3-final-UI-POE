using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Diagnostics;
using System.Net.Sockets;
using RaceDay.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC with automatic anti-forgery checks on every POST
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

builder.Services.Configure<RazorViewEngineOptions>(options =>
    options.ViewLocationFormats.Add("/Views/Accounts/{0}.cshtml"));

// Session keeps the user's login state and role between pages
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// Typed HttpClient that talks to the Part 2 API
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException("ApiSettings:BaseUrl is missing.");
var trustApiCertificate = builder.Configuration.GetValue<bool>("ApiSettings:TrustApiCertificate")
    || builder.Environment.IsDevelopment();

builder.Services.AddHttpClient<ApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    // Cookies are handled by ApiService so each user keeps their own API cookie
    var handler = new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false };

    // Accepts the API's development certificate when running in Docker
    if (trustApiCertificate)
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

    return handler;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// The container only listens on http, so the https redirect is skipped there
if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
    app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// Starts the API by itself when it is not already running, so running only the website just works
if (app.Environment.IsDevelopment()
    && Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true"
    && Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var apiUri)
    && (apiUri.Host == "localhost" || apiUri.Host == "127.0.0.1"))
{
    static bool PortOpen(int port)
    {
        try
        {
            using var client = new TcpClient();
            return client.ConnectAsync("localhost", port).Wait(500) && client.Connected;
        }
        catch { return false; }
    }

    if (!PortOpen(apiUri.Port))
    {
        // Looks upwards from the build folder for API_Proj/API_Proj.csproj
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        string? apiProject = null;
        while (dir != null && apiProject == null)
        {
            var candidate = Path.Combine(dir.FullName, "API_Proj", "API_Proj.csproj");
            if (File.Exists(candidate)) apiProject = candidate;
            dir = dir.Parent;
        }

        if (apiProject != null)
        {
            Console.WriteLine("Starting the RaceDay API...");
            var api = Process.Start(new ProcessStartInfo("dotnet", $"run --project \"{apiProject}\" --launch-profile https")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });

            // Stops the API again when the website stops
            app.Lifetime.ApplicationStopping.Register(() =>
            {
                try { if (api is { HasExited: false }) api.Kill(entireProcessTree: true); } catch { }
            });

            // Waits (up to 2 minutes) so the browser only opens once the API is ready
            for (var i = 0; i < 120 && !PortOpen(apiUri.Port); i++)
                Thread.Sleep(1000);
        }
    }
}

app.Run();
