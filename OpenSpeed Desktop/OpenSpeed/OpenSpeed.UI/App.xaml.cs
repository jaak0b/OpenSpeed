using System.Windows;
using Autofac;
using OpenSpeed.Composition;
using OpenSpeed.UI.View;

namespace OpenSpeed.UI
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private void OnStartup(object sender, StartupEventArgs e)
    {
      var container = Bootstrapper.Initialize(new UiModule());
      var mainWindow = container.Resolve<MainWindow>();
      mainWindow.Show();
    }
  }
}