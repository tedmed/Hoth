var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    .WithContainerName("redis")
    .WithDataVolume("redisdata")
    .WithLifetime(ContainerLifetime.Session)
    ;

var apiService = builder.AddProject<Projects.WeatherEye_AlertProvider>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.WeatherEye_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);



builder.Build().Run();
