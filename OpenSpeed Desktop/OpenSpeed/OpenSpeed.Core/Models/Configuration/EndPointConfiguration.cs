using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OpenSpeed.Core.Models.Configuration
{
  public class EndPointConfiguration : INotifyPropertyChanged
  {
    private string _z21IpAddress = "192.168.0.111";

    //Z21Configuration.Defaults.IpEndPoint.Address.ToString();
    private string _speedSensorIpAddress = "192.168.0.55";

    public string Z21IpAddress
    {
      get => _z21IpAddress;
      set
      {
        if (value == _z21IpAddress)
          return;
        _z21IpAddress = value;
        OnPropertyChanged();
      }
    }

    public string SpeedSensorIpAddress
    {
      get => _speedSensorIpAddress;
      set
      {
        if (value == _speedSensorIpAddress)
          return;
        _speedSensorIpAddress = value;
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