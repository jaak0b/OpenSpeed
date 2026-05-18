using OpenSpeed.Core.Provider;

namespace OpenSpeed.Core.UnitTests.Provider
{

  [TestFixture]
  public class DelayProviderTest
  {
    private const int MinInput = 1;
    private const int MaxInput = 126;
    private const int MinDelay = 1;
    private const int MaxDelay = 5;

    [Test]
    public void Input_MinInput_Returns_MaxDelay()
    {
      var delay = new DelayProvider().ComputeDelayLinear(MinInput, MinInput, MaxInput, MinDelay, MaxDelay);
      Assert.That(delay, Is.EqualTo(MaxDelay));
    }

    [Test]
    public void Input_MaxInput_Returns_MinDelay()
    {
      var delay = new DelayProvider().ComputeDelayLinear(MaxInput, MinInput, MaxInput, MinDelay, MaxDelay);
      Assert.That(delay, Is.EqualTo(MinDelay));
    }

    [Test]
    public void Input_MiddleValue_Returns_ApproxHalfDelay()
    {
      var mid = (MinInput + MaxInput) / 2; // 63
      var delay = new DelayProvider().ComputeDelayLinear(mid, MinInput, MaxInput, MinDelay, MaxDelay);

      Assert.That(delay, Is.EqualTo(3.016d));
    }

    [Test]
    public void Input_QuarterValue_Returns_ExpectedDelay()
    {
      var quarter = MinInput + (MaxInput - MinInput) / 4; // ~32
      var delay = new DelayProvider().ComputeDelayLinear(quarter, MinInput, MaxInput, MinDelay, MaxDelay);

      Assert.That(delay, Is.EqualTo(4.008d));
    }

    [Test]
    public void Input_ThreeQuarterValue_Returns_ExpectedDelay()
    {
      var threeQuarter = MinInput + 3 * (MaxInput - MinInput) / 4; // ~95
      var delay = new DelayProvider().ComputeDelayLinear(threeQuarter, MinInput, MaxInput, MinDelay, MaxDelay);

      Assert.That(delay, Is.EqualTo(2.024d));
    }

    [Test]
    public void Input_SequentialValues_ShouldAlwaysDecrease()
    {
      var previous = new DelayProvider().ComputeDelayLinear(1, MinInput, MaxInput, MinDelay, MaxDelay);

      for (var i = 2; i <= MaxInput; i++)
      {
        var current = new DelayProvider().ComputeDelayLinear(i, MinInput, MaxInput, MinDelay, MaxDelay);
        Assert.That(current, Is.LessThanOrEqualTo(previous));
        previous = current;
      }
    }
  }

}