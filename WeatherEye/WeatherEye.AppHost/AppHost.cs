

using Aspire.Hosting;
using Aspire.Hosting.Yarp.Transforms;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var compose = builder.AddDockerComposeEnvironment("env")
    .WithDashboard(dashboard =>
    {
        dashboard.WithHostPort(8082)
                 .WithForwardedHeaders(enabled: true);

    });



var cache = builder.AddRedis("cache")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Session);

var postgre = builder.AddPostgres("postgre")
    .WithContainerName("postgre")
    .WithDataVolume("postgreData")
    .WithPgAdmin();


var postgreKC = builder.AddPostgres("postgreKC")
    .WithContainerName("postgreKC")
    .WithDataVolume("postgreKCData")
    .WithPgAdmin();

var kcDb = postgreKC.AddDatabase("postgres", "postgres");



var keycloakDbUrl = ReferenceExpression.Create(

 $"jdbc:postgresql://{postgreKC.Resource.PrimaryEndpoint.Property(EndpointProperty.Host)}/{kcDb.Resource.DatabaseName}"
);

var keycloak = builder.AddKeycloak("keycloak", 8081)
    .WithEndpoint("http", e => e.IsExternal = true)
    .WithReference(kcDb)
    .WaitFor(kcDb)
    .WithArgs("--verbose")

    .WithEnvironment("KC_HTTP_ENABLED", "true")


    // DB nastavení
    .WithEnvironment("KC_DB", "postgres")
    .WithEnvironment("KC_DB_URL", keycloakDbUrl)
    .WithEnvironment("KC_DB_USERNAME", postgreKC.Resource.UserNameReference)
    .WithEnvironment("KC_DB_PASSWORD", postgreKC.Resource.PasswordParameter);


if(builder.Environment.IsDevelopment() == false)
{
    keycloak
    // Reverse proxy nastavení
    //.WithEnvironment("KC_PROXY", "edge")
    //.WithEnvironment("KC_PROXY_PROTOCOL_ENABLED", "true")
    .WithEnvironment("KC_PROXY_HEADERS", "xforwarded")
    .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    //.WithEnvironment("KC_HOSTNAME_STRICT_HTTPS", "false")

    .WithEnvironment("KC_HOSTNAME", "https://weathereye.eu/keycloak")
    .WithEnvironment("KC_HOSTNAME_BACKCHANNEL_DYNAMIC", "true");
    // Nejzásadnìjší — nastaví venkovní URL HOST NAME
    //.WithEnvironment("KC_HOSTNAME", "weathereye.eu")
    //.WithEnvironment("KC_HOSTNAME_PORT","443")

    // A BASE PATH za reverse proxy:
    //.WithEnvironment("KC_HOSTNAME_PATH", "/keycloak")

}

if (builder.ExecutionContext.IsRunMode)
{
    keycloak.WithDataVolume()
            .WithRealmImport("KeycloakConfiguration/realm-WeatherEye.json");
}




var rabbitmq = builder.AddRabbitMQ("messaging")
    .WithContainerName("rabbitmqWeatherEye")
    .WithDataVolume("rabbitmqWeatherEyeDataVolume")
    .WithOtlpExporter()
    .WithManagementPlugin();


var apiService = builder.AddProject<Projects.WeatherEye_API>("apiservice")
    .WithHttpHealthCheck("/health")

    .WithReference(keycloak)
    //.WithReference(realm)
    .WaitFor(keycloak)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(cache)
    .WaitFor(cache);

var webfrontend = builder.AddProject<Projects.WeatherEye_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(keycloak)
    //.WithReference(realm)
    .WaitFor(keycloak)
    .WithReference(apiService)
    .WaitFor(apiService);



var chmiAlertProviderDB = postgre.AddDatabase("chmiAlertDB");

builder.AddProject<Projects.ChmiCapAlertProvider>("chmicapalertprovider")

    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(chmiAlertProviderDB)
    .WaitFor(chmiAlertProviderDB);
;

var gateway = builder.AddYarp("gateway")
                     .WithHostPort(8080)
                     .WithOtlpExporter()
                     .WithConfiguration(yarp =>
                     {

                         // Configure routes programmatically
                         yarp.AddRoute("{**catch-all}", webfrontend)
                         .WithTransformXForwarded(
                             xHost: Yarp.ReverseProxy.Transforms.ForwardedTransformActions.Set,
                             xProto: Yarp.ReverseProxy.Transforms.ForwardedTransformActions.Set
                             )
                          .WithTransformForwarded()
                          .WithTransformRequestHeader("X-Forwarded-Host", "weathereye.eu")
                          .WithTransformRequestHeader("X-Forwarded-Proto", "https");

                         yarp.AddRoute("/keycloak/{**catch-all}", keycloak)
                         .WithTransformPathRemovePrefix("/keycloak")
                         .WithTransformXForwarded()
                         .WithTransformForwarded()
                         
                         .WithTransformRequestHeader("X-Forwarded-Port", "443")
                         .WithTransformRequestHeader("X-Forwarded-Prefix", "/keycloak");
                        

                         yarp.AddRoute("/api/{**catch-all}", apiService)
                         .WithTransformPathRemovePrefix("/api")
                         .WithTransformXForwarded()
                         .WithTransformForwarded();

                     })
                     .WaitFor(webfrontend)
                     .WaitFor(apiService)
                     .WaitFor(keycloak)
                     .PublishAsDockerComposeService((resource, service) =>
                     {
                         service.Name = "gateway";
                         service.Ports.Add("8080:5000");
                     });


webfrontend.WithReference(gateway);
apiService.WithReference(gateway);


builder.Build().Run();
