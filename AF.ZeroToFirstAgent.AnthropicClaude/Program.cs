

using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

const string apikey = "<yourApikey>";
const string model = AnthropicModels.Claude35Haiku;

IChatClient client = new AnthropicClient(new APIAuthentication(apikey)).Messages.AsBuilder().Build();

ChatClientAgentRunOptions chatClientAgentRunOptions = new ChatClientAgentRunOptions(new()
{
    ModelId = model,
    MaxOutputTokens = 1000
});

AIAgent agent = new ChatClientAgent(client);
AgentRunResponse response = await agent.RunAsync("What is the Capital of Australia?", options: chatClientAgentRunOptions);
Console.WriteLine(response);

Console.WriteLine(" --- ");

await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("How to make soup?", options: chatClientAgentRunOptions))
{

    Console.Write(update);

}