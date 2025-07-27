using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AiTableTopGameMaster.ConsoleShared.Settings;

public static class ConfigurationExtensions
{
    public static TSettings RegisterConfigurationAndSettings<TSettings>(this ServiceCollection services, string[] args) where TSettings : class
    {
        Assembly entry = Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Entry assembly not found. Ensure this is called from the main application assembly.");
        
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .AddUserSecrets(entry, optional: true)
            .AddCommandLine(args)
            .Build();
        
        services.Configure<TSettings>(config);
        services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<TSettings>>().Value);

        return config.Get<TSettings>() ?? throw new InvalidOperationException("Settings are not configured properly.");
    }
}