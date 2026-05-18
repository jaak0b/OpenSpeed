namespace OpenSpeed.Core.Provider
{
  public interface IDelayProvider
  {
    double ComputeDelayLinear(double input, double minInput, double maxInput, double minDelay, double maxDelay);
  }

  public class DelayProvider : IDelayProvider
  {
    public double ComputeDelayLinear(double input, double minInput, double maxInput, double minDelay, double maxDelay)
    {
      var t = (input - minInput) / (maxInput - minInput);
      return (maxDelay - t * (maxDelay - minDelay));
    }
  }
}