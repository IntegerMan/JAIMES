using System.Reflection;
using AiTableTopGameMaster.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AiTableTopGameMaster.ConsoleShared.Settings;

public static class ConfigurationExtensions
{
    public static TSettings RegisterConfigurationAndSettings<TSettings>(this ServiceCollection services, string[] args) where TSettings : class, ISettingsRoot
    {
        Assembly entry = Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Entry assembly not found. Ensure this is called from the main application assembly.");
        
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .AddUserSecrets(entry, optional: true)
            .AddCommandLine(args)
            .Build();

        TSettings settings = config.Get<TSettings>() ?? throw new InvalidOperationException("Settings are not configured properly.");

        services.Configure<TSettings>(config);
        services.AddSingleton<ISettingsRoot>(settings);
        services.AddSingleton(settings.AzureOpenAI);
        services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<TSettings>>().Value);

        return settings;
    }
}