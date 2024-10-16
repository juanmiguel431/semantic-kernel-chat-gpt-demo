using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace Ai.Cli.Plugins;

public class ElectricCar
{
    private bool _isCarCharging;
    
    [Description("This function starts charging the electric car.")]
    [KernelFunction]
    public string StartCharging()
    {
        if (_isCarCharging)
        {
            return "Car is already charging.";
        }
        
        _isCarCharging = true; // This is where you would call httos://tesla/api/mycar/start. Kidding, you got the idea.
        return "Charging started.";
    }

    [Description("This function stops charging the electric car.")]
    [KernelFunction]
    public string StopCharging()
    {
        if (!_isCarCharging)
        {
            return "Car is not charging.";
        }
        
        _isCarCharging = false;
        return "Charging stopped.";
    }
}