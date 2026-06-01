using Autofac;
using OpenSpeed.Core.Controller;
using OpenSpeed.Core.Models.Configuration;
using OpenSpeed.Core.Provider;

namespace OpenSpeed.Core
{
  public class CoreModule : Module
  {
    override protected void Load(ContainerBuilder builder)
    {
      builder.RegisterType<SpeedStepMeasurementProvider>().As<ISpeedStepMeasurementProvider>().SingleInstance();
      builder.RegisterType<MeasurementController>().As<IMeasurementController>().SingleInstance();
      builder.RegisterType<SpeedStepMeasurementController>().As<ISpeedStepMeasurementController>().SingleInstance();
      builder.RegisterType<LengthMeasurementController>().As<ILengthMeasurementController>().SingleInstance();

      builder.RegisterType<DelayProvider>().As<IDelayProvider>().SingleInstance();

      builder.RegisterType<EndPointConfiguration>().AsSelf().SingleInstance();
      builder.RegisterType<LocomotiveConfiguration>().AsSelf().SingleInstance();
      builder.RegisterType<MeasurementConfiguration>().AsSelf().SingleInstance();
      builder.RegisterType<LengthMeasurementConfiguration>().AsSelf().SingleInstance();
      builder.RegisterType<AppConfiguration>().AsSelf().SingleInstance();

      builder.Register(ctx =>
                       {
                         var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OpenSpeed");
                         return new JsonKeyValueStore(path);
                       })
             .As<IKeyValueStore>()
             .SingleInstance();
    }
  }
}