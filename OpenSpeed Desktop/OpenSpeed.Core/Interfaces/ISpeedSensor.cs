using OpenSpeed.Core.Models;

namespace OpenSpeed.Core.Interfaces
{
  public interface ILocomotiveSpeedSensor
  {
    Task<MeasurementStatus> GetStatusAsync(string url, CancellationToken ct = default);

    Task<SpeedMeasurementDto> TryGetResultAsync(string url, CancellationToken ct = default);

    Task ResetMeasurementAsync(string url, CancellationToken ct = default);
  }
}