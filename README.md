# AI Tabletop Game Master

This is a project to create an AI-powered tabletop game master that can run games like Dungeons & Dragons for solo players. It's a proof of concept using LLMs, AI Orchestration tools like Semantic Kernel, and RAG with a vector database and Kernel Memory.

This project was created by Matt Eland for demonstration and teaching purposes. It is not intended to be a complete game system, but rather a starting point for building AI-assisted tabletop games.

Dungeons & Dragons is a trademark of Wizards of the Coast, and this project is not affiliated with or endorsed by Wizards of the Coast.

Wizards of the Coast has released a free version of the Dungeons & Dragons ruleset, which is used in this project. The ruleset can be found at [DNDBeyond.com](https://www.dndbeyond.com/sources/dnd/br-2024?&icid_medium=organic&icid_source=editorial&icid_campaign=dnd_free_rules_2024&icid_content=article_1804).

## Tasks

The following tasks are planned for the project:

### Semantic Kernel

- [x] Add Semantic Kernel core
- [x] Show tool calls as they occur via the console
- [x] Log Semantic Kernel data to the diagnostic log
- [ ] Add a story info plugin to Semantic Kernel
- [x] Add a location and location list plugin to Semantic Kernel
- [ ] Add a player character plugin to the Semantic Kernel
- [ ] Add encounter and encounter list plugin to Semantic Kernel

### Kernel Memory and RAG

- [ ] Add a RAG skill to the Semantic Kernel using Kernel Memory
- [ ] Have Kernel Memory index the free D&D ruleset
- [ ] Host a free vector database locally for Kernel Memory instance

### Agentic Mode

- [ ] Design an agentic layout for the game's AI

### Quality and Testing

- [ ] Add in Evaluation
- [ ] Add in Unit Tests
- [ ] Add A/B Testing for Prompt Refinement and Model Selection
- [ ] Add safety checks (optional)

### Game Session Management

- [ ] Manage games by having their data live in the same directory
- [ ] Serialize games to JSON files
- [ ] Add a Resume Game command that loads a game state
- [x] Allow selecting a command on startup
