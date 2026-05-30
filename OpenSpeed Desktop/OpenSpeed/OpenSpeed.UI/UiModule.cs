using Autofac;
using OpenSpeed.UI.Localization;
using OpenSpeed.UI.View;
using OpenSpeed.UI.ViewModel;

namespace OpenSpeed.UI
{
  public class UiModule : Module
  {
    override protected void Load(ContainerBuilder builder)
    {
      builder.RegisterInstance(LocalizationManager.Instance).AsSelf().SingleInstance();
      builder.RegisterType<SpeedPlotViewModel>().AsSelf().SingleInstance();
      builder.RegisterType<MainWindowViewModel>().AsSelf().SingleInstance();
      builder.RegisterType<MainWindow>().AsSelf().SingleInstance();
    }
  }
}