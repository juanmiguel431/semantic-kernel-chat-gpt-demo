using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace Ai.Cli.Plugins;

public class ElectricCar
{
    private bool _isCarCharging;
    private int _batteryLevel;
    private CancellationTokenSource? _source;
    
    // Mimic charging the electric car, using a periodic timer.
    private async Task AddJuice()
    {
        _source = new CancellationTokenSource();
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(0.5));

        while (await timer.WaitForNextTickAsync(_source.Token))
        {
            _batteryLevel++;
            if (_batteryLevel == 100)
            {
                _isCarCharging = false;
                Console.WriteLine("\rBattery is full.");
                await _source.CancelAsync();
                return;
            }
            
            Console.Write("\rCharging {0}%", _batteryLevel);
        }
    }
    
    [KernelFunction]
    [Description("This function checks if the electric car is currently charging.")]
    [return: Description("True if the car is charging; otherwise, false.")]
    public bool IsCarCharging()
    {
        return _isCarCharging;
    }

    [KernelFunction]
    [Description("This function returns the current battery level of the electric car.")]
    [return: Description("The current battery level.")]
    public int GetBatteryLevel()
    {
        return _batteryLevel;
    }

    [KernelFunction]
    [Description("This function starts charging the electric car.")]
    [return: Description("A message indicating the status of the charging process.")]
    public string StartCharging()
    {
        if (_isCarCharging)
        {
            return "Car is already charging.";
        }

        if (_batteryLevel == 100)
        {
            return "Battery is already full.";
        }

        Task.Run(AddJuice);

        _isCarCharging = true;
        return "Charging started.";
    }

    [KernelFunction]
    [Description("This function stops charging the electric car.")]
    [return: Description("A message indicating the status of the charging process.")]
    public string StopCharging()
    {
        if (!_isCarCharging)
        {
            return "Car is not charging.";
        }
        
        _isCarCharging = false;
        _source?.Cancel();
        return "Charging stopped.";
    }
}