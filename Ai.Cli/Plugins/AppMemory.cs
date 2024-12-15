using System.ComponentModel;
using Ai.Cli.Models;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;

namespace Ai.Cli.Plugins;

public class AppMemory
{
    private readonly IKernelMemory _memory;
    private readonly AppAiContext _context;

    public AppMemory(IKernelMemory memory, AppAiContext context)
    {
        _memory = memory;
        _context = context;
    }
    
    [KernelFunction("ask_to_app_memory")]
    [Description("Responds to a question using the application's memory.")]
    [return: Description("The answer retrieved from the application's memory.")]
    public async Task<string> AskToAppMemory([Description("The question to be answered.")] string question)
    {
        var filter = new MemoryFilter();
        filter.ByTag("MerchantId", _context.MerchantId.ToString());
        
        var response = await _memory.AskAsync(question, filter: filter);
        return response.Result;
    }
}
