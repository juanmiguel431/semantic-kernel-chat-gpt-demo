// See https://aka.ms/new-console-template for more information
// https://dzone.com/users/4857648/aneeshlalga.html
// https://dzone.com/articles/zero-to-ai-hero-part-one-semantic-kernel
// https://dzone.com/articles/zero-to-ai-hero-part-two-semantic-kernel-plugins
// https://dzone.com/articles/zero-to-ai-hero-part-three-power-of-agents
// https://dzone.com/articles/zero-to-ai-hero-part-four-local-language-models

// https://learn.microsoft.com/en-us/semantic-kernel/concepts/kernel?pivots=programming-language-csharp

#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

using Ai.Cli.Models;
using Ai.Cli.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.KernelMemory;

namespace Ai.Cli;

public class Program
{
    private static int _merchantId;

    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        var azureOpenAi = builder.Configuration.GetSection("AzureOpenAI").Get<AzureOpenAI>()!;
        var azureOpenAiTextEmbedding = builder.Configuration.GetSection("AzureOpenAITextEmbedding").Get<AzureOpenAI>()!;
        var azureAiSearch = builder.Configuration.GetSection("AzureAISearch").Get<AzureOpenAI>()!;

        // Add the OpenAI chat completion service as a singleton
        builder.Services.AddAzureOpenAIChatCompletion(
            deploymentName: azureOpenAi.DeploymentName,
            endpoint: azureOpenAi.Endpoint,
            apiKey: azureOpenAi.ApiKey,
            modelId: azureOpenAi.ModelId
        );

        // #pragma warning disable SKEXP0010
        // builder.Services.AddOpenAIChatCompletion(
        //     modelId: "phi3",
        //     apiKey: null,
        //     endpoint: new Uri("http://localhost:11434/v1")
        // );
        // #pragma warning restore SKEXP0010

        var chatConfig = new AzureOpenAIConfig
        {
            APIKey = azureOpenAi.ApiKey,
            Deployment = azureOpenAi.DeploymentName,
            Endpoint = azureOpenAi.Endpoint,
            APIType = AzureOpenAIConfig.APITypes.ChatCompletion,
            Auth = AzureOpenAIConfig.AuthTypes.APIKey
        };

        var textEmbeddingConfig = new AzureOpenAIConfig
        {
            APIKey = azureOpenAiTextEmbedding.ApiKey,
            Deployment = azureOpenAiTextEmbedding.DeploymentName,
            Endpoint = azureOpenAiTextEmbedding.Endpoint,
            APIType = AzureOpenAIConfig.APITypes.EmbeddingGeneration,
            Auth = AzureOpenAIConfig.AuthTypes.APIKey
        };

        var aiSearchConfig = new AzureAISearchConfig
        {
            APIKey = azureAiSearch.ApiKey,
            Endpoint = azureAiSearch.Endpoint,
            Auth = AzureAISearchConfig.AuthTypes.APIKey,
        };

        var memory = new KernelMemoryBuilder()
            .WithAzureOpenAITextGeneration(chatConfig)
            .WithAzureOpenAITextEmbeddingGeneration(textEmbeddingConfig)
            .WithAzureAISearchMemoryDb(aiSearchConfig)
            .Build();

        _merchantId = 1;
        
        // var documentId1 = await ImportDocument1(memory);
        // await memory.DeleteDocumentAsync(documentId1);
        
        // var documentId2 = await ImportDocument2(memory);
        // return;

        builder.Services.AddSingleton<IKernelMemory>(_ => memory);

        // Create singletons of your plugins
        builder.Services.AddSingleton<TimeTeller>();
        builder.Services.AddSingleton<ElectricCar>();
        builder.Services.AddSingleton<TripPlanner>();
        builder.Services.AddSingleton<WeatherForecaster>();
        builder.Services.AddSingleton<AppMemory>();

        // builder.Services.AddSingleton(() => new TimeTeller());
        // builder.Services.AddSingleton(() => new ElectricCar());
        // builder.Services.AddSingleton(() => new TripPlanner());
        // builder.Services.AddSingleton(() => new WeatherForecaster());
        // builder.Services.AddSingleton(() => new AppMemory());

        // Create the plugin collection (using the KernelPluginFactory to create plugins from objects)
        builder.Services.AddSingleton<KernelPluginCollection>(serviceProvider =>
            [
                KernelPluginFactory.CreateFromObject(serviceProvider.GetRequiredService<TimeTeller>()),
                KernelPluginFactory.CreateFromObject(serviceProvider.GetRequiredService<ElectricCar>()),
                // KernelPluginFactory.CreateFromObject(serviceProvider.GetRequiredService<TripPlanner>()),
                KernelPluginFactory.CreateFromObject(serviceProvider.GetRequiredService<WeatherForecaster>()),
                KernelPluginFactory.CreateFromObject(serviceProvider.GetRequiredService<AppMemory>()),
            ]
        );

        builder.Services.AddScoped(_ => new AppAiContext
        {
            MerchantId = _merchantId
        });

        // Finally, create the Kernel service with the service provider and plugin collection
        builder.Services.AddTransient(serviceProvider =>
        {
            var pluginCollection = serviceProvider.GetRequiredService<KernelPluginCollection>();
            return new Kernel(serviceProvider, pluginCollection);
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

        // var agent = new ChatCompletionAgent
        // {
        //     Name = "TripPlannerAgent",
        //     Instructions =
        //         """
        //         I'm going to plan a short one day vacation to {{destination}}. I would like to start around {{timeOfDay}}.
        //         Before I do that, can you succinctly recommend the top 2 steps I should take in a numbered list?
        //         I want to make sure I don't forget to pack anything for the weather at my destination and my car is sufficiently charged before I start the journey.
        //         """,
        //     Kernel = kernel,
        //     Arguments = new KernelArguments(openAiSettings)
        // };
        
        var agent = new ChatCompletionAgent
        {
            Name = "DiabetesExpertAgent",
            Instructions =
                """
                Always execute the function ask_to_app_memory to retrieve data from the app memory.
                """,
            Kernel = kernel,
            Arguments = new KernelArguments(openAiSettings)
        };

        while (true)
        {
            Console.Write("Juan Miguel: ");
            var prompt = Console.ReadLine();

            chatHistory.AddUserMessage(prompt!);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("AI: ");

            var result = agent.InvokeAsync(history: chatHistory);

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
    }

    private static async Task<string> ImportDocument1(IKernelMemory memory)
    {
        const string filePath = @"C:\Cloud\OneDrive\Documents\MemberCare\AI Knowledge\Diabetes ES.pdf";
        var fileBytes = await File.ReadAllBytesAsync(filePath);
            
        using var memoryStream = new MemoryStream(fileBytes);
            
        var tags = new TagCollection
        {
            new KeyValuePair<string, List<string?>>("Health", ["Diabetes", "Chronic Diseases", "Glucose", "Endocrinology"]),
            new KeyValuePair<string, List<string?>>("Education", ["Disease Awareness", "Health Education", "Prevention"])
        };
        
        tags.Add("MerchantId", [_merchantId.ToString()]);
        
        var documentId = await memory.ImportDocumentAsync(
            content: memoryStream,
            fileName: "Diabetes ES.pdf",
            tags: tags,
            index: "default"
        );

        return documentId;
    }
    
    private static async Task<string> ImportDocument2(IKernelMemory memory)
    {
        const string filePath = @"C:\Cloud\OneDrive\Documents\MemberCare\AI Knowledge\Nutrition.pdf";
        var fileBytes = await File.ReadAllBytesAsync(filePath);
            
        using var memoryStream = new MemoryStream(fileBytes);
            
        var tags = new TagCollection
        {
            new KeyValuePair<string, List<string?>>("Health", [
                "Plant-Based Nutrition",
                "Healthy Eating",
                "Food as Medicine",
                "Dietary Spectrum",
                "Nutritional Guide",
                "Whole Foods",
                "Healthy Lifestyle"
            ]),
            new KeyValuePair<string, List<string?>>("Cooking", [
                "Vegan Recipes",
                "Plant-Based Recipes",
                "Cooking Essentials",
                "Quick Recipes",
                "Kitchen Tips"
            ]),
            new KeyValuePair<string, List<string?>>("Recipes", [
                "Breakfast Recipes",
                "Lunch Recipes",
                "Dinner Recipes",
                "Snack Ideas",
                "Desserts",
                "Smoothie Recipes",
                "Plant-Based Meal Prep"
            ]),
            new KeyValuePair<string, List<string?>>("Lifestyle", [
                "Meal Planning",
                "Sample Menu Plan"
            ])
        };
        
        tags.Add("MerchantId", [_merchantId.ToString()]);
        
        var documentId = await memory.ImportDocumentAsync(
            content: memoryStream,
            fileName: "Nutrition.pdf",
            tags: tags,
            index: "default"
        );

        return documentId;
    }
}