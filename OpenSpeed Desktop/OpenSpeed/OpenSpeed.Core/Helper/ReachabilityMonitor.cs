using System.Net;
using System.Net.NetworkInformation;
using Timer = System.Timers.Timer;

namespace OpenSpeed.Core.Helper
{
  public class ReachabilityMonitor
  {
    private readonly Timer _timer;
    private bool? _lastReachable;

    public ReachabilityMonitor(Func<string> getAddress, Action<bool> onAvailabilityChanging)
    {
      _timer = new(TimeSpan.FromSeconds(1))
               {
                 AutoReset = true,
                 Enabled = true
               };
      _timer.Elapsed += (_, _) => CheckState(getAddress, onAvailabilityChanging);
    }

    private void CheckState(Func<string> getAddress, Action<bool> onAvailabilityChanging)
    {
      var reachable = IsReachable(getAddress());

      if (_lastReachable == reachable)
        return;

      _lastReachable = reachable;
      onAvailabilityChanging(reachable);
    }

    private static bool IsReachable(string address)
    {
      try
      {
        using var ping = new Ping();
        var reply = ping.Send(IPAddress.Parse(address), 1000);
        return reply.Status == IPStatus.Success;
      }
      catch
      {
        return false;
      }
    }
  }
}