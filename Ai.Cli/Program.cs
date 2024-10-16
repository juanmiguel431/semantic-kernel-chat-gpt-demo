// See https://aka.ms/new-console-template for more information

using Ai.Cli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings()
{
    Args = args
});

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// var builder = Host.CreateApplicationBuilder(args); // The above two lines can be simplified with this
    

var azureOpenAi = builder.Configuration.GetSection("AzureOpenAI").Get<AzureOpenAI>();

builder.Services.AddAzureOpenAIChatCompletion(
    deploymentName: azureOpenAi.DeploymentName,
    endpoint: azureOpenAi.Endpoint,
    apiKey: azureOpenAi.ApiKey,
    modelId: azureOpenAi.ModelId);

var app = builder.Build();

var chat = app.Services.GetRequiredService<IChatCompletionService>();

var chatHistory = new ChatHistory();

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("AI: What am I?");
Console.ForegroundColor = ConsoleColor.Yellow;

Console.Write("Juan Miguel: ");
var whatAmI = Console.ReadLine();
chatHistory.AddSystemMessage(whatAmI!);

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("AI: Cool! How can I help?");
Console.ForegroundColor = ConsoleColor.Yellow;

while (true)
{
    Console.Write("Juan Miguel: ");
    var prompt = Console.ReadLine();

    chatHistory.AddUserMessage(prompt!);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("AI: ");

// var response = await chat.GetChatMessageContentsAsync(chatHistory);
// var lastMessage = response.Last();

    await foreach (var response in chat.GetStreamingChatMessageContentsAsync(chatHistory))
    {
        Console.Write(response);
        await Task.Delay(100);
    }
    
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine();
}