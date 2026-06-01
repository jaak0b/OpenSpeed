namespace OpenSpeed.Core.Models
{
  public sealed class SpeedStepMeasurement : IEquatable<SpeedStepMeasurement>
  {
    public required int SpeedStep { get; init; }

    public int? ForwardPass { get; set; }

    public int? BackwardPass { get; set; }

    public bool Equals(SpeedStepMeasurement? other) => SpeedStep == other?.SpeedStep;

    override public bool Equals(object? obj) => obj is SpeedStepMeasurement other && Equals(other);

    override public int GetHashCode() => HashCode.Combine(SpeedStep);
  }
}