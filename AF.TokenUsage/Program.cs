

using AF.Shared;
using AF.Shared.Extensions;
using AFExperiments;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI.Chat;

AzureOpenAIClient client = new AzureOpenAIClient(
    new Uri(Constants.Endpoint_AzureOpenAI),
    new System.ClientModel.ApiKeyCredential(Constants.ApiKey));

ChatClient chatClient = client.GetChatClient(Constants.Model);
AIAgent agent = chatClient.CreateAIAgent(instructions: "You are a friendly AI bot, answering questions about healthy food");

string question = "What category fruit is 'Orange' and how healthy is it?";

AgentRunResponse response = await agent.RunAsync(question);
Utils.WriteLineGreen(response.Text);

Utils.Separator();

//With Streaming:

List<AgentRunResponseUpdate> updates = [];
await foreach(AgentRunResponseUpdate update in agent.RunStreamingAsync(question))
{
    Console.Write(update);
    updates.Add(update);
}

AgentRunResponse collectedResponseFromStreaming = updates.ToAgentRunResponse();
collectedResponseFromStreaming.Usage.OutputAsInformation(); //"OutputAsInformation" is custom extension method that prints the token usage cont and reasoning count

Utils.Separator();
