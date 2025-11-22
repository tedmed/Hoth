

using Aspire.Hosting;
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
        .WithHostPort(64725)

    .WithPgAdmin();


var postgreKC = builder.AddPostgres("postgreKC")
    .WithContainerName("postgreKC")
    .WithDataVolume("postgreKCData")
        .WithHostPort(64726)
    .WithPgAdmin();

var kcDb = postgreKC.AddDatabase("postgres", "postgres");



var keycloakDbUrl = ReferenceExpression.Create(

 $"jdbc:postgresql://{postgreKC.Resource.PrimaryEndpoint.Property(EndpointProperty.Host)}/{kcDb.Resource.DatabaseName}"
);

var keycloak = builder.AddKeycloak("keycloak", 8081)
    .WithEndpoint("http", e => e.IsExternal = true)
    //.WithHttpsEndpoint(9443,9443)
    .WithReference(kcDb)
    .WaitFor(kcDb)
    .WithArgs("--verbose")

    .WithEnvironment("KC_HTTP_ENABLED", "true")

    .WithEnvironment("KC_PROXY_HEADERS", "xforwarded")
    .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    // DB nastavení
    .WithEnvironment("KC_DB", "postgres")
    .WithEnvironment("KC_DB_URL", keycloakDbUrl)
    .WithEnvironment("KC_DB_USERNAME", postgreKC.Resource.UserNameReference)
    .WithEnvironment("KC_DB_PASSWORD", postgreKC.Resource.PasswordParameter);


if (builder.Environment.IsDevelopment() == false)
{
    keycloak
    // Reverse proxy nastavení
    //.WithEnvironment("KC_PROXY", "edge")
    //.WithEnvironment("KC_PROXY_PROTOCOL_ENABLED", "true")

    //.WithEnvironment("KC_HOSTNAME_STRICT_HTTPS", "false")
    .WithEnvironment("KC_HOSTNAME_BACKCHANNEL_DYNAMIC", "true")

    .WithEnvironment("KC_HOSTNAME", "https://auth.weathereye.eu/");
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
    .WithExternalHttpEndpoints()
    .WithReference(keycloak)
    //.WithReference(realm)
    .WaitFor(keycloak)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(cache)
    .WaitFor(cache)
    .PublishAsDockerComposeService((resource, service) =>
    {
        service.Name = "apiservice";
        service.Ports.Add("7721:7721");
        service.Restart = "unless-stopped";
    });

var webfrontend = builder.AddProject<Projects.WeatherEye_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(keycloak)
    //.WithReference(realm)
    .WaitFor(keycloak)
    .WithReference(apiService)
    .WaitFor(apiService)
    .PublishAsDockerComposeService((resource, service) =>
    {
        service.Name = "webfrontend";
        service.Ports.Add("7722:7722");
        service.Restart = "unless-stopped";
    });


var chmiAlertProviderDB = postgre.AddDatabase("chmiAlertDB");

builder.AddProject<Projects.ChmiCapAlertProvider>("chmicapalertprovider")

    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(chmiAlertProviderDB)
    .WaitFor(chmiAlertProviderDB)
    .PublishAsDockerComposeService((resource, service) =>
    {
        service.Name = "chmicapalertprovider";
        service.Restart = "unless-stopped";
    });

var postgreUser = builder.AddPostgres("postgreUser")
    .WithContainerName("postgreUser")
    .WithDataVolume("postgreUserData")
    .WithHostPort(64727)
    .WithPgAdmin();

var userDB = postgreUser.AddDatabase("UserDB");

builder.AddProject<Projects.UserService>("userservice")
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(userDB)
    .WaitFor(userDB)
    .PublishAsDockerComposeService((resource, service) =>
    {
        service.Name = "userservice";
        service.Restart = "unless-stopped";
    });




builder.Build().Run();
