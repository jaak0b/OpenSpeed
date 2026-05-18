using OpenSpeed.Core.Models;

namespace OpenSpeed.Core.EventArgs
{
  public class SpeedStepMeasurementEventArgs(SpeedStepMeasurement measurement) : System.EventArgs
  {
    public SpeedStepMeasurement Measurement { get; } = measurement;
  }
}