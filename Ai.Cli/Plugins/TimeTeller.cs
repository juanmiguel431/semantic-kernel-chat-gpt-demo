using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace Ai.Cli.Plugins;

public class TimeTeller
{
    [Description("This function retrieves the current time.")]
    [KernelFunction]
    public string GetCurrentTime()
    {
        return DateTime.Now.ToString("F");
    }

    [Description("This function checks if in off-peak period between 9pm and 7am")]
    [KernelFunction]
    public bool IsOffPeak()
    {
        return DateTime.Now.Hour < 7 || DateTime.Now.Hour >= 21;
    }
}