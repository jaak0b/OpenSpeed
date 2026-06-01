using OpenSpeed.Core.Models;
using OpenSpeed.Core.Models.Configuration;

namespace OpenSpeed.Core.Provider
{
  public interface ISpeedStepMeasurementProvider
  {
    IEnumerable<SpeedStepMeasurement> Provide(MeasurementConfiguration configuration, DccSpeedMode dccSpeedMode);
  }

  public class SpeedStepMeasurementProvider() : ISpeedStepMeasurementProvider
  {
    public IEnumerable<SpeedStepMeasurement> Provide(MeasurementConfiguration configuration, DccSpeedMode dccSpeedMode)
    {
      if (configuration.StartingSpeedStep <= 0)
        throw new ArgumentException("Starting speed step must be bigger then 0.", nameof(configuration.StartingSpeedStep));

      var maxSpeed = (int)dccSpeedMode;
      if (maxSpeed <= configuration.StartingSpeedStep)
        throw new ArgumentException("Staring speed step must be lower then dcc speed mode.", nameof(configuration.StartingSpeedStep));

      if (configuration.SpeedStepInterval <= 0)
        throw new ArgumentException("Steep step interval must be bigger then 0.", nameof(configuration.StartingSpeedStep));

      for (var i = configuration.StartingSpeedStep; i <= maxSpeed; i += configuration.SpeedStepInterval)
        yield return new () { SpeedStep = i };
    }
  }
}