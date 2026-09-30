using Codebreaker.AppHost.Extensions;
using Codebreaker.ServiceDefaults;
using Aspire.Hosting.AWS.Deployment;

using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

CodebreakerSettings settings = new();
builder.Configuration.GetSection("CodebreakerSettings").Bind(settings);

#pragma warning disable ASPIREAWSPUBLISHERS001
switch (settings.DeploymentTarget)
{
    case DeploymentTargetType.AzureContainerApps:
        builder.AddAzureContainerAppEnvironment("aca");
        break;
    case DeploymentTargetType.AzureAppService:
        builder.AddAzureAppServiceEnvironment("appservice");
        break;
    case DeploymentTargetType.AzureAks:
        builder.AddAzureKubernetesEnvironment("aks");
        break;
    case DeploymentTargetType.AwsEcsFargate:
        builder.AddAWSCDKEnvironment(
            name: "Codebreaker",
            cdkDefaultsProviderFactory: CDKDefaultsProviderFactory.Preview_V1);
        break;
    case DeploymentTargetType.AwsEks:
        builder.AddKubernetesEnvironment("eks");
        break;
    default:
        throw new NotSupportedException($"Deployment target {settings.DeploymentTarget} is not supported.");
}
#pragma warning restore ASPIREAWSPUBLISHERS001

var gameApis = builder.AddProject<Projects.Codebreaker_GameAPIs>("gameapis")
    .WithHttpHealthCheck("/health")
    .WithEnvironment(EnvVarNames.DataStore, settings.DataStore.ToString())
    .WithExternalHttpEndpoints();

switch (settings.DataStore)
{
    case DataStoreType.InMemory:
        // no action needed, in-memory is the default
        break;
    case DataStoreType.SqlServer:
        builder.ConfigureSqlServer(gameApis);
        break;
    case DataStoreType.Cosmos:
        builder.ConfigureCosmos(gameApis, settings.UseEmulator);
        break;
    case DataStoreType.Postgres:
        builder.ConfigurePostgres(gameApis);
        break;
    default:
        throw new NotSupportedException($"DataStore {settings.DataStore} is not supported.");
}

builder.Build().Run();
