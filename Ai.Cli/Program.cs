// See https://aka.ms/new-console-template for more information


using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings()
{
    Args = args
});

builder.Services.AddAzureOpenAIChatCompletion(
    deploymentName: "",
    endpoint: "",
    apiKey: "",
    modelId: "");

var app = builder.Build();

var chat = app.Services.GetRequiredService<IChatCompletionService>();

var chatHistory = new ChatHistory();

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("AI: What am I?");
Console.ForegroundColor = ConsoleColor.Yellow;

while (true)
{
    Console.WriteLine("Juan Miguel: ");
    var whatAmI = Console.ReadLine();
    chatHistory.AddSystemMessage(whatAmI!);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("AI: Coll! How can I help?");
    Console.ForegroundColor = ConsoleColor.Yellow;

    Console.WriteLine("Juan Miguel: ");
    var prompt = Console.ReadLine();

    chatHistory.AddUserMessage(prompt!);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("AI: ");

// var response = await chat.GetChatMessageContentsAsync(chatHistory);
// var lastMessage = response.Last();

    await foreach (var response in chat.GetStreamingChatMessageContentsAsync(chatHistory))
    {
        Console.WriteLine(response);
    }
    
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine();
}