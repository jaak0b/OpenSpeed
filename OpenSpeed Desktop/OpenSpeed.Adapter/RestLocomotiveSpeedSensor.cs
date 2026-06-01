using System.Text.Json;
using OpenSpeed.Core.Interfaces;
using OpenSpeed.Core.Models;

namespace OpenSpeed.Adapter
{
  public sealed class RestLocomotiveSpeedSensor(HttpClient httpClient) : ILocomotiveSpeedSensor
  {
    public async Task<MeasurementStatus> GetStatusAsync(string url, CancellationToken ct = default)
    {
      var json = await httpClient.GetStringAsync($"{CreateUri(url)}status", ct);
      var dto = JsonSerializer.Deserialize<StatusDto>(json)!;
      return dto.Status;
    }

    public async Task<SpeedMeasurementDto> TryGetResultAsync(string url, CancellationToken ct = default)
    {
      var json = await httpClient.GetStringAsync($"{CreateUri(url)}result", ct);
      return JsonSerializer.Deserialize<SpeedMeasurementDto>(json)!;
    }

    public async Task ResetMeasurementAsync(string url, CancellationToken ct = default)
    {
      await httpClient.GetStringAsync($"{CreateUri(url)}reset", ct);
    }

    private Uri CreateUri(string url) => new UriBuilder(url).Uri;
  }
}