using MobileNotificationService.Services;
using OpenTelemetry.Trace;
using System.Text.Json.Serialization;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);

var rabbitmqEndpoint = builder.Configuration.GetConnectionString("messaging");
builder.Services.AddSingleton<INotificationSender, NotificationSender>();

if (rabbitmqEndpoint is not null)
{
    builder.UseWolverine(opts =>
    {
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
                .AddHttpClientInstrumentation()
                .AddSource("Wolverine");
        });
}
builder.AddServiceDefaults();

var host = builder.Build();
host.Run();
