using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace AiTableTopGameMaster.ConsoleApp.Helpers;

public static class LoggingExtensions
{
    public static void AddAigmAppLogging(this ServiceCollection services)
    {
        DateTimeOffset now = DateTimeOffset.Now;
        DateOnly today = DateOnly.FromDateTime(now.Date);
        Guid fileId = Guid.CreateVersion7(now);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Combine(Environment.CurrentDirectory, "Logs", $"{today:O}-{fileId}.log"),
                rollingInterval: RollingInterval.Infinite,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                Path.Combine(Environment.CurrentDirectory, "Logs", $"{today:O}-{fileId}.transcript"),
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                outputTemplate: "{Message:lj}{NewLine}",
                rollingInterval: RollingInterval.Infinite)
            .CreateLogger();
        
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(Log.Logger, dispose: true);
        });
    }
}