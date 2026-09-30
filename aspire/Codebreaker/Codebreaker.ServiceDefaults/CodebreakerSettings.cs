using Microsoft.Extensions.Configuration;

namespace Codebreaker.ServiceDefaults;

public enum DataStoreType
{
    InMemory,
    DistributedMemory,
    SqlServer,
    Cosmos,
    Postgres,
    MongoDb
}

public enum DeploymentTargetType
{
    AzureContainerApps,
    AzureAppService,
    AzureAks,
    AwsEcsFargate,
    AwsEks
}

public enum EmulatorOption
{
    PreferDocker,
    PreferLocal,
    UseAzure
}

public class CodebreakerSettings
{
    public DataStoreType DataStore { get; set; } = DataStoreType.InMemory;
    public EmulatorOption UseEmulator { get; set; } = EmulatorOption.PreferDocker;
    public DeploymentTargetType DeploymentTarget { get; set; } = DeploymentTargetType.AzureContainerApps;
}

public static class ConfigurationExtensions
{
    public static DataStoreType GetDataStore(this IConfiguration configuration)
    {
        string? dataStore = configuration.GetValue<string>(EnvVarNames.DataStore);
        return Enum.TryParse(dataStore, out DataStoreType result) ? result : DataStoreType.InMemory;
    }

    public static DeploymentTargetType GetDeploymentTarget(this IConfiguration configuration)
    {
        string? deploymentTarget = configuration.GetValue<string>($"CodebreakerSettings:{EnvVarNames.DeploymentTarget}");
        return Enum.TryParse(deploymentTarget, out DeploymentTargetType result)
            ? result
            : DeploymentTargetType.AzureContainerApps;
    }
}