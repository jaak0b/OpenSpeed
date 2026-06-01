using System;
using System.Linq;
using System.Windows;
using OpenSpeed.Core.Models;

namespace OpenSpeed.UI.Theming
{
  public class ThemeManager
  {
    public static ThemeManager Instance { get; } = new();

    public Theme Current { get; private set; } = Theme.Light;

    public event EventHandler? ThemeChanged;

    public void ApplyTheme(Theme theme)
    {
      var dictionaries = Application.Current.Resources.MergedDictionaries;

      var existing = dictionaries.FirstOrDefault(
        d => d.Source != null && d.Source.OriginalString.Contains("Colors.", StringComparison.OrdinalIgnoreCase));

      if (existing != null && Current == theme)
        return;

      var updated = new ResourceDictionary
                    {
                      Source = new Uri($"Styles/Colors.{theme}.xaml", UriKind.Relative)
                    };

      if (existing != null)
        dictionaries.Remove(existing);

      dictionaries.Insert(0, updated);

      Current = theme;
      ThemeChanged?.Invoke(this, EventArgs.Empty);
    }
  }
}
