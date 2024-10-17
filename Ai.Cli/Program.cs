// See https://aka.ms/new-console-template for more information
// https://dzone.com/users/4857648/aneeshlalga.html
// https://dzone.com/articles/zero-to-ai-hero-part-one-semantic-kernel
// https://dzone.com/articles/zero-to-ai-hero-part-two-semantic-kernel-plugins
// https://dzone.com/articles/zero-to-ai-hero-part-three-power-of-agents
// https://dzone.com/articles/zero-to-ai-hero-part-four-local-language-models

using Ai.Cli;
using Ai.Cli.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var builder = Host.CreateApplicationBuilder(args);

var azureOpenAi = builder.Configuration.GetSection("AzureOpenAI").Get<AzureOpenAI>()!;

builder.Services.AddScoped<Kernel>(_ =>
{
    var kernelBuilder = Kernel.CreateBuilder();

    kernelBuilder.AddAzureOpenAIChatCompletion(
        deploymentName: azureOpenAi.DeploymentName,
        endpoint: azureOpenAi.Endpoint,
        apiKey: azureOpenAi.ApiKey,
        modelId: azureOpenAi.ModelId);

    kernelBuilder.Plugins.AddFromType<TimeTeller>();
    kernelBuilder.Plugins.AddFromType<ElectricCar>();
    kernelBuilder.Plugins.AddFromType<TripPlanner>();
    kernelBuilder.Plugins.AddFromType<WeatherForecaster>();

    var kernel = kernelBuilder.Build();
    return kernel;
});

var app = builder.Build();

var kernel = app.Services.GetRequiredService<Kernel>();
var chat = kernel.GetRequiredService<IChatCompletionService>();

var chatHistory = new ChatHistory();

// Console.ForegroundColor = ConsoleColor.Green;
// Console.WriteLine("AI: What am I?");
// Console.ForegroundColor = ConsoleColor.Yellow;
//
// Console.Write("Juan Miguel: ");
// var whatAmI = Console.ReadLine();
// chatHistory.AddSystemMessage(whatAmI!);

chatHistory.AddSystemMessage(
    """
    You are a friendly assistant who likes to follow the rules. You will complete required steps
    and request approval before taking any consequential actions. If the user doesn't provide
    enough information for you to complete a task, you will keep asking questions until you have
    enough information to complete the task.
    """);

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("AI: How can I help you today?");
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

    var result = chat.GetStreamingChatMessageContentsAsync(
        chatHistory: chatHistory,
        kernel: kernel,
        executionSettings: openAiSettings);

    var assistantMessage = "";
    await foreach (var response in result)
    {
        assistantMessage += response.Content;
        Console.Write(response);
        await Task.Delay(100);
    }

    chatHistory.AddAssistantMessage(assistantMessage);

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine();
}