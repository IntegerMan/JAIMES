# GitHub Copilot Instructions for JAIMES

## Project Overview

**JAIMES** (Join AI to Make Epic Stories) is an AI-powered tabletop game master designed to run tabletop roleplaying games for solo players. This is a proof-of-concept project that demonstrates the integration of Large Language Models (LLMs), AI orchestration tools, and Retrieval-Augmented Generation (RAG) technologies.

**Author:** Matt Eland  
**Purpose:** Demonstration and teaching project for AI-assisted tabletop gaming  
**Target Framework:** .NET 9.0  

## Architecture & Project Structure

The solution follows a clean architecture pattern with clear separation of concerns:

```
AiTableTopGameMaster.sln
├── AiTableTopGameMaster.ConsoleApp/     # Main console application and UI
├── AiTableTopGameMaster.Core/           # Core AI functionality and plugins
├── AiTableTopGameMaster.Domain/         # Domain models and entities
├── AiTableTopGameMaster.Tests/          # Unit tests
└── adventures/                          # JSON-based adventure content
```

### Key Components

- **Console App**: Entry point with rich console UI using Spectre.Console
- **Core**: AI orchestration using Microsoft Semantic Kernel with plugin architecture
- **Domain**: Immutable domain models (Adventure, Character, Location, Encounter)
- **Systems**: Game system-specific implementations (currently D&D 5E)
- **Tests**: Comprehensive unit tests using xUnit and Shouldly

## Technologies & Frameworks

### Core Technologies
- **.NET 9.0** - Latest .NET framework
- **C# 13** - Modern C# language features
- **Microsoft Semantic Kernel** - AI orchestration and plugin system
- **Microsoft Kernel Memory** - RAG implementation for knowledge retrieval
- **Ollama** - Local LLM hosting and inference

### Supporting Libraries
- **Spectre.Console** - Rich console UI and formatting
- **Serilog** - Structured logging framework
- **Microsoft.Extensions.*** - Dependency injection, configuration, hosting
- **xUnit** - Unit testing framework
- **Shouldly** - Fluent assertion library
- **JetBrains.Annotations** - Code analysis

### Data & Configuration
- **JSON** - Adventure definitions, character sheets, and configuration
- **appsettings.json** - Application configuration
- **File-based storage** - Adventures and game data
- **Ollama models** - Local LLMs for AI responses

## Coding Conventions & Best Practices

### C# Language Features
- **Nullable reference types** enabled throughout the solution
- **File-scoped namespaces** for cleaner code organization
- **Global usings** enabled to reduce repetitive using statements
- **Required properties** for ensuring object initialization
- **Init-only properties** for immutable object creation
- **Records and classes** with appropriate mutability patterns
- **Pattern matching** for concise type checks and casting
- **Async/await** for asynchronous programming, but only where necessary
- **Target-typed new expressions** for cleaner object instantiation
- **Expression-bodied members** for concise method definitions
- **Avoid var** except for in foreach loops

### Code Style
```csharp
// Preferred namespace declaration
namespace AiTableTopGameMaster.Domain;

// Domain models with required properties
public class Adventure
{
    public required string Name { get; init; }
    public required string Author { get; init; }
    public List<Location> Locations { get; init; } = [];
}

// Service registration extensions
public static class ServiceExtensions
{
    public static IKernelBuilder AddAdventurePlugins(this IKernelBuilder builder, Adventure adventure)
    {
        StoryInfoPlugin plugin = new(adventure);
        builder.Plugins.AddFromObject(plugin);
        
        return builder;
    }
}
```

### Dependency Injection Patterns
- Use `Microsoft.Extensions.DependencyInjection` for service registration
- Register services in extension methods for clean organization
- Prefer constructor injection over service locator pattern
- Use `IServiceProvider` for service resolution in Program.cs

### Logging Practices
- Use **Serilog** for structured logging throughout the application
- Log at appropriate levels (Debug, Information, Warning, Error)
- Include structured data in log messages
- Separate diagnostic logging from console output

```csharp
Log.Debug("Adventure loaded: {Name} by {Author}", adventure.Name, adventure.Author);
```

## AI & Semantic Kernel Patterns

### Plugin Architecture
- Implement plugins as classes with methods decorated with Semantic Kernel attributes
- Group related functionality into cohesive plugins (StoryInfoPlugin, LocationsPlugin, etc.)
- Use dependency injection to provide required services to plugins

### AI Core System
- Design AI functionality around "cores" that manage specific aspects
- Each core can customize which plugins and models it uses
- Implement history tracking and context management
- Handle tool call errors gracefully with retry mechanisms

### RAG Implementation
- Use Kernel Memory for document indexing and retrieval
- Index external content (D&D SRD, game rules) for knowledge augmentation
- Provide context-aware responses using retrieved knowledge

## Testing Strategy

### Unit Testing Approach
- Use **xUnit** as the primary testing framework
- Use **Shouldly** for fluent, readable assertions
- Test business logic in isolation from external dependencies
- Mock external services using interfaces

### Test Structure
```csharp
public class AdventureLoaderTests
{
    [Theory]
    [InlineData("adventure1.json", "Expected Name")]
    [InlineData("adventure2.json", "Another Name")]
    public async Task LoadAdventure_LoadsAdventureCorrectly(string path, string expectedName)
    {
        // Arrange
        AdventureLoader loader = new();
        
        // Act
        Adventure? adventure = await loader.LoadAsync(path);
        
        // Assert
        adventure.ShouldNotBeNull();
        adventure.Name.ShouldBe(expectedName);
    }
}
```

### Testing Guidelines
- Test happy path scenarios and error conditions
- Use temporary files for file system testing
- Clean up resources in finally blocks or using statements
- Test async operations properly with async/await

## Development Guidelines

### Error Handling
- Use structured exception handling with appropriate exception types
- Provide meaningful error messages to users
- Log errors with sufficient context for debugging
- Handle AI model failures gracefully

### Performance Considerations
- Use async/await patterns for I/O operations
- Consider memory usage when processing large documents
- Implement proper disposal patterns for resources
- Cache frequently accessed data appropriately

### JSON and Data Handling
- Use System.Text.Json for serialization/deserialization
- Implement proper validation for JSON schema
- Handle missing or malformed data gracefully
- Use strongly-typed models instead of dynamic objects

### Console UI Best Practices
- Use Spectre.Console for rich formatting and user interaction
- Separate display logic from business logic
- Provide clear user feedback for long-running operations
- Handle user input validation appropriately

## Common Patterns

### Service Registration
```csharp
services.AddSingleton<IAdventureLoader, AdventureLoader>();
services.Configure<OllamaSettings>(configuration.GetSection("Ollama"));
```

### Plugin Implementation
```csharp
public class StoryInfoPlugin
{
    [KernelFunction, Description("Gets story information")]
    public string GetStoryInfo() => adventure.Backstory;
}
```

### Immutable Domain Models
```csharp
public class Character
{
    public required string Name { get; init; }
    public required string Specialization { get; init; }
    public List<string> Skills { get; init; } = [];
}
```

### Async File Operations
```csharp
public async Task<Adventure> LoadAdventureAsync(string path)
{
    string json = await File.ReadAllTextAsync(path);
    return JsonSerializer.Deserialize<Adventure>(json) ?? throw new JsonException();
}
```

## When Contributing Code

1. **Follow established patterns** - Look at existing code for guidance on structure and style
2. **Use nullable reference types** - All new code should properly handle null values
3. **Add appropriate logging** - Include structured logging for important operations
4. **Write tests** - Include unit tests for new functionality
5. **Handle errors gracefully** - Provide meaningful error messages and recovery paths
6. **Document complex logic** - Add comments for AI orchestration and complex business rules
7. **Use dependency injection** - Register and inject services properly
8. **Follow async patterns** - Use async/await for I/O operations consistently

## Additional Notes

- The project uses cutting-edge .NET 9.0 features and may require the latest SDK
- AI model responses can be unpredictable; implement robust error handling
- JSON adventure files follow a specific schema - validate input appropriately
- The project is designed for learning and demonstration; prioritize clarity over optimization
- Consider the educational aspect when making changes - code should be readable and well-documented