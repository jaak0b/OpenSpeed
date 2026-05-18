namespace OpenSpeed.Core.Models
{
  public sealed class MeasurementSession
  {
    public int TrainAddress { get; init; }

    public double ScaleRatio { get; init; } // e.g. 1:87 → 87

    public List<SpeedStepMeasurement> Steps { get; set; } = [];
  }
}