var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Session)
    ;




var keycloak = builder.AddKeycloakContainer("keycloak")
    .WithContainerName("keycloak")
    .WithDataVolume("keycloakWeatherEyeDataVolume")
    //.WithImport("./KeycloakConfiguration/Test-realm.json")
    //.WithImport("./KeycloakConfiguration/Test-users-0.json");
;

var realm = keycloak.AddRealm("WeatherEye-public");

var rabbitmq = builder.AddRabbitMQ("messaging")
    .WithContainerName("rabbitmqWeatherEye")
    .WithDataVolume("rabbitmqWeatherEyeDataVolume")
    .WithManagementPlugin()
    ;


var apiService = builder.AddProject<Projects.WeatherEye_API>("apiservice")
    .WithReference(keycloak)
    .WithReference(realm)
    .WaitFor(keycloak)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(cache)
    .WaitFor(cache)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.WeatherEye_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(keycloak)
    .WithReference(realm)
    .WaitFor(keycloak)
    .WithReference(apiService)
    .WaitFor(apiService);


var postgre = builder.AddPostgres("postgre")
    .WithContainerName("postgre")
    .WithDataVolume("postgreData")
    .WithPgAdmin()
    ;

var chmiAlertProviderDB = postgre.AddDatabase("chmiAlertDB");

builder.AddProject<Projects.ChmiCapAlertProvider>("chmicapalertprovider")

    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(chmiAlertProviderDB)
    .WaitFor(chmiAlertProviderDB)
    ;



builder.Build().Run();
