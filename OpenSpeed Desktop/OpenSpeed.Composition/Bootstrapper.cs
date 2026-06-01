using Autofac;
using Autofac.Core;
using Autofac.Extras.CommonServiceLocator;
using CommonServiceLocator;
using Microsoft.Extensions.Logging;
using OpenSpeed.Adapter;
using OpenSpeed.Core;
using IContainer = Autofac.IContainer;

namespace OpenSpeed.Composition
{
  public class Bootstrapper
  {
    public static IContainer Initialize(IModule? module, ILogger? logger = null)
    {
      // if (logger is not null)
      //   serviceCollection.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger, true));

      ContainerBuilder builder = new();
      builder.RegisterModule(new AdapterModule());
      builder.RegisterModule(new CoreModule());
      if (module is not null)
        builder.RegisterModule(module);

      var container = builder.Build();
      ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocator(container));

      return container;
    }
  }
}