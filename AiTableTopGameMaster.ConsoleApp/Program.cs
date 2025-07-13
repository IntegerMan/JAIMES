using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Spectre.Console;

IAnsiConsole console = AnsiConsole.Console;
IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config.Sources.Clear();
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>(optional: true)
            .AddCommandLine(args);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton(console);
        services.Configure<AppSettings>(context.Configuration);
        services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<AppSettings>>().Value);
    })
    .Build();

console.MarkupLine("[green]Welcome to the AI TableTop Game Master![/]");

AppSettings config = host.Services.GetRequiredService<AppSettings>();
console.MarkupLine($"[yellow]Setting1:[/] {config.Setting1}, [yellow]Setting2:[/] {config.Setting2}");