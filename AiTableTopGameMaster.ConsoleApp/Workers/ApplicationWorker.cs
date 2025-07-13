using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Workers;

public class ApplicationWorker(IAnsiConsole console, AppSettings settings, IChatClient chat) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        console.MarkupLine("[green]Welcome to the AI TableTop Game Master![/]");

        console.MarkupLine($"[yellow]Setting1:[/] {settings.Ollama.ChatEndpoint}, [yellow]Setting2:[/] {settings.Ollama.ChatModelId}");
        
        console.MarkupLine("[blue]Chat Client initialized successfully![/]");
        
        await console.Status().StartAsync("Generating greeting", async _ =>
        {
            ChatMessage message = new(ChatRole.System,
                "Greet the player and introduce yourself as the AI TableTop Game Master.");
            ChatMessage greeting = new(ChatRole.User, "Hello, I'm the player. Can you introduce yourself?");
            
            ChatResponse response = await chat.GetResponseAsync([message, greeting], cancellationToken: cancellationToken);
            
            console.MarkupLineInterpolated($"[green]AI Response:[/] {response.Text}");
        });
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}