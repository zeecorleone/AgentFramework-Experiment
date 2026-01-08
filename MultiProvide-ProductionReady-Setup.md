# Production-Ready DI Setup for OpenAI and AzureOpenAI

This guide shows how to register AI clients and a vector store so your application can switch between **OpenAI** and **AzureOpenAI** at runtime. The samples assume you already have configuration values for both providers.

## Prerequisites
- .NET 8 or later (project targets .NET 10 preview here)
- NuGet packages:
  - `Azure.AI.OpenAI`
  - `OpenAI` (the OpenAI SDK)
  - `Microsoft.Extensions.AI` and `Microsoft.Agents.AI`
  - `Microsoft.SemanticKernel.Connectors.SqlServer` (or equivalent for `SqlServerVectorStore`)
- Configuration keys available in your chosen provider (Key Vault, app settings, etc.):
  - `SecretKeys.HubOpenAiKey` (OpenAI key)
  - `SecretKeys.AzureOpenAiEndpointSweden`
  - `SecretKeys.AzureOpenAiKeySweden`
  - A SQL Server connection string

> Replace the `SecretKeys` lookups with whatever accessor you use (e.g., `builder.Configuration["..."]`) as long as you enforce required values.

## DI Extension to Register AI and Vector Store
Add an extension on `WebApplicationBuilder` so all registrations live in one place.

```csharp
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.SqlServer; // adjust to your actual namespace for SqlServerVectorStore
using OpenAI;

public static partial class WebApplicationBuilderExtensions
{
    public static void AddAiAndVectorStore(this WebApplicationBuilder builder)
    {
        string openAiKey = builder.Configuration.GetRequiredValue(SecretKeys.HubOpenAiKey);
        string azureOpenAiEndpointSweden = builder.Configuration.GetRequiredValue(SecretKeys.AzureOpenAiEndpointSweden);
        string azureOpenAiKeySweden = builder.Configuration.GetRequiredValue(SecretKeys.AzureOpenAiKeySweden);

        string connectionString = builder.Configuration.GetDatabaseConnectionString();

        builder.Services.AddSingleton(new SqlVectorStoreConfiguration(connectionString));
        builder.Services.AddSingleton(new AzureOpenAIClient(new Uri(azureOpenAiEndpointSweden), new System.ClientModel.ApiKeyCredential(azureOpenAiKeySweden)));
        builder.Services.AddSingleton(new OpenAIClient(openAiKey));

        AzureOpenAIClient azureOpenAiClient = new(new Uri(azureOpenAiEndpointSweden), new System.ClientModel.ApiKeyCredential(azureOpenAiKeySweden));
        builder.Services.AddEmbeddingGenerator(azureOpenAiClient.GetEmbeddingClient(AiIds.EmbeddingModels.Ada).AsIEmbeddingGenerator());

        builder.Services.AddScoped<VectorStore, SqlServerVectorStore>(options => new SqlServerVectorStore(connectionString, new SqlServerVectorStoreOptions
        {
            EmbeddingGenerator = options.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>()
        }));
    }
}
```

> Keep the `ApiKeyCredential` type consistent with the SDK version you reference (`System.ClientModel.ApiKeyCredential` is used above). Replace `AiIds.EmbeddingModels.Ada` with your preferred embedding model identifier.

## Register the Extension in `Program.cs`
Call the extension early in host building and register your agent factory.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddAiAndVectorStore();
builder.Services.AddScoped<AiAgentQuery>();

var app = builder.Build();
// ... the rest of your pipeline
app.Run();
```

## Agent Factory That Chooses Provider at Runtime
Use a scoped service that receives both clients and returns an agent based on the requested provider.

```csharp
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;

public class AiAgentQuery(AzureOpenAIClient azureOpenAiClient, OpenAIClient openAiClient) : IScopedService
{
    public ChatClientAgent GetAgent(AiModel modelToUse, string instructions, List<AITool>? tools = null)
    {
        ChatOptions options = new();

        if (!string.IsNullOrWhiteSpace(modelToUse.ReasoningEffort))
        {
            options.RawRepresentationFactory = _ => new ChatCompletionOptions
            {
#pragma warning disable OPENAI001
                ReasoningEffortLevel = modelToUse.ReasoningEffort,
#pragma warning restore OPENAI001
            };
        }

        options.Instructions = instructions;

        if (tools?.Count > 0)
        {
            options.Tools = tools;
            options.AllowMultipleToolCalls = true;
            options.ToolMode = ChatToolMode.Auto;
        }

        return modelToUse.Provider switch
        {
            AiProvider.AzureOpenAi => azureOpenAiClient
                .GetChatClient(modelToUse.ModelName)
                .CreateAIAgent(new ChatClientAgentOptions { ChatOptions = options }),

            AiProvider.OpenAi => openAiClient
                .GetChatClient(modelToUse.ModelName)
                .CreateAIAgent(new ChatClientAgentOptions { ChatOptions = options }),

            _ => throw new ArgumentOutOfRangeException(nameof(modelToUse.Provider), modelToUse.Provider, null)
        };
    }
}
```

## How to Use in Your Application
- Inject `AiAgentQuery` into your controller/page/service.
- Pass the desired `AiModel` (containing `Provider`, `ModelName`, and optional `ReasoningEffort`).
- Provide optional `tools` when you want tool calling enabled.

```csharp
public class ChatService(AiAgentQuery agentQuery)
{
    public async Task<string> AskAsync(AiModel model, string prompt)
    {
        ChatClientAgent agent = agentQuery.GetAgent(model, "You are a helpful assistant.");
        AgentRunResponse response = await agent.RunAsync(prompt);
        return response.Text;
    }
}
```

## Production Notes
- Store keys in a secure secret provider (Key Vault or other) and use `GetRequiredValue` to fail fast when missing.
- Prefer scoped lifetimes for agents; keep clients singletons.
- Validate model names and provider mappings via configuration to prevent runtime errors.
- Monitor usage and errors per provider to fine-tune routing between OpenAI and Azure OpenAI.
