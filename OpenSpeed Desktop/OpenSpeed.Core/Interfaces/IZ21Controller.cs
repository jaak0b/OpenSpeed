namespace OpenSpeed.Core.Interfaces
{
  public interface IZ21Controller
  {
    Task SetSpeedAsync(Models.DccSpeedMode dccSpeedMode, ushort decoderAddress, ushort speedStep, bool direction);

    void Configure(string ipAddress);
  }
}