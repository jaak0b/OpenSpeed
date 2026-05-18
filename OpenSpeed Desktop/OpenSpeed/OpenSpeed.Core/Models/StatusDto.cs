using System.Text.Json.Serialization;

namespace OpenSpeed.Core.Models
{
  public sealed class StatusDto
  {
    [JsonPropertyName("status")]
    public MeasurementStatus Status { get; init; }
  }
}