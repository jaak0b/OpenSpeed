using System.ComponentModel;
using System.Runtime.CompilerServices;
using OpenSpeed.Core.Controller;

namespace OpenSpeed.Core.Models.Configuration
{

  public class MeasurementConfiguration(IKeyValueStore valueStore) : INotifyPropertyChanged
  {
    public int SpeedStepInterval
    {
      get => valueStore.GetValue(nameof(SpeedStepInterval), 1);
      set
      {
        valueStore.SetValue(nameof(SpeedStepInterval), value);
        OnPropertyChanged();
      }
    }

    public int StartingSpeedStep
    {
      get => valueStore.GetValue(nameof(StartingSpeedStep), 1);
      set
      {
        valueStore.SetValue(nameof(StartingSpeedStep), value);
        OnPropertyChanged();
      }
    }

    public int MaxSpeed
    {
      get => valueStore.GetValue<int>(nameof(MaxSpeed));
      set
      {
        valueStore.SetValue(nameof(MaxSpeed), value);
        OnPropertyChanged();
      }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new (propertyName));
    }
  }
}