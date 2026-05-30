using System.ComponentModel;
using System.Runtime.CompilerServices;
using OpenSpeed.Core.Controller;

namespace OpenSpeed.Core.Models.Configuration
{
  public class LengthMeasurementConfiguration(IKeyValueStore valueStore) : INotifyPropertyChanged
  {
    public int SpeedStep
    {
      get => valueStore.GetValue(nameof(SpeedStep), 10);
      set
      {
        valueStore.SetValue(nameof(SpeedStep), value);
        OnPropertyChanged();
      }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new(propertyName));
    }
  }
}
