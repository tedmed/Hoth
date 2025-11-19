
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
using JasperFx.MultiTenancy;
using OpenTelemetry.Trace;
using System.Drawing.Text;
using System.Text.Json.Serialization;
using UserService.Services;
using Wolverine;
using Wolverine.RabbitMQ;

const string dbConnectionName = "AuthProviderDB";

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
//builder.Services.AddHostedService<ChmiCapService>();
builder.Services
    .AddSingleton<ITypesInfo>(s => {
        var typesInfo = XafTypesInfo.Instance;
        XpoTypesInfoHelper.ForceInitialize((TypesInfo)typesInfo);
        typesInfo.RegisterEntity(typeof(PermissionPolicyUser));
        typesInfo.RegisterEntity(typeof(PermissionPolicyRole));
        return typesInfo;
    })
    .AddScoped<IObjectSpaceProviderFactory, ObjectSpaceProviderFactory>()
    .AddSingleton<IXpoDataStoreProvider>((serviceProvider) => {
        return XPObjectSpaceProvider.GetDataStoreProvider(finalConnectionString, null, true);
    });

//TypesInfo typesInfo = new TypesInfo();
//RegisterEntities(typesInfo);

//IXpoDataStoreProvider dataStoreProvider = XPObjectSpaceProvider.GetDataStoreProvider(finalConnectionString, null);

//AuthenticationStandard authentication = new AuthenticationStandard();
//SecurityStrategyComplex security = new SecurityStrategyComplex(typeof(PermissionPolicyUser), typeof(PermissionPolicyRole), authentication, typesInfo);
//security.RegisterXPOAdapterProviders();

//SecuredObjectSpaceProvider objectSpaceProvider = new SecuredObjectSpaceProvider(security, dataStoreProvider, typesInfo, null);


//authentication.SetLogonParameters(new AuthenticationStandardLogonParameters(userName: "User", password: string.Empty));
//IObjectSpace loginObjectSpace = objectSpaceProvider.CreateObjectSpace();
//security.Logon(loginObjectSpace);



var host = builder.Build();
host.Run();



void RegisterEntities(TypesInfo typesInfo)
{
    typesInfo.GetOrAddEntityStore(ti => new XpoTypeInfoSource(ti));

    //typesInfo.RegisterEntity(typeof(Employee));
    typesInfo.RegisterEntity(typeof(PermissionPolicyUser));
    typesInfo.RegisterEntity(typeof(PermissionPolicyRole));
}