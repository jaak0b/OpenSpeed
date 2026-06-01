using FakeItEasy;
using OpenSpeed.Core.Controller;
using OpenSpeed.Core.Interfaces;
using OpenSpeed.Core.Models;
using OpenSpeed.Core.Models.Configuration;
using OpenSpeed.Core.Provider;

namespace OpenSpeed.Core.UnitTests.Controller;

[TestFixture]
[Timeout(30000)]
public class LengthMeasurementControllerTests
{
    private IZ21Controller _z21 = null!;
    private ILocomotiveSpeedSensor _sensor = null!;
    private IDelayProvider _delay = null!;
    private LengthMeasurementController _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _z21 = A.Fake<IZ21Controller>();
        _sensor = A.Fake<ILocomotiveSpeedSensor>();
        _delay = A.Fake<IDelayProvider>();
        _sut = new LengthMeasurementController(_z21, _sensor, _delay);

        A.CallTo(() => _delay.ComputeDelayLinear(A<double>._, A<double>._, A<double>._, A<double>._, A<double>._))
            .Returns(0.0);
    }

    private static LocomotiveConfiguration MakeLocoConfig(int address = 3, DccSpeedMode mode = DccSpeedMode.Steps128)
    {
        var store = new InMemoryKeyValueStore()
            .Set(nameof(LocomotiveConfiguration.LocomotiveScale), 87)
            .Set(nameof(LocomotiveConfiguration.DecoderAddress), address)
            .Set(nameof(LocomotiveConfiguration.DccSpeedMode), mode);
        return new LocomotiveConfiguration(store);
    }

    private static EndPointConfiguration MakeEndpointConfig()
    {
        var store = new InMemoryKeyValueStore()
            .Set(nameof(EndPointConfiguration.SpeedSensorIpAddress), "http://192.168.1.1")
            .Set(nameof(EndPointConfiguration.Z21IpAddress), "192.168.1.2");
        return new EndPointConfiguration(store);
    }

    private static LengthMeasurementConfiguration MakeLengthConfig(int speedStep = 10)
    {
        var store = new InMemoryKeyValueStore()
            .Set(nameof(LengthMeasurementConfiguration.SpeedStep), speedStep);
        return new LengthMeasurementConfiguration(store);
    }

    private void SetupTwoPassSequence(double fwdLength = 30.0, double bwdLength = 32.0)
    {
        A.CallTo(() => _sensor.TryGetResultAsync(A<string>._, A<CancellationToken>._))
            .ReturnsNextFromSequence(
                new SpeedMeasurementDto { Id = 1 },
                new SpeedMeasurementDto { Id = 2, TrainLengthCm = fwdLength },
                new SpeedMeasurementDto { Id = 3 },
                new SpeedMeasurementDto { Id = 4, TrainLengthCm = bwdLength });
    }

    [Test]
    public async Task MeasureAsync_BothPassesComplete_ReturnsAverageOfBothLengths()
    {
        SetupTwoPassSequence(fwdLength: 30.0, bwdLength: 32.0);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        var result = await _sut.MeasureAsync(MakeLocoConfig(), MakeEndpointConfig(), MakeLengthConfig(), cts.Token);

        Assert.That(result, Is.EqualTo(31.0));
    }

    [Test]
    public async Task MeasureAsync_TwoPasses_CallsSetSpeedFourTimes()
    {
        SetupTwoPassSequence();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        await _sut.MeasureAsync(MakeLocoConfig(), MakeEndpointConfig(), MakeLengthConfig(), cts.Token);

        A.CallTo(() => _z21.SetSpeedAsync(A<DccSpeedMode>._, A<ushort>._, A<ushort>._, A<bool>._))
            .MustHaveHappened(4, Times.Exactly);
    }

    [Test]
    public async Task MeasureAsync_ForwardPass_UsesDrivingDirectionTrue()
    {
        SetupTwoPassSequence();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        await _sut.MeasureAsync(MakeLocoConfig(address: 3), MakeEndpointConfig(), MakeLengthConfig(speedStep: 10), cts.Token);

        A.CallTo(() => _z21.SetSpeedAsync(A<DccSpeedMode>._, 3, 10, true))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _z21.SetSpeedAsync(A<DccSpeedMode>._, 3, 0, true))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task MeasureAsync_BackwardPass_UsesDrivingDirectionFalse()
    {
        SetupTwoPassSequence();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        await _sut.MeasureAsync(MakeLocoConfig(address: 3), MakeEndpointConfig(), MakeLengthConfig(speedStep: 10), cts.Token);

        A.CallTo(() => _z21.SetSpeedAsync(A<DccSpeedMode>._, 3, 10, false))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _z21.SetSpeedAsync(A<DccSpeedMode>._, 3, 0, false))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void MeasureAsync_CancellationDuringForwardPoll_ThrowsOperationCanceledException()
    {
        A.CallTo(() => _sensor.TryGetResultAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult(new SpeedMeasurementDto { Id = 99 }));

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        Assert.ThrowsAsync<TaskCanceledException>(() =>
            _sut.MeasureAsync(MakeLocoConfig(), MakeEndpointConfig(), MakeLengthConfig(), cts.Token));
    }
}
