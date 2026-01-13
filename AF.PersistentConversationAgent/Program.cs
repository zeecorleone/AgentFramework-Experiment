

using AF.Shared.Extensions;
using AFExperiments;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

AzureOpenAIClient client = new AzureOpenAIClient(
    new Uri(Constants.Endpoint_AzureOpenAI),
    new System.ClientModel.ApiKeyCredential(Constants.ApiKey));

var chatClient = client.GetChatClient(Constants.Model);
AIAgent agent = chatClient.CreateAIAgent("You are a friendly AI Bot, answering my questions.");

AgentThread thread = agent.GetNewThread();

var resumeOldChat = true;

if(resumeOldChat)
{
    thread = await AgentThreadPersistenceFileBased.ResumeChatIfRequestedAsync(agent);
}

while(true)
{
    Console.Write("User>>: ");
    string userInput = Console.ReadLine();

    if(!string.IsNullOrEmpty(userInput))
    {
        Microsoft.Extensions.AI.ChatMessage message = new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, userInput);
        await foreach(AgentRunResponseUpdate update in agent.RunStreamingAsync(message, thread))
        {
            Console.Write(update);
        }
        
        Console.WriteLine("\n" + string.Empty.PadLeft(50, '*') + "\n");
        await AgentThreadPersistenceFileBased.StoreThreadAsync(thread);

    }
    
}