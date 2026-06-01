using System.Windows;
using OpenSpeed.UI.ViewModel;

namespace OpenSpeed.UI.View;

public partial class MainWindow : Window
{
  public MainWindowViewModel MainWindowViewModel { get; }

  public MainWindow(MainWindowViewModel mainWindowViewModel)
  {
    MainWindowViewModel = mainWindowViewModel;
    DataContext = MainWindowViewModel;
    InitializeComponent();
    Plot.Controller = new OxyPlot.PlotController();
  }

  private async void ButtonStartMeasurement_OnClick(object sender, RoutedEventArgs e) => await MainWindowViewModel.StartMeasurement();

  private async void ButtonCancelMeasurement_OnClick(object sender, RoutedEventArgs e) => await MainWindowViewModel.CancelMeasurement();

  private async void ButtonStartLengthMeasurement_OnClick(object sender, RoutedEventArgs e) => await MainWindowViewModel.StartLengthMeasurement();

  private async void ButtonCancelLengthMeasurement_OnClick(object sender, RoutedEventArgs e) => await MainWindowViewModel.CancelLengthMeasurement();

  private void ButtonExport_OnClick(object sender, RoutedEventArgs e) => MainWindowViewModel.ExportResults();

  private void ThemeToggle_OnClick(object sender, RoutedEventArgs e) => MainWindowViewModel.ToggleTheme();

  private void Minimize_OnClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

  private void MaximizeRestore_OnClick(object sender, RoutedEventArgs e)
    => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

  private void Close_OnClick(object sender, RoutedEventArgs e) => Close();

  protected override void OnStateChanged(System.EventArgs e)
  {
    base.OnStateChanged(e);
    ButtonMaximize.Content = ((char)(WindowState == WindowState.Maximized ? 0xE923 : 0xE922)).ToString();
  }
}
