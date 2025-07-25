# Multi-Agent Architecture Implementation

This document details the multi-agent architecture implemented to improve game master responses through a structured pipeline approach.

## Overview

The multi-agent system replaces the single GameMaster agent with a three-agent pipeline that processes each player input through specialized agents:

**Pipeline**: Player Input → Planning Agent → Game Master Agent → Editor Agent → Final Response

## Agent Responsibilities

### 1. Planning Agent (`PlanningAgent`)
**Purpose**: Analyzes player input and creates structured plans for appropriate responses.

**Key Responsibilities**:
- Analyze player intent and validate actions within game rules
- Plan appropriate consequences and identify needed dice rolls
- Ensure player agency is respected throughout planning
- Create structured guidance for the Game Master agent

**Critical Rules**:
- NEVER roll dice for the player - only suggest when they should roll
- NEVER take actions on behalf of the player without explicit request
- NEVER make decisions for the player character
- Always respect player agency

**Output Format**: Structured plan with sections for analysis, validity, response type, suggested content, dice requirements, and narrative notes.

### 2. Game Master Agent (`GameMaster`)
**Purpose**: Converts structured plans into engaging narrative responses.

**Key Responsibilities**:
- Take planning output and create natural, immersive narratives
- Maintain appropriate game master tone and atmosphere
- Describe scenes, actions, and consequences vividly
- Ask for dice rolls when needed (never roll for the player)
- Provide clear information about what the player sees/hears/experiences

**Input**: Receives both recent chat history and the structured plan from the Planning Agent.

**Output**: Natural game master narrative response ready for final editing.

### 3. Editor Agent (`EditorAgent`)
**Purpose**: Improves and proofs responses to ensure tabletop RPG best practices.

**Key Responsibilities**:
- Review responses for common tabletop RPG mistakes
- Ensure player agency and game master best practices are maintained
- Fix unclear or confusing dice roll instructions
- Improve clarity and engagement without changing core meaning
- Remove inappropriate game master behavior

**Common Mistakes Fixed**:
- Game master rolling dice for the player
- Taking actions on behalf of the player without permission
- Making decisions for the player character
- Unclear instructions about what dice to roll
- Breaking the fourth wall inappropriately
- Inconsistent tone or style

## Implementation Details

### Agent Flow

```csharp
public async Task<ChatHistory> ChatAsync(ChatHistory history, CancellationToken cancellationToken = default)
{
    // Step 1: Planning Agent analyzes the situation and creates a plan
    string plan = await GetPlanningResponse(history, cancellationToken);
    _log.LogInformation("Planning Agent created plan: {Plan}", plan);
    
    // Step 2: Game Master Agent creates response based on the plan
    string gmResponse = await GetGameMasterResponse(history, plan, cancellationToken);
    _log.LogInformation("Game Master Agent created response: {Response}", gmResponse);
    
    // Step 3: Editor Agent improves and proofs the response
    string finalResponse = await GetEditorResponse(gmResponse, cancellationToken);
    _log.LogInformation("Editor Agent improved response: {FinalResponse}", finalResponse);
    
    // Add only the final edited response to the chat history
    var assistantMessage = new ChatMessageContent(AuthorRole.Assistant, finalResponse);
    history.Add(assistantMessage);
    
    return history;
}
```

### Chat History Management

**Critical Feature**: Only the final edited response is stored in the chat history. Intermediate steps (planning, draft responses) are:
- Logged for observability and debugging
- Used internally for agent coordination
- **Not persisted** in the chat history

This ensures that subsequent requests only see clean, final responses without cluttering the conversation with internal agent communications.

### Configuration

The system supports switching between multi-agent and single-agent modes:

```json
{
  "UseMultiAgentMode": true,  // Default: use multi-agent pipeline
  "Ollama": { /* ... */ }
}
```

When `UseMultiAgentMode` is `false`, the system uses the legacy single GameMaster agent for backward compatibility.

### Console Output

The system provides visual feedback during the multi-agent process:

```
[System] Generating response...
[System] Planning response...
[System] Creating narrative response...
[System] Editing and improving response...
GameMaster: [Final polished response]
```

## Benefits

### 1. **Prevents Common Mistakes**
- Multiple review stages catch dice rolling violations
- Editor specifically looks for and fixes player agency violations
- Planning stage validates actions within game rules

### 2. **Improved Response Quality**
- Specialized agents focus on their expertise areas
- Editor stage polishes clarity and engagement
- Structured planning ensures thoughtful responses

### 3. **Observability**
- Comprehensive logging at each pipeline stage
- Console output shows progress through the pipeline
- Easy to debug issues in specific agent stages

### 4. **Player Agency Protection**
- Multiple checkpoints ensure player decisions are respected
- Planning agent validates player intent
- Editor removes any inappropriate game master behavior

### 5. **Maintainability**
- Clear separation of concerns between agents
- Individual agents can be tested and improved independently
- Easy to add new agents or modify existing ones

## Testing

The multi-agent system includes comprehensive unit tests:

```csharp
[Fact]
public void AllAgents_ShouldPreventDiceRollingForPlayer()
{
    // Tests that all agents include rules preventing dice rolling for players
    planningAgent.Instructions.ShouldContain("NEVER roll dice for the player");
    gmAgent.Instructions.ShouldContain("NEVER roll dice for the player");
    editorAgent.Instructions.ShouldContain("Game master rolling dice for the player");
}
```

Tests cover:
- Agent creation and configuration
- Multi-agent coordination flow
- Prevention of common tabletop RPG mistakes
- Configuration switching between modes

## Performance Considerations

- **Latency**: Three sequential agent calls increase response time
- **Observability**: Comprehensive logging helps monitor performance
- **Efficiency**: Each agent receives only necessary context to minimize token usage
- **Fallback**: Single-agent mode available for performance-critical scenarios

## Future Enhancements

- **Parallel Processing**: Some agent stages could potentially run in parallel
- **Caching**: Plan and draft responses could be cached for similar scenarios
- **Adaptive Pipeline**: Skip agents when simple responses don't require full pipeline
- **Quality Metrics**: Track improvement metrics from editor agent changes