using System.Reflection;
using AiTableTopGameMaster.Core.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AiTableTopGameMaster.ConsoleShared.Settings;

public static class ConfigurationExtensions
{
    public static AppSettings RegisterConfigurationAndSettings(this ServiceCollection services, string[] args)
    {
        Assembly entry = Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Entry assembly not found. Ensure this is called from the main application assembly.");
        
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .AddUserSecrets(entry, optional: true)
            .AddCommandLine(args)
            .Build();
        
        services.Configure<AppSettings>(config);
        services.Configure<OllamaSettings>(config.GetSection("Ollama"));
        services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<AppSettings>>().Value);
        services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<OllamaSettings>>().Value);

        // Get the AppSettings instance to ensure it's loaded
        return config.Get<AppSettings>()
                     ?? throw new InvalidOperationException("AppSettings are not configured properly.");
    }
}