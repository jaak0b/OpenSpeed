using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenSpeed.UI.Controls
{
  public partial class IpAddressInput : UserControl
  {
    public static readonly DependencyProperty IpAddressProperty =
      DependencyProperty.Register(
        nameof(IpAddress), typeof(string), typeof(IpAddressInput),
        new FrameworkPropertyMetadata(
          "0.0.0.0",
          FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
          OnIpAddressChanged));

    public string IpAddress
    {
      get => (string)GetValue(IpAddressProperty);
      set => SetValue(IpAddressProperty, value);
    }

    public static readonly DependencyProperty IsConnectedProperty =
      DependencyProperty.Register(
        nameof(IsConnected), typeof(bool), typeof(IpAddressInput),
        new PropertyMetadata(false));

    public bool IsConnected
    {
      get => (bool)GetValue(IsConnectedProperty);
      set => SetValue(IsConnectedProperty, value);
    }

    private bool _updating;

    private TextBox[] Octets => [OctetOne, OctetTwo, OctetThree, OctetFour];

    public IpAddressInput()
    {
      InitializeComponent();
    }

    private static void OnIpAddressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is IpAddressInput ctrl && e.NewValue is string ip)
        ctrl.ParseIpToOctets(ip);
    }

    private void ParseIpToOctets(string ip)
    {
      if (_updating) return;
      _updating = true;
      try
      {
        var parts = ip?.Split('.') ?? [];
        for (var i = 0; i < 4; i++)
        {
          Octets[i].Text = i < parts.Length && int.TryParse(parts[i], out var v)
            ? v.ToString()
            : "0";
        }
      }
      finally
      {
        _updating = false;
      }
    }

    private void UpdateIpAddress()
    {
      if (_updating) return;
      _updating = true;
      try
      {
        IpAddress = string.Join(".", Octets.Select(t => string.IsNullOrEmpty(t.Text) ? "0" : t.Text));
      }
      finally
      {
        _updating = false;
      }
    }

    private void MoveFocus(TextBox current, int direction)
    {
      var idx = System.Array.IndexOf(Octets, current) + direction;
      if (idx >= 0 && idx < 4)
      {
        Octets[idx].Focus();
        Octets[idx].SelectAll();
      }
    }

    private void Octet_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (sender is not TextBox tb) return;

      if (tb.Text.Length > 0 && int.TryParse(tb.Text, out var val) && val > 255)
      {
        tb.Text = "255";
        tb.CaretIndex = 3;
        return;
      }

      if (tb.Text.Length == 3)
        MoveFocus(tb, +1);

      UpdateIpAddress();
    }

    private void Octet_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
      e.Handled = !e.Text.All(char.IsDigit);
    }

    private void Octet_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (sender is not TextBox tb) return;

      switch (e.Key)
      {
        case Key.OemPeriod:
        case Key.Decimal:
          MoveFocus(tb, +1);
          e.Handled = true;
          break;

        case Key.Back when tb.Text.Length == 0:
          MoveFocus(tb, -1);
          e.Handled = true;
          break;

        case Key.Left when tb.CaretIndex == 0:
          MoveFocus(tb, -1);
          e.Handled = true;
          break;

        case Key.Right when tb.CaretIndex == tb.Text.Length:
          MoveFocus(tb, +1);
          e.Handled = true;
          break;
      }
    }
  }
}
