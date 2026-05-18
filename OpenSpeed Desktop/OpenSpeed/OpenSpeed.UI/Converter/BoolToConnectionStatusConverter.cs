using IPUserControls;
using System.Globalization;
using System.Windows.Data;

namespace OpenSpeed.UI.Converter
{
  public class BoolToConnectionStatusConverter : IValueConverter
  {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      if (value is bool b)
        return b ? ConnectionStatus.Connected : ConnectionStatus.Disconnected;

      return ConnectionStatus.Error;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      if (value is ConnectionStatus status)
        return status == ConnectionStatus.Connected;

      return false;
    }
  }
}