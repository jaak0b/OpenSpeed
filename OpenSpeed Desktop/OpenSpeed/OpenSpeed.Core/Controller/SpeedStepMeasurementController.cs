using OpenSpeed.Core.Interfaces;
using OpenSpeed.Core.Models.Configuration;
using OpenSpeed.Core.Provider;

namespace OpenSpeed.Core.Controller
{
  public interface ISpeedStepMeasurementController
  {
    Task<int> MeasureAsync(int speedStep, bool direction, LocomotiveConfiguration locomotiveConfiguration, EndPointConfiguration endPointConfiguration, CancellationToken ct);
  }

  public class SpeedStepMeasurementController(IZ21Controller z21Controller, ILocomotiveSpeedSensor speedSensor, IDelayProvider delayProvider) : ISpeedStepMeasurementController
  {
    public async Task<int> MeasureAsync(int speedStep, bool direction, LocomotiveConfiguration locomotiveConfiguration, EndPointConfiguration endPointConfiguration, CancellationToken ct)
    {
      await speedSensor.ResetMeasurementAsync(endPointConfiguration.SpeedSensorIpAddress, ct);

      var result = await speedSensor.TryGetResultAsync(endPointConfiguration.SpeedSensorIpAddress, ct);
      var startId = result.Id;

      await z21Controller.SetSpeedAsync(locomotiveConfiguration.DccSpeedMode, (ushort)locomotiveConfiguration.DecoderAddress, (ushort)speedStep, direction);
      var delay = delayProvider.ComputeDelayLinear(speedStep, 1, (int)locomotiveConfiguration.DccSpeedMode, 0.5, 2);

      while (result.Id == startId)
      {
        await Task.Delay(TimeSpan.FromMilliseconds(250), ct);
        ct.ThrowIfCancellationRequested();
        result = await speedSensor.TryGetResultAsync(endPointConfiguration.SpeedSensorIpAddress, ct);
      }

      await Task.Delay(TimeSpan.FromSeconds(delay), ct); // wait for the locomotive to get away from the speed sensor.

      await z21Controller.SetSpeedAsync(locomotiveConfiguration.DccSpeedMode, (ushort)locomotiveConfiguration.DecoderAddress, 0, direction);

      return (int)Math.Round(result.SpeedKmh * locomotiveConfiguration.LocomotiveScale, 0);
    }
  }
}