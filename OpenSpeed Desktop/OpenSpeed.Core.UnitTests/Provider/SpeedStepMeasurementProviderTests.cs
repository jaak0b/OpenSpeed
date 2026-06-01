using OpenSpeed.Core.Models;
using OpenSpeed.Core.Models.Configuration;
using OpenSpeed.Core.Provider;

namespace OpenSpeed.Core.UnitTests.Provider;

[TestFixture]
public class SpeedStepMeasurementProviderTests
{
    private SpeedStepMeasurementProvider _sut = null!;

    [SetUp]
    public void SetUp() => _sut = new SpeedStepMeasurementProvider();

    private static MeasurementConfiguration MakeConfig(int startingStep, int interval, int maxSpeed = 0)
    {
        var store = new InMemoryKeyValueStore()
            .Set(nameof(MeasurementConfiguration.StartingSpeedStep), startingStep)
            .Set(nameof(MeasurementConfiguration.SpeedStepInterval), interval)
            .Set(nameof(MeasurementConfiguration.MaxSpeed), maxSpeed);
        return new MeasurementConfiguration(store);
    }

    [Test]
    public void Provide_StartingSpeedStepIsZero_ThrowsArgumentException()
    {
        var config = MakeConfig(startingStep: 0, interval: 1);
        Assert.Throws<ArgumentException>(() => _sut.Provide(config, DccSpeedMode.Steps14).ToList());
    }

    [Test]
    public void Provide_StartingSpeedStepIsNegative_ThrowsArgumentException()
    {
        var config = MakeConfig(startingStep: -1, interval: 1);
        Assert.Throws<ArgumentException>(() => _sut.Provide(config, DccSpeedMode.Steps14).ToList());
    }

    [Test]
    public void Provide_StartingSpeedStepEqualsOrExceedsModeMax_ThrowsArgumentException()
    {
        var config = MakeConfig(startingStep: 14, interval: 1);
        Assert.Throws<ArgumentException>(() => _sut.Provide(config, DccSpeedMode.Steps14).ToList());
    }

    [Test]
    public void Provide_SpeedStepIntervalIsZero_ThrowsArgumentException()
    {
        var config = MakeConfig(startingStep: 1, interval: 0);
        Assert.Throws<ArgumentException>(() => _sut.Provide(config, DccSpeedMode.Steps14).ToList());
    }

    [Test]
    public void Provide_SpeedStepIntervalIsNegative_ThrowsArgumentException()
    {
        var config = MakeConfig(startingStep: 1, interval: -1);
        Assert.Throws<ArgumentException>(() => _sut.Provide(config, DccSpeedMode.Steps14).ToList());
    }

    [Test]
    public void Provide_ValidSteps14Config_YieldsStepsOneToFourteen()
    {
        var config = MakeConfig(startingStep: 1, interval: 1);
        var steps = _sut.Provide(config, DccSpeedMode.Steps14).Select(s => s.SpeedStep).ToList();

        Assert.That(steps, Has.Count.EqualTo(14));
        Assert.That(steps.First(), Is.EqualTo(1));
        Assert.That(steps.Last(), Is.EqualTo(14));
    }

    [Test]
    public void Provide_ValidConfigWithInterval_SkipsStepsCorrectly()
    {
        var config = MakeConfig(startingStep: 2, interval: 3);
        var steps = _sut.Provide(config, DccSpeedMode.Steps28).Select(s => s.SpeedStep).ToList();

        Assert.That(steps, Is.EqualTo(new[] { 2, 5, 8, 11, 14, 17, 20, 23, 26 }));
    }

    [Test]
    public void Provide_Steps128Config_FirstAndLastStepCorrect()
    {
        var config = MakeConfig(startingStep: 1, interval: 1);
        var steps = _sut.Provide(config, DccSpeedMode.Steps128).Select(s => s.SpeedStep).ToList();

        Assert.That(steps.First(), Is.EqualTo(1));
        Assert.That(steps.Last(), Is.EqualTo(128));
    }
}
