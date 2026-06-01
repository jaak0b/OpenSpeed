using System.ComponentModel;
using System.Runtime.CompilerServices;
using OpenSpeed.Core.Controller;

namespace OpenSpeed.Core.Models.Configuration
{
  public class LocomotiveConfiguration(IKeyValueStore valueStore) : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler? PropertyChanged;

    public int LocomotiveScale
    {
      get => valueStore.GetValue(nameof(LocomotiveScale), 87);
      set
      {
        valueStore.SetValue(nameof(LocomotiveScale), value);
        OnPropertyChanged();
      }
    }


    public int DecoderAddress
    {
      get => valueStore.GetValue<int>(nameof(DecoderAddress));
      set
      {
        valueStore.SetValue(nameof(DecoderAddress), value);
        OnPropertyChanged();
      }
    }

    public DccSpeedMode DccSpeedMode
    {
      get => valueStore.GetValue(nameof(DccSpeedMode), DccSpeedMode.Steps128);
      set
      {
        valueStore.SetValue(nameof(DccSpeedMode), value);
        OnPropertyChanged();
      }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new(propertyName));
    }
  }
}