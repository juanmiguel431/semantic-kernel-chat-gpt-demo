using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace Ai.Cli.Plugins;

public class TimeTeller
{
    [KernelFunction]
    [Description("This function retrieves the current time.")]
    [return: Description("The current time.")]
    public string GetCurrentTime()
    {
        return DateTime.Now.ToString("F");
    }

    [KernelFunction]
    [Description("This function checks if the current time is off-peak.")]
    [return: Description("True if the current time is off-peak; otherwise, false.")]
    public bool IsOffPeak()
    {
        return DateTime.Now.Hour < 7 || DateTime.Now.Hour >= 21;
    }
}