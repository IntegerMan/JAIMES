using AiTableTopGameMaster.ConsoleApp.Clients;
using AiTableTopGameMaster.Domain;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Spectre.Console.Cli;

namespace AiTableTopGameMaster.ConsoleApp.Commands;

[UsedImplicitly]
public class ChatCommand(IConsoleChatClient consoleChat, Adventure adventure) : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        ICollection<ChatMessage> history = [
            new(ChatRole.System, $"You are an AI game master for a tabletop role-playing game of Dungeons and Dragons 5th Edition. You will interact with the player, who is a human, and provide responses to their queries and actions. You are working through a short demonstration adventure called {adventure.Name} by {adventure.Author}, but have liberty to improvise and create new content as needed. Keep things fair and challenging and drive the story forward. Let the player tell you what they want, then interpret their response. Do not suggestion actions to the player or take actions on their behalf unless they are blatantly obvious."), 
            new(ChatRole.Tool, $"Here is the adventure backstory: {adventure.Backstory}"),
            new(ChatRole.User, "Hello, I'm playing a level 1 rogue. Please start our adventure."),
        ];
        await consoleChat.ChatIndefinitelyAsync(history);

        return 0;
    }
}