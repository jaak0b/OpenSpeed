using Autofac;
using OpenSpeed.Core.Interfaces;
using Z21.Autofac;

namespace OpenSpeed.Adapter
{
  public class AdapterModule : Module
  {
    override protected void Load(ContainerBuilder builder)
    {
      builder.Register(c => new HttpClient()).AsSelf().SingleInstance();
      builder.RegisterType<RestLocomotiveSpeedSensor>().As<ILocomotiveSpeedSensor>().SingleInstance();
      builder.RegisterType<Z21Controller>().As<IZ21Controller>().SingleInstance();
      builder.AddZ21();
    }
  }
}