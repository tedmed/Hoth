
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using JasperFx.MultiTenancy;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);

var rabbitmqEndpoint = builder.Configuration.GetConnectionString("messaging");

builder.AddNpgsqlDataSource(connectionName: "chmiAlertDB");

if (rabbitmqEndpoint is not null)
{
    builder.UseWolverine(opts =>
    {
        // Important! Convert the "connection string" up above to a Uri
        opts.UseRabbitMq(new Uri(rabbitmqEndpoint)).AutoProvision();
    });
}
var conString = builder.Configuration.GetConnectionString("chmiAlertDB");

if (conString is null) throw new ArgumentNullException("ConnectionString:chmiAlertDB");

Dictionary<string, string> conStringParsed = new();

foreach (var item in conString.Split(";"))
{
    var val = item.Split("=");
    conStringParsed.Add(val[0], val[1]);
}

var finalConnectionString =
    PostgreSqlConnectionProvider.GetConnectionString(
        conStringParsed["Host"],
        Convert.ToInt32(conStringParsed["Port"]),
        conStringParsed["Username"],
        conStringParsed["Password"],
        conStringParsed["Database"]
    );

XpoDefault.DataLayer = XpoDefault.GetDataLayer(finalConnectionString, AutoCreateOption.DatabaseAndSchema);

builder.AddServiceDefaults();
builder.Services.AddHostedService<ChmiCapService>();

var host = builder.Build();
host.Run();
