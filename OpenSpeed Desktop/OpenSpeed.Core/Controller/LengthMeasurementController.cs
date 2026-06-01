using OpenSpeed.Core.Interfaces;
using OpenSpeed.Core.Models.Configuration;
using OpenSpeed.Core.Provider;

namespace OpenSpeed.Core.Controller
{
  public interface ILengthMeasurementController
  {
    Task<double> MeasureAsync(
      LocomotiveConfiguration locomotiveConfiguration,
      EndPointConfiguration endPointConfiguration,
      LengthMeasurementConfiguration lengthMeasurementConfiguration,
      CancellationToken ct);
  }

  public class LengthMeasurementController(
    IZ21Controller z21Controller,
    ILocomotiveSpeedSensor speedSensor,
    IDelayProvider delayProvider) : ILengthMeasurementController
  {
    public async Task<double> MeasureAsync(
      LocomotiveConfiguration locomotiveConfiguration,
      EndPointConfiguration endPointConfiguration,
      LengthMeasurementConfiguration lengthMeasurementConfiguration,
      CancellationToken ct)
    {
      var url = endPointConfiguration.SpeedSensorIpAddress;
      var mode = locomotiveConfiguration.DccSpeedMode;
      var addr = (ushort)locomotiveConfiguration.DecoderAddress;
      var step = (ushort)lengthMeasurementConfiguration.SpeedStep;
      var postPassDelay = delayProvider.ComputeDelayLinear(step, 1, (int)mode, 0.5, 2);

      // Forward pass
      await speedSensor.ResetMeasurementAsync(url, ct);
      var fwd = await speedSensor.TryGetResultAsync(url, ct);
      var startId = fwd.Id;

      await z21Controller.SetSpeedAsync(mode, addr, step, direction: true);

      while (fwd.Id == startId)
      {
        await Task.Delay(TimeSpan.FromMilliseconds(250), ct);
        ct.ThrowIfCancellationRequested();
        fwd = await speedSensor.TryGetResultAsync(url, ct);
      }

      await Task.Delay(TimeSpan.FromSeconds(postPassDelay), ct);
      await z21Controller.SetSpeedAsync(mode, addr, 0, direction: true);
      await Task.Delay(TimeSpan.FromSeconds(2), ct); // allow deceleration before reversing

      // Backward pass
      await speedSensor.ResetMeasurementAsync(url, ct);
      var bwd = await speedSensor.TryGetResultAsync(url, ct);
      startId = bwd.Id;

      await z21Controller.SetSpeedAsync(mode, addr, step, direction: false);

      while (bwd.Id == startId)
      {
        await Task.Delay(TimeSpan.FromMilliseconds(250), ct);
        ct.ThrowIfCancellationRequested();
        bwd = await speedSensor.TryGetResultAsync(url, ct);
      }

      await Task.Delay(TimeSpan.FromSeconds(postPassDelay), ct);
      await z21Controller.SetSpeedAsync(mode, addr, 0, direction: false);

      return (fwd.TrainLengthCm + bwd.TrainLengthCm) / 2.0;
    }
  }
}
