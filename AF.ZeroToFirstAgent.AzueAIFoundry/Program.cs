/* Steps:
* 1: Create an 'Azure AI Foundry' Resource + Deploy Model
* 2: Add Nuget Packages (Azure.AI.Agents.Persistent, Azure.Identity, Microsoft.Agents.AI.AzureAI)
* 3: Create an PersistentAgentsClient (Azure Identity)
* 4: Use the client's Administration to create a new agent
* 5: Use client to get an AIAgent from the persistentAgent's Id
* 6: Create a new Thread
* 7: Call like normal
* 8: (Optional) Clean up
* 
* NEW FLOW Packages:
* install Azure.AI.Projects + Azure.AI.Projects.OpenAI + Azure.Identity + Microsoft.Agents.AI.AzureAI
* 
*/

#define NET8_0_OR_GREATER
#define PREVIEW_FEATURES

using AFExperiments;
using Azure;
using Azure.AI.Agents.Persistent;
using Azure.AI.Projects;
using Azure.Identity;
using Azure.AI.Projects.OpenAI;
using Microsoft.Agents.AI;

const string endpoint = Constants.Endpoint_AzureOpenAI;
const string model = Constants.Model;

//Azure.AI.Agents.Persistent.PersistentAgentsClient client = new Azure.AI.Agents.Persistent.PersistentAgentsClient(
//    endpoint,
//    new Azure.Identity.DefaultAzureCredential()
//    );

//Response<PersistentAgent> aiFoundryAgent = null;

//try
//{
//    aiFoundryAgent = await client.Administration.CreateAgentAsync(model, "MyFirstAgent", "My First Agent", "You are a nice AI");

//    AIAgent agent = client.GetAIAgentAsync(aiFoundryAgent.Value.Id);
//}
//finally
//{

//}

#pragma warning disable CA2252

//777baf61-e9a7-4096-9c6c-e58f95ab2fac

var credentialOptions = new DefaultAzureCredentialOptions
{
    TenantId = "777baf61-e9a7-4096-9c6c-e58f95ab2fac",
    // Explicitly allow only Entra-safe credentials
    ExcludeEnvironmentCredential = false,
    ExcludeManagedIdentityCredential = false,
    ExcludeAzureCliCredential = false,
    ExcludeVisualStudioCredential = false,

    // Explicitly block browser & personal account paths
    ExcludeInteractiveBrowserCredential = true,
    ExcludeSharedTokenCacheCredential = true // obsolete but intentional
};

var credentials = new DefaultAzureCredential(credentialOptions);
AIProjectClient projectClient = new AIProjectClient(
    new Uri(Constants.Endpoint_AzureAIFoundry),
    credentials
    );


// create an agent version (newer flow)
var createOptions = new AgentVersionCreationOptions(
    new PromptAgentDefinition(model)
    {
        Instructions = "You are a nice Agent",
        //Temperature = (float?)0.7,
        Model = model,
    });
createOptions.Description = "My First Agent (desc)";

var foundryAgent = await projectClient.Agents.CreateAgentVersionAsync("my-first-agent", createOptions);

//var foundryAgentId = foundryAgent.Value.Id;
Console.WriteLine($"Agent ID: {foundryAgent.Value.Id}");

var agent = projectClient.GetAIAgent(foundryAgent);

AgentRunResponse response = await agent.RunAsync("What is the capital of France?");
Console.WriteLine(response);

//OR with streaming:
await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("How to make tea?"))
{
    Console.Write(update);
}

//delete agent
await projectClient.Agents.DeleteAgentAsync("my-first-agent");


#pragma warning restore CA2252