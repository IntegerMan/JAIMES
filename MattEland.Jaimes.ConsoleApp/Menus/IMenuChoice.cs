namespace AiTableTopGameMaster.ConsoleApp.Menus;

public interface IMenuChoice
{
    string MenuText { get; }
    Task<ApplicationState> RunAsync();
    int Order { get; }
}