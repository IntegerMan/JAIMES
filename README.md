# AI Tabletop Game Master

This is a project to create an AI-powered tabletop game master that can run table top roleplaying games for solo players. It's a proof of concept using LLMs, AI Orchestration tools like Semantic Kernel, and RAG with a vector database and Kernel Memory.

## Recent Major Update: Agents Framework Migration

The application has been **successfully migrated** from direct `IChatCompletionService` usage to **Semantic Kernel's agents framework**. This provides:

- ✅ **Better conversation structure and management** via ChatCompletionAgent
- ✅ **Clear extension points for multi-agent coordination** 
- ✅ **Improved separation of concerns** between game mechanics and AI services
- ✅ **Future support for specialized agents** (NPCs, world description, rules enforcement)

See [AGENTS_MIGRATION.md](AGENTS_MIGRATION.md) for detailed migration information and multi-agent extension points.

## Features

- **Agent-Based Game Master**: Uses Semantic Kernel's agents framework for intelligent conversation management
- **Adventure Support**: JSON-based adventure definitions with locations, encounters, and NPCs
- **Character Integration**: Character sheets and progression tracking  
- **Plugin System**: Extensible plugins for sourcebook lookup, character management, and encounters
- **Multi-Agent Ready**: Designed with extension points for future multi-agent coordination

This project was created by Matt Eland for demonstration and teaching purposes. It is not intended to be a complete game system, but rather a starting point for building AI-assisted tabletop games.

## Tasks

The following tasks are planned for the project:

### Semantic Kernel

- [x] Add Semantic Kernel core
- [x] Show tool calls as they occur via the console
- [x] Log Semantic Kernel data to the diagnostic log
- [x] Add a story info plugin to Semantic Kernel
- [x] Add a location and location list plugin to Semantic Kernel
- [x] Add a player character plugin to the Semantic Kernel
- [x] Add encounter and encounter list plugin to Semantic Kernel

### Kernel Memory and RAG

- [x] Add a RAG skill to the Semantic Kernel using Kernel Memory
- [x] Have Kernel Memory index the free D&D ruleset
- [x] Display more indexing progress in the console
- [ ] Host a free vector database locally for Kernel Memory instance

### Agentic Mode

- [x] **Design an agentic layout for the game's AI** ✅ COMPLETED
- [x] **Migrate to Semantic Kernel's agents framework** ✅ COMPLETED
- [x] **Create GameMaster agent with comprehensive prompts** ✅ COMPLETED
- [x] **Add extension points for multi-agent coordination** ✅ COMPLETED
- [ ] Implement multi-agent scenarios (NPCs, World agents, etc.)
- [ ] Add agent-to-agent communication protocols

### Quality and Testing

- [x] Add unit test project
- [ ] Generate sample transcripts for testing
- [ ] Add in Unit Tests around Semantic Kernel
- [ ] Add in Evaluation for common game scenarios
- [ ] Add A/B Testing for Prompt Refinement and Model Selection
- [ ] Add content safety checks (optional)

### Content

- [x] Load games and characters from JSON files
- [ ] Add a second character to the beach game
- [ ] Add a second adventure
- [ ] Optionally, separate characters from adventures

### User Interface

- [x] Add a console UI for the game
- [ ] Add configuration options for disabling displaying certain events
- [ ] Add an Uno Platform UI for the game

### Game Session Management

- [ ] Manage games by having their data live in the same directory
- [ ] Serialize games to JSON files
- [ ] Add a Resume Game command that loads a game state
- [x] Allow selecting a command on startup
