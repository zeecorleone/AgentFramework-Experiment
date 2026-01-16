
#pragma warning disable MEAI001

using AF.Shared;
using AF.Shared.Tools;
using AFExperiments;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System.Reflection;
using System.Text;


AzureOpenAIClient client = new AzureOpenAIClient(
    new Uri(Constants.Endpoint_AzureOpenAI),
    new System.ClientModel.ApiKeyCredential(Constants.ApiKey));

//Get tools via reflection
MethodInfo[] methods = typeof(FileSystemTools).GetMethods(BindingFlags.Public | BindingFlags.Instance);
List<AITool> tools = methods.Select(x => AIFunctionFactory.Create(x, new FileSystemTools())).Cast<AITool>().ToList();

//Approval tools
#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
tools.Add(new ApprovalRequiredAIFunction(AIFunctionFactory.Create(DangerousTools.SomethingDangerous)));
#pragma warning restore MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

ChatClient chatClient = client.GetChatClient(Constants.Model);
AIAgent agent = chatClient.CreateAIAgent(
    instructions: "You are a File System expert agent. When working with files you need to provide the full path, not just the filename",
    tools: tools
    )
    .AsBuilder()
    .Use(FunctionCallMiddleware)
    .Build();

AgentThread thread = agent.GetNewThread();

#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

while(true)
{
    Console.Write(">>: ");
    string? input = Console.ReadLine();
    Microsoft.Extensions.AI.ChatMessage message = new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, input ?? string.Empty);
    AgentRunResponse response = await agent.RunAsync(message, thread);
    List<UserInputRequestContent> userInputRequests = response.UserInputRequests.ToList();

    while(userInputRequests.Count > 0)
    {
        List<Microsoft.Extensions.AI.ChatMessage> userInputResponses = userInputRequests
            .OfType<FunctionApprovalRequestContent>()
            .Select(functionApprovalRequest =>
            {
                Utils.WriteLineRed($"The agent wants to invoke the following functio, please with Y to approve: Name: {functionApprovalRequest.FunctionCall.Name}");
                var userApprovalInputKey = Console.ReadKey();
                var approved = userApprovalInputKey.KeyChar.ToString().Equals("Y", StringComparison.OrdinalIgnoreCase);

                return new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, [functionApprovalRequest.CreateResponse(approved)]
                    );
            })
            .ToList();

        response = await agent.RunAsync(userInputResponses, thread);
        userInputRequests = response.UserInputRequests.ToList();
    }

    //Console.WriteLine(response);
    Utils.WriteLineMagenta(response.Text);
    Utils.Separator();

}

async ValueTask<object?> FunctionCallMiddleware(
    AIAgent callingAgent,
    FunctionInvocationContext context,
    Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, 
    CancellationToken cancellationToken)
{
    StringBuilder functionCallDetails = new();
    functionCallDetails.AppendLine($"- Tool Call: {context.Function.Name}");
    if(context.Arguments.Count > 0)
    {
        functionCallDetails.Append($" (Args: { string.Join(",", context.Arguments.Select(x => $"[{x.Key} = {x.Value}]")) }");
    }

    Utils.WriteLineDarkGray(functionCallDetails.ToString());

    return await next(context, cancellationToken);
}

#pragma warning restore MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.