using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OpenSpeed.Core.Models.Configuration
{
  public class LocomotiveConfiguration : INotifyPropertyChanged
  {
    private int _decoderAddress = 17;
    private DccSpeedMode _dccSpeedMode = DccSpeedMode.Steps128;
    private int _locomotiveScale = 87;

    public event PropertyChangedEventHandler? PropertyChanged;
    
    public int LocomotiveScale
    {
      get => _locomotiveScale;
      set
      {
        if (value == _locomotiveScale)
          return;
        _locomotiveScale = value;
        OnPropertyChanged();
      }
    }

    
    public int DecoderAddress
    {
      get => _decoderAddress;
      set
      {
        if (value == _decoderAddress)
          return;
        _decoderAddress = value;
        OnPropertyChanged();
      }
    }

    public DccSpeedMode DccSpeedMode
    {
      get => _dccSpeedMode;
      set
      {
        if (value == _dccSpeedMode)
          return;

        _dccSpeedMode = value;
        OnPropertyChanged();
      }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new(propertyName));
    }
  }
}