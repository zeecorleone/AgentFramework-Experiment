using AF.BlazorServerDI.Components;
using AFExperiments;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI.Chat;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

AzureOpenAIClient client = new AzureOpenAIClient(
    new Uri(Constants.Endpoint_AzureOpenAI),
    new System.ClientModel.ApiKeyCredential(Constants.ApiKey));

builder.Services.AddSingleton(client);

ChatClient chatClient = client.GetChatClient(Constants.Model);
builder.Services.AddKeyedSingleton("gpt-5-mini", chatClient);

ChatClientAgent agent = chatClient.CreateAIAgent();
builder.Services.AddKeyedSingleton("gpt-5-mini", agent);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
