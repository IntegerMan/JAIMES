using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AiTableTopGameMaster.ConsoleApp.Commands;

[UsedImplicitly]
public class SelectCommandCommand(IAnsiConsole console, IServiceProvider services) : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        IEnumerable<ICommand> commands = services.GetServices<ICommand>();
        
        ICommand command = console.Prompt(
            new SelectionPrompt<ICommand>()
                .Title("Select a command:")
                .PageSize(5)
                .MoreChoicesText("[grey](Move up and down to reveal more commands)[/]")
                .AddChoices(commands.OfType<ChatCommandBase>())
                .UseConverter(c => c.ToString()!)
        );

        return await command.Execute(context, null);
    }
}