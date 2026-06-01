using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenSpeed.UI.Controls
{
  public partial class NumericUpDown : UserControl
  {
    public static readonly DependencyProperty ValueProperty =
      DependencyProperty.Register(
        nameof(Value), typeof(int), typeof(NumericUpDown),
        new FrameworkPropertyMetadata(
          0,
          FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
          OnValueChanged));

    public int Value
    {
      get => (int)GetValue(ValueProperty);
      set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty MinimumProperty =
      DependencyProperty.Register(
        nameof(Minimum), typeof(int), typeof(NumericUpDown),
        new PropertyMetadata(0));

    public int Minimum
    {
      get => (int)GetValue(MinimumProperty);
      set => SetValue(MinimumProperty, value);
    }

    public static readonly DependencyProperty MaximumProperty =
      DependencyProperty.Register(
        nameof(Maximum), typeof(int), typeof(NumericUpDown),
        new PropertyMetadata(int.MaxValue));

    public int Maximum
    {
      get => (int)GetValue(MaximumProperty);
      set => SetValue(MaximumProperty, value);
    }

    private bool _updating;

    public NumericUpDown()
    {
      InitializeComponent();
      Loaded += (_, _) => SyncTextToValue(Value);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is NumericUpDown ctrl && e.NewValue is int v)
        ctrl.SyncTextToValue(v);
    }

    private void SyncTextToValue(int v)
    {
      if (_updating) return;
      _updating = true;
      try { ValueBox.Text = v.ToString(); }
      finally { _updating = false; }
    }

    private void Increment() => Value = Math.Min(Value + 1, Maximum);
    private void Decrement() => Value = Math.Max(Value - 1, Minimum);

    private void ValueBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (_updating) return;
      if (!int.TryParse(ValueBox.Text, out var parsed)) return;

      var clamped = Math.Max(Minimum, Math.Min(Maximum, parsed));
      _updating = true;
      try { Value = clamped; }
      finally { _updating = false; }
    }

    private void ValueBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
      foreach (var c in e.Text)
      {
        if (char.IsDigit(c)) continue;
        if (c == '-' && Minimum < 0 && ValueBox.CaretIndex == 0 && !ValueBox.Text.Contains('-')) continue;
        e.Handled = true;
        return;
      }
    }

    private void ValueBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Up:
          Increment();
          e.Handled = true;
          break;
        case Key.Down:
          Decrement();
          e.Handled = true;
          break;
      }
    }

    private void PartUp_Click(object sender, RoutedEventArgs e) => Increment();
    private void PartDown_Click(object sender, RoutedEventArgs e) => Decrement();
  }
}
