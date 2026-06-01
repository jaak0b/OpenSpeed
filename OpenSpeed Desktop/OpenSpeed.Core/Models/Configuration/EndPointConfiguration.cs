using System.ComponentModel;
using System.Runtime.CompilerServices;
using OpenSpeed.Core.Controller;

namespace OpenSpeed.Core.Models.Configuration
{
  public class EndPointConfiguration(IKeyValueStore valueStore) : INotifyPropertyChanged
  {
    public string Z21IpAddress
    {
      get => valueStore.GetValue<string>(nameof(Z21IpAddress)) ?? "192.168.0.111";
      set
      {
        valueStore.SetValue(nameof(Z21IpAddress), value);;
        OnPropertyChanged();
      }
    }

    public string SpeedSensorIpAddress
    {
      get => valueStore.GetValue<string>(nameof(SpeedSensorIpAddress)) ?? "";
      set
      {
        valueStore.SetValue(nameof(SpeedSensorIpAddress), value);
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