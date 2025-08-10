namespace AiTableTopGameMaster.ConsoleApp.Menus;

public interface IMainMenuChoice
{
    string MenuText { get; }
    Task<ApplicationState> RunAsync();
    int Order { get; }
}