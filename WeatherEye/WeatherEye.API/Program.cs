using ImTools;
using JasperFx.Core;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddRedisOutputCache("cache");

services.AddAuthorization();


services.AddAuthentication()

                .AddKeycloakJwtBearer(
                    serviceName: "keycloak",
                    realm: "WeatherEye",
                    options =>
                    {
                        options.Audience = "account";
                        options.RequireHttpsMetadata = false;

                        if (!builder.Environment.IsDevelopment())
                        {
                            options.Authority = "https://weathereye.eu/realms/WeatherEye";
                        }
                        //if (builder.Environment.IsDevelopment())
                        //{
                        //}
                    });


var rabbitmqEndpoint = builder.Configuration.GetConnectionString("messaging");

if (rabbitmqEndpoint is not null)
{
    builder.Host.UseWolverine(opts =>
    {
        // Important! Convert the "connection string" up above to a Uri
        opts.UseRabbitMq(new Uri(rabbitmqEndpoint))
        .AutoProvision()
        .UseConventionalRouting();

        opts.UseSystemTextJsonForSerialization(stj =>
        {
            stj.UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode;
            stj.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            stj.IncludeFields = true;
        });
    });
    builder.Services.AddOpenTelemetry()
        .WithTracing(configure =>
        {
            configure
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("Wolverine");
        });

}


builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});


var app = builder.Build();

app.UseForwardedHeaders();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//    app.MapScalarApiReference();
//}
app.MapOpenApi();
app.MapScalarApiReference();
app.UseOutputCache();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
