using JasperFx.Core;
using Keycloak.AuthServices.Authentication;
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

services.AddKeycloakWebApiAuthentication(
    configuration,
    options =>
    {
        options.Audience = "workspaces-client";
        options.RequireHttpsMetadata = false;
    }
);
services.AddAuthorization();

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



var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseOutputCache();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
