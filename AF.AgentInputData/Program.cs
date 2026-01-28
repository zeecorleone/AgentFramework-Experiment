


using AF.Shared.Extensions;
using AFExperiments;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;

//Azure OpneAI Client
AzureOpenAIClient azureOpenAiClient = new AzureOpenAIClient(
    new Uri(Constants.Endpoint_AzureOpenAI),
    new System.ClientModel.ApiKeyCredential(Constants.ApiKey));

//OpenAI Client
OpenAIClient openAiClient = new OpenAIClient(new Azure.AzureKeyCredential(Constants.ApiKey));


AIAgent azureOpenAiAgent = azureOpenAiClient.GetChatClient(Constants.Model).CreateAIAgent();
AIAgent openAiAgent = openAiClient.GetChatClient(Constants.Model).CreateAIAgent();


Scenario scenario = Scenario.Pdf;

AgentRunResponse response;

switch(scenario)
{
    case Scenario.Text:
        {
            response = await azureOpenAiAgent.RunAsync("What is the capital of Germany?");
            ShowResponse(response);
        }
        break;

    case Scenario.Image:
        {
            //------Image via URI
            var chatMsg = new Microsoft.Extensions.AI.ChatMessage(ChatRole.User,
                [
                    new TextContent("What is in this image?"),
                    new UriContent("https://upload.wikimedia.org/wikipedia/commons/thumb/d/d1/Kit_Kat_Matcha-9136.jpg/250px-Kit_Kat_Matcha-9136.jpg", "image/jpeg")
                ]);
            response = await azureOpenAiAgent.RunAsync(chatMsg);
            ShowResponse(response);

            //------Local file

            string path = @"D:\garbage\agent-framework\images\test.png";

            string base64Image = Convert.ToBase64String(System.IO.File.ReadAllBytes(path));
            string dataUri = $"data:image/png;base64,{base64Image}";
            chatMsg = new Microsoft.Extensions.AI.ChatMessage(ChatRole.User,
                [
                    new TextContent("What is in this image?"),
                    new UriContent(dataUri, "image/png")
                ]);
            response = await azureOpenAiAgent.RunAsync(chatMsg);

            ShowResponse(response);

            //------Image via Memory
            ReadOnlyMemory<byte> imageBytes = System.IO.File.ReadAllBytes(path).AsMemory();
            chatMsg = new Microsoft.Extensions.AI.ChatMessage(ChatRole.User,
                [
                    new TextContent("What is in this image?"),
                    new DataContent(imageBytes, "image/png")
                ]);

            response = await azureOpenAiAgent.RunAsync(chatMsg);
            ShowResponse(response);
        }
        break;
    case Scenario.Pdf:
        {
            //Notes
            //- This Scenario works only with OpenAI, Not AzureOpenAI.
            //- PDFs can't be read via URI, only via Memory or local file.

            //---------- PDF as Base64
            string path = @"D:\garbage\agent-framework\images\sample-prices.pdf";
            string base64Pdf = Convert.ToBase64String(System.IO.File.ReadAllBytes(path));
            var dataUri = $"data:application/pdf;base64,{base64Pdf}";

            var chatMsg = new Microsoft.Extensions.AI.ChatMessage(ChatRole.User,
                [
                    new TextContent("Summarize the content of this PDF document."),
                    new UriContent(dataUri, "application/pdf")
                ]);
            response = await openAiAgent.RunAsync(chatMsg);
            ShowResponse(response);

            //----------- PDF via Memory
            ReadOnlyMemory<byte> pdfBytes = System.IO.File.ReadAllBytes(path).AsMemory();
            chatMsg = new Microsoft.Extensions.AI.ChatMessage(ChatRole.User,
                [
                    new TextContent("Summarize the content of this PDF document."),
                    new DataContent(pdfBytes, "application/pdf")
                ]);
            response = await openAiAgent.RunAsync(chatMsg);
            ShowResponse(response);
        }
        break;
}




void ShowResponse(AgentRunResponse agentRunResponse)
{
    Console.WriteLine(agentRunResponse);
    agentRunResponse.Usage.OutputAsInformation();
}


enum Scenario
{
    Text,
    Pdf,
    Image,
}