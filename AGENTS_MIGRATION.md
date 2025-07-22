# Semantic Kernel Agents Framework Migration

This document explains the migration from direct `IChatCompletionService` usage to Semantic Kernel's agents framework and outlines extension points for future multi-agent coordination.

## Migration Overview

The application has been migrated from using `IChatCompletionService` directly to using Semantic Kernel's agents framework. This provides better structure for conversation management and enables future multi-agent coordination.

### Before: Direct Service Usage
```csharp
IChatCompletionService chatService = kernel.GetRequiredService<IChatCompletionService>();
IReadOnlyList<ChatMessageContent>? response = await chatService.GetChatMessageContentsAsync(
    history, settings, kernel: kernel, cancellationToken: cancellationToken);
```

### After: Agent-Based Communication
```csharp
IAgentChatClient client = services.GetRequiredService<IAgentChatClient>();
await client.ChatIndefinitelyAsync(history);
```

## Key Components

### 1. IAgentChatClient Interface
- Replaces `IConsoleChatClient` for agent-based communication
- Provides access to the `GameMasterAgent` for inspection/coordination
- Maintains same method signatures for backward compatibility

### 2. AgentChatClient Implementation
- Uses `ChatCompletionAgent` from Semantic Kernel agents framework
- Currently wraps the agent's kernel to maintain existing functionality
- Contains extension points for future multi-agent coordination

### 3. GameMasterAgentFactory
- Creates properly configured `ChatCompletionAgent` instances
- Combines adventure context, character information, and system prompts
- Centralizes agent configuration logic

## Extension Points for Multi-Agent Support

### 1. Agent Group Coordination (AgentChatClient.cs)
```csharp
// TODO: Future enhancement - Create AgentGroupChat for multi-agent scenarios:
// var agentGroup = new AgentGroupChat(GameMasterAgent, npcAgent1, npcAgent2);
// await foreach (var message in agentGroup.InvokeAsync(history))
```

**Implementation Strategy:**
- Create an `AgentGroupChat` that manages conversation turns between agents
- Define agent roles (GameMaster, NPC, WorldDescriptor, etc.)
- Implement turn-taking logic based on conversation context

### 2. Multiple Agent Types (ServiceExtensions.cs)
```csharp
// EXTENSION POINT: Future multi-agent support could register additional agents here
// For example:
// services.AddTransient<NPCAgent>(sp => CreateNPCAgent(sp, "Merchant"));
// services.AddTransient<WorldAgent>(sp => CreateWorldAgent(sp));
```

**Suggested Agent Types:**
- **NPCAgent**: Roleplay specific non-player characters
- **WorldAgent**: Provide environmental descriptions and world state
- **RulesAgent**: Handle rule enforcement and dice rolling
- **NarratorAgent**: Focus on storytelling and scene setting

### 3. Agent Configuration (GameMasterAgentFactory.cs)
```csharp
// EXTENSION POINT: Agent configuration could be enhanced for multi-agent scenarios
// For example, different agent types could have different configurations:
// - GameMasterAgent: Narrative control, rule enforcement
// - NPCAgent: Character roleplay for specific NPCs
// - WorldAgent: Environmental descriptions and world state
```

## Implementation Example: Adding NPC Agents

### Step 1: Create NPC Agent Factory
```csharp
public class NPCAgentFactory
{
    public ChatCompletionAgent CreateNPCAgent(Character npc, Adventure adventure, Kernel kernel)
    {
        return new ChatCompletionAgent
        {
            Name = $"NPC_{npc.Name}",
            Description = $"Roleplay agent for {npc.Name}",
            Instructions = $"You are {npc.Name}, a {npc.Specialization}. {npc.CharacterSheet}",
            Kernel = kernel
        };
    }
}
```

### Step 2: Register Multiple Agents
```csharp
services.AddTransient<NPCAgentFactory>();
services.AddTransient<IEnumerable<Agent>>(sp =>
{
    var adventure = sp.GetRequiredService<Adventure>();
    var kernel = sp.GetRequiredService<Kernel>();
    var npcFactory = sp.GetRequiredService<NPCAgentFactory>();
    var gmFactory = sp.GetRequiredService<GameMasterAgentFactory>();
    
    var agents = new List<Agent>
    {
        gmFactory.CreateGameMasterAgent(adventure, character, kernel)
    };
    
    // Add NPC agents for each character in the adventure
    agents.AddRange(adventure.Characters.Select(npc => 
        npcFactory.CreateNPCAgent(npc, adventure, kernel)));
    
    return agents;
});
```

### Step 3: Implement Agent Group Coordination
```csharp
public class MultiAgentChatClient : IAgentChatClient
{
    private readonly AgentGroupChat _agentGroup;
    
    public MultiAgentChatClient(IEnumerable<Agent> agents)
    {
        _agentGroup = new AgentGroupChat(agents.ToArray());
    }
    
    public async Task<ChatHistory> ChatAsync(ChatHistory history, CancellationToken cancellationToken)
    {
        await foreach (var message in _agentGroup.InvokeAsync(history, cancellationToken))
        {
            history.Add(message);
            // Display message with agent identification
        }
        return history;
    }
}
```

## Backward Compatibility

The legacy `IConsoleChatClient` interface and `ConsoleChatClient` implementation remain available for backward compatibility. They can be removed in a future version once the agent-based approach is fully validated.

## Testing Strategy

- Unit tests verify agent creation and configuration
- Integration tests ensure chat functionality works end-to-end
- Mock agents can be used to test multi-agent coordination logic
- Consider adding performance tests for multi-agent scenarios

## Performance Considerations

- Agent framework may introduce slight overhead compared to direct service calls
- Multi-agent scenarios will require careful management of API calls and token usage
- Consider implementing agent response caching for frequently used responses
- Monitor conversation context length as multiple agents contribute to history

## Security Considerations

- Each agent should have clearly defined capabilities and permissions
- System prompts should prevent agents from impersonating each other
- Consider implementing agent authentication/authorization for sensitive operations
- Monitor for potential prompt injection attacks across agent boundaries