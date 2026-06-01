using System.Text.Json.Serialization;

namespace OpenSpeed.Core.Models
{

  public sealed class SpeedMeasurementDto
  {
    [JsonPropertyName("id")]
    public long Id { get; init; }
    
    [JsonPropertyName("speed_kmh")]
    public double SpeedKmh { get; init; }
    
    [JsonPropertyName("train_length_cm")]
    public double TrainLengthCm { get; init; }
  }
}