// See https://aka.ms/new-console-template for more information

using Ai.Cli;
using Ai.Cli.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var builder = Host.CreateApplicationBuilder(args);

var azureOpenAi = builder.Configuration.GetSection("AzureOpenAI").Get<AzureOpenAI>();

builder.Services.AddSingleton<Kernel>(serviceProvider =>
{
    var kernelBuilder = Kernel.CreateBuilder();

    kernelBuilder.AddAzureOpenAIChatCompletion(
        deploymentName: azureOpenAi.DeploymentName,
        endpoint: azureOpenAi.Endpoint,
        apiKey: azureOpenAi.ApiKey,
        modelId: azureOpenAi.ModelId);

    kernelBuilder.Plugins.AddFromType<TimeTeller>();
    kernelBuilder.Plugins.AddFromType<ElectricCar>();
    
    var kernel = kernelBuilder.Build();
    return kernel;
});

var app = builder.Build();

var kernel = app.Services.GetRequiredService<Kernel>();
var chat = kernel.GetRequiredService<IChatCompletionService>();

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

var openAiSettings = new OpenAIPromptExecutionSettings
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
};

while (true)
{
    Console.Write("Juan Miguel: ");
    var prompt = Console.ReadLine();

    chatHistory.AddUserMessage(prompt!);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("AI: ");

// var response = await chat.GetChatMessageContentsAsync(chatHistory);
// var lastMessage = response.Last();

    await foreach (var response in chat.GetStreamingChatMessageContentsAsync(
                       chatHistory: chatHistory,
                       kernel: kernel,
                       executionSettings: openAiSettings))
    {
        Console.Write(response);
        await Task.Delay(100);
    }
    
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine();
}