using System.Net;
using OpenSpeed.Core.Interfaces;
using Z21.Core;
using Z21.Core.Command.Driving;
using Z21.Core.Model;

namespace OpenSpeed.Adapter
{
  public class Z21Controller(IZ21Client z21Client, Z21Configuration configuration) : IZ21Controller
  {
    public async Task SetSpeedAsync(OpenSpeed.Core.Models.DccSpeedMode dccSpeedMode, ushort decoderAddress, ushort speedStep, bool direction)
    {
      var drivingDirection = direction ? DrivingDirection.Forward : DrivingDirection.Backward;
      var speedMode = GetZ21DccSpeedMode(dccSpeedMode);

      await z21Client.SendCommandsAsync(new SetLocoDriveCommand(speedMode, decoderAddress, drivingDirection, speedStep));
    }

    public void Configure(string ipAddress)
    {
      var endPoint = Z21Configuration.Defaults.IpEndPoint;
      endPoint.Address = IPAddress.Parse(ipAddress);
      configuration.ClientIPEndPoint = endPoint;
    }

    private DccSpeedMode GetZ21DccSpeedMode(OpenSpeed.Core.Models.DccSpeedMode dccSpeedMode)
      => dccSpeedMode switch
         {
           Core.Models.DccSpeedMode.Steps14 => DccSpeedMode.Steps14,
           Core.Models.DccSpeedMode.Steps28 => DccSpeedMode.Steps28,
           Core.Models.DccSpeedMode.Steps128 => DccSpeedMode.Steps128,
           _ => throw new ArgumentOutOfRangeException(nameof(dccSpeedMode), dccSpeedMode, null)
         };
  }
}