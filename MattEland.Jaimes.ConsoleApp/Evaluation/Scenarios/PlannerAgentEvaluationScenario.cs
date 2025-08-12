using AiTableTopGameMaster.ConsoleApp.Helpers;
using MattEland.Jaimes.Agents;
using MattEland.Jaimes.Agents.Messages;
using MattEland.Jaimes.Agents.Models;
using MattEland.Jaimes.Agents.Processes;
using MattEland.Jaimes.Agents.Steps;
using MattEland.Jaimes.Core.Cores;
using MattEland.Jaimes.Core.Domain;
using MattEland.Jaimes.Core.Evaluation;
using MattEland.Jaimes.Core.Helpers;
using MattEland.Jaimes.Core.Models;
using MattEland.Jaimes.Core.Services;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;

#pragma warning disable SKEXP0080

namespace AiTableTopGameMaster.ConsoleApp.Evaluation.Scenarios;

public class PlannerAgentEvaluationScenario(
    IServiceProvider services,
    IModelFactory modelFactory,
    IAnsiConsole console,
    Adventure adventure,
    Character character,
    IConversationContextService conversation,
    IPromptsService promptsService,
    IKernelBuilder kernelBuilder) : EvaluationScenario
{
    public override string Name => "PlannerAgent Evaluation";

    public override string Message =>
        promptsService.GetInitialGreetingMessage(adventure.CreateChatData());

    protected override string CompletenessGroundTruth
        => """ 
           The response should be a short plan intended to aid the game master in starting a tabletop role-playing game session.
           The response should include CHECKS, KEY POINTS, and CAUTIONS sections with minimal formatting.
           The CHECKS section should indicate that no skill checks or dice rolls are needed at this time
           The KEY POINTS section should include the following:
           - The player character's name is Emcee, a level 1 rogue.
           - Emcee is a smuggler or other form of criminal who has been shipwreck
           - They have washed up on a mysterious island after a shipwreck.
           - Mention the coral on the beach
           - Mention the jungle deeper into the island.
           The CAUTIONS section should provide good reminders for the game master
           The response should be brief and focused on the immediate situation, not the adventure.
           The response should be written as a plan, not a narrative.
           """;

    protected override string EquivalenceGroundTruth
        => """
           CHECKS: Not needed.

           KEY POINTS: The player is Emcee, a level 1 rogue. They were on a smuggling ship called the Silver Minnow and have washed up on a mysterious island after a shipwreck.
           Describe the beach, the mysterious coral, and mention the jungle deeper into the island.

           CAUTIONS: Avoid giving away too much information about the island's mysteries. 
           Keep your response brief and focused on the immediate situation. 
           Ask the player what they want to do, but do not provide a list of options or actions.
           """;

    public override async Task<ChatResult> GetResponseAsync(string message, string modelId)
    {
        adventure.PlayerCharacter = character;

        ChatHistory history = [];
        history.AddUserMessage(message);

        ProcessBuilder kernelProcess = PlannerProcess.Create();
        
        console.WriteMermaidNotation(kernelProcess);
        
        modelFactory.ConfigureKernel(kernelBuilder, Name, modelId, []);
        Kernel kernel = kernelBuilder.Build();

        KernelProcess process = kernelProcess.Build();
        await using LocalKernelProcessContext runningProcess = await process.StartAsync(
            kernel,
            new KernelProcessEvent
            {
                Id = ProcessEvents.StartProcess,
                Visibility = KernelProcessEventVisibility.Public,
                Data = new ConversationMessage(history, adventure, character, services)
            },
            externalMessageChannel: new LoggingExternalMessageChannel(console));
        
        PlanCompleteMessage? result = conversation.GetContext<PlanCompleteMessage>();
        string? json = console.WriteAsJson(result?.Plan);

        return new ChatResult
        {
            History = conversation.GetRequiredContext<ChatHistory>(PlannerStep.RenderedHistoryKey),
            Response = json.AsChatResponse(),
        };
    }
}