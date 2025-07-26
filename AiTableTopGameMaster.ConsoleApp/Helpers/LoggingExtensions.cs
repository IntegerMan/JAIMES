using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Filters;

namespace AiTableTopGameMaster.ConsoleApp.Helpers;

public static class LoggingExtensions
{
    public static void AddJaimesAppLogging(this ServiceCollection services)
    {
        string filename = "Adventure";
        DeleteOldLogs(filename);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Logger(l =>
            {
                l.WriteTo.File(
                    new Serilog.Formatting.Json.JsonFormatter(renderMessage: true),
                    path: Path.Combine(Environment.CurrentDirectory, "Logs", $"{filename}.json"),
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose,
                    retainedFileCountLimit: null,
                    rollingInterval: RollingInterval.Infinite
                );
            })
            .WriteTo.Logger(l =>
            {
                l.Filter.ByIncludingOnly(Matching.FromSource("AiTableTopGameMaster"))
                    .WriteTo.File(
                        Path.Combine(Environment.CurrentDirectory, "Logs", $"{filename}.transcript"),
                        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                        rollingInterval: RollingInterval.Infinite,
                        outputTemplate: "{Message:lj}{NewLine}"
                    );
            })
            .CreateLogger();

        services.AddLogging(b => b.ConfigureSerilogLogging(true));
    }

    private static void DeleteOldLogs(string filename)
    {
        FileInfo file = new(Path.Combine(Environment.CurrentDirectory, "Logs", $"{filename}.json"));
        if (file.Exists)
        {
            file.Delete();
        }
        file = new(Path.Combine(Environment.CurrentDirectory, "Logs", $"{filename}.transcript"));
        if (file.Exists)
        {
            file.Delete();
        }
    }

    public static void ConfigureSerilogLogging(this ILoggingBuilder loggingBuilder, bool disposeLogger = false)
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddSerilog(Log.Logger, dispose: disposeLogger);
    }
}