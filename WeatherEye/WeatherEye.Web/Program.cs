using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MudBlazor.Services;
using OpenTelemetry.Trace;
using System.IdentityModel.Tokens.Jwt;
using WeatherEye.Web;
using WeatherEye.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add MudBlazor services
builder.Services.AddMudServices();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddHttpContextAccessor()
                .AddTransient<AuthorizationHandler>();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        client.BaseAddress = new("https+http://apiservice");
    }).AddHttpMessageHandler<AuthorizationHandler>();
;

builder.Services.AddHttpClient<CAPApiClient>(client =>
{
    client.BaseAddress = new("https+http://apiservice");
}).AddHttpMessageHandler<AuthorizationHandler>();
;

builder.Services.AddOpenTelemetry()
        .WithTracing(configure =>
        {
            configure
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation();
        });

var oidcScheme = OpenIdConnectDefaults.AuthenticationScheme;
builder.Services.AddAuthentication(oidcScheme)
                .AddKeycloakOpenIdConnect("keycloak", realm: "WeatherEye", oidcScheme, opts =>
                {
                opts.ClientId = "WeatherEyeWeb";
                opts.ResponseType = OpenIdConnectResponseType.Code;
                opts.Scope.Add("weathereye:all");
                opts.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                opts.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
                opts.SaveTokens = true;

                if (!builder.Environment.IsDevelopment())
                {
                    opts.Authority = "https://weathereye.eu/keycloak/realms/WeatherEye";
                }

                opts.RequireHttpsMetadata = false;

                    opts.Events.OnRedirectToIdentityProvider = context =>
                    {
                        var request = context.Request;

                        var forwardedHost = request.Headers["X-Forwarded-Host"].FirstOrDefault();
                        var forwardedProto = request.Headers["X-Forwarded-Proto"].FirstOrDefault();

                        if (!string.IsNullOrEmpty(forwardedHost))
                        {
                            context.ProtocolMessage.RedirectUri = $"{forwardedProto ?? request.Scheme}://{forwardedHost}{context.Options.CallbackPath}";
                        }

                        return Task.CompletedTask;
                    };

                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

builder.Services.AddCascadingAuthenticationState();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.

    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseForwardedHeaders();

app.UseHttpsRedirection();

app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.MapLoginAndLogout();



app.Run();
