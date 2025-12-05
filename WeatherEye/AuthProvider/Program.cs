
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Core;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.DC.Xpo;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.ClientServer;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;
using JasperFx.MultiTenancy;
using OpenTelemetry.Trace;
using System.Drawing.Text;
using System.Text.Json.Serialization;
using UserService.Services;
using Wolverine;
using Wolverine.RabbitMQ;

const string dbConnectionName = "UserDB";

var builder = Host.CreateApplicationBuilder(args);

var rabbitmqEndpoint = builder.Configuration.GetConnectionString("messaging");

builder.AddNpgsqlDataSource(connectionName: dbConnectionName);

if (rabbitmqEndpoint is not null)
{
    builder.UseWolverine(opts =>
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
                .AddHttpClientInstrumentation()
                .AddSource("Wolverine");
        });
}
var conString = builder.Configuration.GetConnectionString(dbConnectionName);

if (conString is null) throw new ArgumentNullException($"ConnectionString:{dbConnectionName}");


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

XpoDefault.Session = null;
string conn = finalConnectionString;
conn = XpoDefault.GetConnectionPoolString(conn);
XPDictionary dict = new ReflectionDictionary();
IDataStore store = XpoDefault.GetConnectionProvider(conn, AutoCreateOption.DatabaseAndSchema);
dict.GetDataStoreSchema(System.Reflection.Assembly.GetExecutingAssembly());
IDataLayer dl = new ThreadSafeDataLayer(dict, store);

XpoDefault.DataLayer = dl;
XpoDefault.Session = null;

builder.AddServiceDefaults();


var host = builder.Build();
host.Run();



void RegisterEntities(TypesInfo typesInfo)
{
    typesInfo.GetOrAddEntityStore(ti => new XpoTypeInfoSource(ti));

    //typesInfo.RegisterEntity(typeof(Employee));
    typesInfo.RegisterEntity(typeof(PermissionPolicyUser));
    typesInfo.RegisterEntity(typeof(PermissionPolicyRole));
}