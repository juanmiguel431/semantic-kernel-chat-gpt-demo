using System.ComponentModel;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;

namespace Ai.Cli.Plugins;

public class AppMemory
{
    private readonly MemoryServerless _memory;

    public AppMemory(MemoryServerless memory)
    {
        _memory = memory;
    }
    
    [KernelFunction("ask_to_app_memory")]
    [Description("Responds to a question using the application's memory.")]
    [return: Description("The answer retrieved from the application's memory.")]
    public async Task<string> AskToAppMemory([Description("The question to be answered.")] string question)
    {
        var response = await _memory.AskAsync(question);
        return response.Result;
    }
}
