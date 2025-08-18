using System.Reflection;
using MattEland.Jaimes.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AiTableTopGameMaster.ConsoleApp.Helpers;

public static class ConfigurationExtensions
{
    public static void RegisterConfigurationAndSettings(this ServiceCollection services, string[] args) 
    {
        Assembly entry = Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Entry assembly not found. Ensure this is called from the main application assembly.");
        
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .AddUserSecrets(entry, optional: true)
            .AddCommandLine(args)
            .Build();

        AppSettings settings = config.Get<AppSettings>() ?? throw new InvalidOperationException("Settings are not configured properly.");

        services.Configure<AppSettings>(config);
        services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<AppSettings>>().Value);
        services.AddKeyedSingleton(serviceKey: "ModelServiceAssignments", settings.ModelServiceAssignments);
    }
}