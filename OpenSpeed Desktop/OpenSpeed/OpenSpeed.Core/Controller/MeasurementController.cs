using OpenSpeed.Core.EventArgs;
using OpenSpeed.Core.Models.Configuration;
using OpenSpeed.Core.Provider;

namespace OpenSpeed.Core.Controller
{

  public interface IMeasurementController
  {
    event EventHandler<SpeedStepMeasurementEventArgs>? OnSpeedStepMeasured;

    Task StartMeasurementAsync(LocomotiveConfiguration locomotiveConfiguration, EndPointConfiguration endPointConfiguration, MeasurementConfiguration measurementConfiguration, CancellationToken ct = default);
  }

  public class MeasurementController(ISpeedStepMeasurementProvider measurementProvider, ISpeedStepMeasurementController measurementController) : IMeasurementController
  {
    public event EventHandler<SpeedStepMeasurementEventArgs>? OnSpeedStepMeasured;

    public async Task StartMeasurementAsync(LocomotiveConfiguration locomotiveConfiguration, EndPointConfiguration endPointConfiguration, MeasurementConfiguration measurementConfiguration,
                                            CancellationToken ct = default)
    {
      foreach (var measurement in measurementProvider.Provide(measurementConfiguration, locomotiveConfiguration.DccSpeedMode))
      {
        ct.ThrowIfCancellationRequested();
        measurement.ForwardPass = await measurementController.MeasureAsync(measurement.SpeedStep, true, locomotiveConfiguration, endPointConfiguration, ct);
        measurement.BackwardPass = await measurementController.MeasureAsync(measurement.SpeedStep, false, locomotiveConfiguration, endPointConfiguration, ct);

        OnSpeedStepMeasured?.Invoke(this, new(measurement));

        if (measurementConfiguration.MaxSpeed == 0)
          continue;
        
        var average = (measurement.ForwardPass + measurement.BackwardPass) / 2;
        if (average >= measurementConfiguration.MaxSpeed)
          return;
      }
    }
  }
}