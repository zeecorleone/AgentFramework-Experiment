/* Steps:
* 1: Create an 'Azure AI Foundry' Resource (or legacy 'Azure OpenAI Resource')
* 2: Add Nuget Packages (Azure.AI.OpenAI + Microsoft.Agents.AI.OpenAI)
* 3: Create an AzureOpenAIClient (API Key or Azure Identity)
* 4: Get a ChatClient and Create an AI Agent from it
* 5: Call RunAsync or RunStreamingAsync
*/
using AFExperiments;
using Microsoft.Agents.AI;
using OpenAI.Chat;

const string endpoint = Constants.Endpoint_AzureOpenAI;
const string apikey = Constants.ApiKey;
const string model = Constants.Model;


Azure.AI.OpenAI.AzureOpenAIClient client = new Azure.AI.OpenAI.AzureOpenAIClient(
    new Uri(endpoint), 
    new System.ClientModel.ApiKeyCredential(apikey)
    );

ChatClient chatClient = client.GetChatClient(model);
AIAgent agent = chatClient.CreateAIAgent();

//AgentRunResponse response = await agent.RunAsync("What is the capital of France?");
//Console.WriteLine(response);

//OR with streaming:

await foreach(AgentRunResponseUpdate update in agent.RunStreamingAsync("How to make tea?"))
{
    Console.Write(update);
}




