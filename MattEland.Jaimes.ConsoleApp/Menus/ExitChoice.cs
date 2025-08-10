using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Menus;

public class ExitChoice(IAnsiConsole console) : IMainMenuChoice
{
    public string MenuText => "Exit Application";
    public int Order => int.MaxValue;
    public Task<ApplicationState> RunAsync()
    {
        console.WriteLine("Thank you for using JAIMES. Sorry if I inflicted a total party kill on you.");
        
        return Task.FromResult(ApplicationState.Terminating);
    }
}