using AFExperiments;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI.Chat;

namespace AF.BlazorServerDI.Components.Pages;

public partial class Home(
        AzureOpenAIClient _azureOpenAIClient, 
        [FromKeyedServices("gpt-5-mini")] ChatClient _chatClinet,
        [FromKeyedServices("gpt-5-mini")] ChatClientAgent _agent
    )
{
    private string? _question;
    private string? _answer;

    public async Task AskAi()
    {
        if (string.IsNullOrWhiteSpace(_question))
            return;

        ChatClientAgent newAgent = _azureOpenAIClient
            .GetChatClient(Constants.Model)
            .CreateAIAgent();

        //Alternate approach:
        //agent = _chatClinet.CreateAIAgent();
        //OR
        //agent = _agent; // DI approach
        //But the limitation with above approaches is that
        //I will be limited to use the model provided by DI container, Unlike
        //the first approach where I can choose any model
        //at runtime when calling _azureOpenAIClient.GetChatClient,
        //..also, we may need to GetAudioClient similarly in future, or any other type of client etc

        AgentRunResponse response = await newAgent.RunAsync(_question);
        _answer = response.Text;
        StateHasChanged();

    }
}
