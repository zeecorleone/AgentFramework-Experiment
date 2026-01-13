
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AF.Shared.Extensions;

public static class AgentThreadPersistenceFileBased
{
    private static string ConversationPath => Path.Combine(@"D:\garbage\agent-framework\agent-persistence", "conversation.json");

    public static async Task<AgentThread> ResumeChatIfRequestedAsync(AIAgent agent)
    {
        if (File.Exists(ConversationPath))
        {
            Console.Write("Restore previous conversation? (Y/N): ");
            ConsoleKeyInfo key = Console.ReadKey();
            Console.Clear();
            if (key.Key == ConsoleKey.Y)
            {
                var fileContent = await File.ReadAllTextAsync(ConversationPath);
                JsonElement jsonElement = JsonSerializer.Deserialize<JsonElement>(fileContent);
                AgentThread resumedThread = agent.DeserializeThread(jsonElement);

                await RestoreConsole(resumedThread);
                return resumedThread;
            }
        }

        return agent.GetNewThread();
    }

    private static async Task RestoreConsole(AgentThread resumedThread)
    {
        ChatClientAgentThread chatClientAgentThread = (ChatClientAgentThread)resumedThread;
        if (chatClientAgentThread.MessageStore != null)
        {
            IList<ChatMessage>? messages = resumedThread.GetService<IList<ChatMessage>>();
            foreach (ChatMessage message in messages!)
            {
                if (message.Role == ChatRole.User)
                {
                    Console.WriteLine($"User>>: {message.Text}");
                }
                else if (message.Role == ChatRole.Assistant)
                {
                    Console.WriteLine($"{message.Text}");
                    Console.WriteLine();
                    Console.WriteLine(string.Empty.PadLeft(50, '*'));
                    Console.WriteLine();
                }
            }
        }
    }

    public static async Task StoreThreadAsync(AgentThread thread)
    {
        JsonElement serializedThread = thread.Serialize();
        await File.WriteAllTextAsync(ConversationPath, JsonSerializer.Serialize(serializedThread));
    }
}
