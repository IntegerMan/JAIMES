using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Filters;

namespace AiTableTopGameMaster.ConsoleApp.Helpers;

public static class LoggingExtensions
{
    public static void AddAigmAppLogging(this ServiceCollection services)
    {
        DateTimeOffset now = DateTimeOffset.Now;
        DateOnly today = DateOnly.FromDateTime(now.Date);
        Guid fileId = Guid.CreateVersion7(now);

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Logger(l =>
            {
                l.WriteTo.File(
                    new Serilog.Formatting.Json.JsonFormatter(renderMessage: true),
                    Path.Combine(Environment.CurrentDirectory, "Logs", $"{today:O}-{fileId}.json"),
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose,
                    rollingInterval: RollingInterval.Infinite
                );
            })
            .WriteTo.Logger(l =>
            {
                l.Filter.ByIncludingOnly(Matching.FromSource("AiTableTopGameMaster"))
                    .WriteTo.File(
                        Path.Combine(Environment.CurrentDirectory, "Logs", $"{today:O}-{fileId}.transcript"),
                        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                        rollingInterval: RollingInterval.Infinite,
                        outputTemplate: "{Message:lj}{NewLine}"
                    );
            })
            .CreateLogger();

        services.AddLogging(b => b.ConfigureSerilogLogging(true));
    }

    public static void ConfigureSerilogLogging(this ILoggingBuilder loggingBuilder, bool disposeLogger = false)
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddSerilog(Log.Logger, dispose: disposeLogger);
    }
}