using FakeItEasy;
using OpenSpeed.Core.Controller;
using OpenSpeed.Core.Interfaces;
using OpenSpeed.Core.Models;
using OpenSpeed.Core.Models.Configuration;
using OpenSpeed.Core.Provider;

namespace OpenSpeed.Core.UnitTests.Controller;

[TestFixture]
public class SpeedStepMeasurementControllerTests
{
    private IZ21Controller _z21 = null!;
    private ILocomotiveSpeedSensor _sensor = null!;
    private IDelayProvider _delay = null!;
    private SpeedStepMeasurementController _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _z21 = A.Fake<IZ21Controller>();
        _sensor = A.Fake<ILocomotiveSpeedSensor>();
        _delay = A.Fake<IDelayProvider>();
        _sut = new SpeedStepMeasurementController(_z21, _sensor, _delay);

        A.CallTo(() => _delay.ComputeDelayLinear(A<double>._, A<double>._, A<double>._, A<double>._, A<double>._))
            .Returns(0.0);
    }

    private static LocomotiveConfiguration MakeLocoConfig(int scale = 87, int address = 3, DccSpeedMode mode = DccSpeedMode.Steps128)
    {
        var store = new InMemoryKeyValueStore()
            .Set(nameof(LocomotiveConfiguration.LocomotiveScale), scale)
            .Set(nameof(LocomotiveConfiguration.DecoderAddress), address)
            .Set(nameof(LocomotiveConfiguration.DccSpeedMode), mode);
        return new LocomotiveConfiguration(store);
    }

    private static EndPointConfiguration MakeEndpointConfig(string url = "http://192.168.1.1")
    {
        var store = new InMemoryKeyValueStore()
            .Set(nameof(EndPointConfiguration.SpeedSensorIpAddress), url)
            .Set(nameof(EndPointConfiguration.Z21IpAddress), "192.168.1.2");
        return new EndPointConfiguration(store);
    }

    private void SetupPollSequence(long baselineId, long resultId, double speedKmh = 10.0)
    {
        A.CallTo(() => _sensor.TryGetResultAsync(A<string>._, A<CancellationToken>._))
            .ReturnsNextFromSequence(
                new SpeedMeasurementDto { Id = baselineId },
                new SpeedMeasurementDto { Id = baselineId },
                new SpeedMeasurementDto { Id = resultId, SpeedKmh = speedKmh });
    }

    [Test]
    public async Task MeasureAsync_ValidInputs_CallsResetMeasurementBeforeBaseline()
    {
        SetupPollSequence(baselineId: 10, resultId: 11);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        await _sut.MeasureAsync(1, true, MakeLocoConfig(), MakeEndpointConfig(), cts.Token);

        A.CallTo(() => _sensor.ResetMeasurementAsync(A<string>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task MeasureAsync_ValidInputs_CallsSetSpeedWithCorrectSpeedStepAndDirection()
    {
        SetupPollSequence(baselineId: 10, resultId: 11);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        await _sut.MeasureAsync(42, true, MakeLocoConfig(address: 3), MakeEndpointConfig(), cts.Token);

        A.CallTo(() => _z21.SetSpeedAsync(DccSpeedMode.Steps128, 3, 42, true))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task MeasureAsync_ResultIdChanges_ReturnsSpeedMultipliedByLocomotiveScale()
    {
        SetupPollSequence(baselineId: 10, resultId: 11, speedKmh: 10.0);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var result = await _sut.MeasureAsync(1, true, MakeLocoConfig(scale: 87), MakeEndpointConfig(), cts.Token);

        Assert.That(result, Is.EqualTo(870));
    }

    [Test]
    public async Task MeasureAsync_AfterLoopBreaks_StopsLocoWithSpeedStepZero()
    {
        SetupPollSequence(baselineId: 10, resultId: 11);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        await _sut.MeasureAsync(42, true, MakeLocoConfig(address: 3), MakeEndpointConfig(), cts.Token);

        A.CallTo(() => _z21.SetSpeedAsync(DccSpeedMode.Steps128, 3, 0, true))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void MeasureAsync_CancellationDuringPoll_ThrowsOperationCanceledException()
    {
        A.CallTo(() => _sensor.TryGetResultAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult(new SpeedMeasurementDto { Id = 99 }));

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        Assert.ThrowsAsync<TaskCanceledException>(() =>
            _sut.MeasureAsync(1, true, MakeLocoConfig(), MakeEndpointConfig(), cts.Token));
    }

    [Test]
    public async Task MeasureAsync_WithOnePollIteration_CallsTryGetResultThreeTimes()
    {
        SetupPollSequence(baselineId: 10, resultId: 11);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        await _sut.MeasureAsync(1, true, MakeLocoConfig(), MakeEndpointConfig(), cts.Token);

        A.CallTo(() => _sensor.TryGetResultAsync(A<string>._, A<CancellationToken>._))
            .MustHaveHappened(3, Times.Exactly);
    }
}
