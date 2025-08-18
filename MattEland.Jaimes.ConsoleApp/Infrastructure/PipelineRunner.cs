using MattEland.Jaimes.Agents;
using MattEland.Jaimes.Agents.Messages;
using MattEland.Jaimes.Agents.Models;
using MattEland.Jaimes.Core.Domain;
using MattEland.Jaimes.Core.Evaluation;
using MattEland.Jaimes.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

#pragma warning disable SKEXP0080

namespace AiTableTopGameMaster.ConsoleApp.Infrastructure;

public class PipelineRunner(IServiceProvider services)
{
    public async Task RunAsync(ProcessBuilder processBuilder, ChatHistory history, Adventure adventure)
    {
        AppSettings settings = services.GetRequiredService<AppSettings>();
        OrchestrationConfiguration configuration = new()
        {
            ModelServiceAssignments = settings.ModelServiceAssignments
        };
        
        IKernelBuilder kernelBuilder = services.GetRequiredService<IKernelBuilder>();
        kernelBuilder.Services.AddSingleton(configuration);
        kernelBuilder.Services.AddSingleton(services.GetRequiredService<EvaluationManager>());
        kernelBuilder.Services.AddSingleton(services.GetRequiredService<IEventsService>());
        kernelBuilder.Services.AddSingleton(services.GetRequiredService<IConversationContextService>());
        Kernel kernel = kernelBuilder.Build();
        
        KernelProcess process = processBuilder.Build();
        services.GetRequiredService<IEventsService>().SendMessage(new ProcessCreatedMessage
        {
            Configuration = configuration,
            Name = processBuilder.Name,
            Process = process
        });

        ConversationMessage startMessage = new()
        {
            History = history,
            Adventure = adventure,
            Character = adventure.PlayerCharacter ?? throw new ArgumentException("Player character is required to run the process.", nameof(adventure)),
            Configuration = configuration
        };
        KernelProcessEvent initialEvent = new()
        {
            Id = ProcessEvents.StartProcess,
            Visibility = KernelProcessEventVisibility.Public,
            Data = startMessage
        };
        
        await using LocalKernelProcessContext runningProcess = await process.StartAsync(
            kernel,
            initialEvent,
            externalMessageChannel: services.GetService<IExternalKernelProcessMessageChannel>());
    }
}