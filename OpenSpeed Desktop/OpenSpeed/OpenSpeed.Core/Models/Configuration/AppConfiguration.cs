using System.ComponentModel;
using System.Runtime.CompilerServices;
using OpenSpeed.Core.Controller;

namespace OpenSpeed.Core.Models.Configuration
{
  public class AppConfiguration(IKeyValueStore valueStore) : INotifyPropertyChanged
  {
    public Language Language
    {
      get => valueStore.GetValue(nameof(Language), Language.English);
      set
      {
        valueStore.SetValue(nameof(Language), value);
        OnPropertyChanged();
      }
    }

    public Theme Theme
    {
      get => valueStore.GetValue(nameof(Theme), Theme.Light);
      set
      {
        valueStore.SetValue(nameof(Theme), value);
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
