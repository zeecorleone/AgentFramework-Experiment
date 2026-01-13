// See https://aka.ms/new-console-template for more information
using AF.Shared.Tools;
using AFExperiments;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

Console.WriteLine("Hello, World!");


AzureOpenAIClient client = new AzureOpenAIClient(
    new Uri(Constants.Endpoint_AzureOpenAI),
    new System.ClientModel.ApiKeyCredential(Constants.ApiKey));

ChatClient chatClient = client.GetChatClient(Constants.Model);

//AIAgent agent = chatClient.CreateAIAgent("You are a Time expert agent, answering questions");

AIAgent agent = chatClient.CreateAIAgent(
    instructions: "You are a Time expert agent.",
    tools:
        [
            AIFunctionFactory.Create(Tools.CurrentDateAndTime, "current_date_time"),
            AIFunctionFactory.Create(Tools.CurrentTimeZone, "current_time_zone")
        ]
    );

AgentThread thread = agent.GetNewThread();


while(true)
{
    Console.Write("User>>: ");
    string userInput = Console.ReadLine();
    if (!string.IsNullOrEmpty(userInput))
    {
        Microsoft.Extensions.AI.ChatMessage message = new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, userInput);
        await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync(message, thread))
        {
            Console.Write(update);
        }
        Console.WriteLine("\n" + string.Empty.PadLeft(50, '*') + "\n");
    }

}