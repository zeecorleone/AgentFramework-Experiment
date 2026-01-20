
using AF.Shared;
using AF.Shared.Extensions;
using AFExperiments;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

//WHY? 
//WHy we need to consider reasoning settings? this is because models like GPT-5 have
//set defult reasoning level to "medium", which is too expensive for simple questions. and 
//the response also takes a lot of time.
//hence we should be able to adjust the reasoning level based on the complexity of the question asked.

AzureOpenAIClient client = new AzureOpenAIClient(
    new Uri(Constants.Endpoint_AzureOpenAI),
    new System.ClientModel.ApiKeyCredential(Constants.ApiKey), new AzureOpenAIClientOptions
    {
        NetworkTimeout = TimeSpan.FromMinutes(5) //You might need to adjust timeout when using long thinking agents
    });

//Defult model reasoning is set to "medium".
ChatClient chatClient = client.GetChatClient(Constants.Model);
ChatClientAgent agent = chatClient.CreateAIAgent();

AgentRunResponse response1 = await agent.RunAsync("What is the capital of France and how many people live there?");

Console.WriteLine(response1);
response1.Usage.OutputAsInformation();

Utils.Separator();

/////////////////////////////////////////////

//Let's control the reasoning effort for faster answer and lower cost.

ChatClientAgent agent2 = chatClient.CreateAIAgent(
        options: new ChatClientAgentOptions
        {
            ChatOptions = new ChatOptions
            {
                RawRepresentationFactory = _ => new ChatCompletionOptions
                {
#pragma warning disable OPENAI001
                    ReasoningEffortLevel = "minimal", //'minimal', 'low', 'medium' (default) or 'high'
#pragma warning restore OPENAI001
                },
            }
        });

AgentRunResponse response2 = await agent2.RunAsync("What is the Capital of France and how many people live there?");
Console.WriteLine(response2);
response2.Usage.OutputAsInformation();


Utils.Separator();

//Simpler version of above using my own extension method
ChatClientAgent agentControllingReasoningEffortSimplified = chatClient
    .CreateAIAgentForAzureOpenAi(reasoningEffort: "minimal");

AgentRunResponse response3 = await agentControllingReasoningEffortSimplified.RunAsync("What is the Capital of France and how many people live there?");
Console.WriteLine(response3);
response3.Usage.OutputAsInformation();