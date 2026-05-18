using System.Windows;
using OpenSpeed.UI.ViewModel;

namespace OpenSpeed.UI.View;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
  public MainWindowViewModel MainWindowViewModel { get; }

  public MainWindow(MainWindowViewModel mainWindowViewModel)
  {
    MainWindowViewModel = mainWindowViewModel;
    DataContext = MainWindowViewModel;
    InitializeComponent();
  }

  private async void ButtonStartMeasurement_OnClick(object sender, RoutedEventArgs e) => await MainWindowViewModel.StartMeasurement();

  private async void ButtonCancelMeasurement_OnClick(object sender, RoutedEventArgs e) => await MainWindowViewModel.CancelMeasurement();
}