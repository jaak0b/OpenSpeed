using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OpenSpeed.Core.Models.Configuration
{

  public class MeasurementConfiguration : INotifyPropertyChanged
  {
    private int _speedStepInterval = 5;
    private int _startingSpeedStep = 4;
    private int _maxSpeed;

    public int SpeedStepInterval
    {
      get => _speedStepInterval;
      set
      {
        if (value == _speedStepInterval)
          return;
        _speedStepInterval = value;
        OnPropertyChanged();
      }
    }

    public int StartingSpeedStep
    {
      get => _startingSpeedStep;
      set
      {
        if (value == _startingSpeedStep)
          return;
        _startingSpeedStep = value;
        OnPropertyChanged();
      }
    }

    public int MaxSpeed
    {
      get => _maxSpeed;
      set
      {
        if (value == _maxSpeed)
          return;
        _maxSpeed = value;
        OnPropertyChanged();
      }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}