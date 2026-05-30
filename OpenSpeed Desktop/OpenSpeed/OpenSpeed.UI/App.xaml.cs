using System.Windows;
using Autofac;
using OpenSpeed.Composition;
using OpenSpeed.Core.Models.Configuration;
using OpenSpeed.UI.Localization;
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
      LocalizationManager.Instance.SetLanguage(container.Resolve<AppConfiguration>().Language);
      var mainWindow = container.Resolve<MainWindow>();
      mainWindow.Show();
    }
  }
}