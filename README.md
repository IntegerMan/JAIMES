# AI Tabletop Game Master

This is a project to create an AI-powered tabletop game master that can run table top roleplaying games for solo players. It's a proof of concept using LLMs, AI Orchestration tools like Semantic Kernel, and RAG with a vector database and Kernel Memory.

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

- [ ] Design an agentic layout for the game's AI

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
