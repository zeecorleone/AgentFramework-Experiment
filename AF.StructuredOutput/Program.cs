

using AF.Shared;
using AF.Shared.Models;
using AFExperiments;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;


string question = "What are the top 10 movies according to IMDB?";


AzureOpenAIClient client = new AzureOpenAIClient(
    new Uri(Constants.Endpoint_AzureOpenAI),
    new System.ClientModel.ApiKeyCredential(Constants.ApiKey));

//Without Structured Ouput

//ChatClient chatClient = client.GetChatClient(Constants.Model);
//AIAgent agent = chatClient.CreateAIAgent("You are an expert in IMDB lists.");

//AgentRunResponse response1 = await agent.RunAsync(question);
//Console.WriteLine(response1);

////////////////////////////////////////////////

//With Structured Output

ChatClient chatClient = client.GetChatClient(Constants.Model);
ChatClientAgent agent2 = chatClient.CreateAIAgent("You are an expert in IMDB lists."); //<-- Notice, this is not AIAgent, it is ChatClientAgent which has base class as AIAgent

AgentRunResponse<MovieResult> response2 = await agent2.RunAsync<MovieResult>(question);

MovieResult result2 = response2.Result;

DisplayMovies(result2);


Utils.Separator();

/////////////////////////////////////////////


//With structured output, but this time using with AIAgent, instead ot ChatClientAgent which expects target <Model> T. 
//this is for more cumbersome scenarios where we do have AIAgent available, instead of ChatClientAgent.


//we may not need following options all the time, but it is needed when "ENUM" is envolved, so we get string values of enums instead of numeric.
JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
{
    PropertyNameCaseInsensitive = true,
    TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    Converters = { new JsonStringEnumConverter() }
};


AIAgent agent3 = chatClient.CreateAIAgent("You are an expert in IMDB Lists");

//now setup response json schema settings
ChatResponseFormatJson chatResponseFormatJson = Microsoft.Extensions.AI.ChatResponseFormat.ForJsonSchema<MovieResult>(jsonSerializerOptions);

AgentRunResponse response3 = await agent3.RunAsync(question, options: new ChatClientAgentRunOptions()
{
    ChatOptions = new ChatOptions()
    {
        ResponseFormat = chatResponseFormatJson,
    }
});

MovieResult result3 = response3.Deserialize<MovieResult>(jsonSerializerOptions);

DisplayMovies(result3);



static void DisplayMovies(MovieResult movieResult)
{
    int counter = 1;
    Console.WriteLine(movieResult.MessageBack);
    foreach (Movie movie in movieResult.Top10Movies)
    {
        Console.WriteLine($"{counter}: {movie.Title} ({movie.YearOfRelease}) - Genre: {movie.Genre} - Director: {movie.Director} - IMDB Score: {movie.ImdbScore}");
        counter++;
    }
}
