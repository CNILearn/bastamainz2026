var builder = DistributedApplication.CreateBuilder(args);


var containerenv = builder.AddAzureContainerAppEnvironment("basta-env");

var blobs = builder.AddAzureStorage("mybastastorage")
    .AddBlobContainer("myblobs");

var apiService = builder.AddProject<Projects.WeatherSample_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(blobs).WaitFor(blobs)
    .WithComputeEnvironment(containerenv);

builder.AddProject<Projects.WeatherSample_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
