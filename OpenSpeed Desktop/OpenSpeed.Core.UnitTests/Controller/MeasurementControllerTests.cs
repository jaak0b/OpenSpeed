using FakeItEasy;
using OpenSpeed.Core.Controller;
using OpenSpeed.Core.EventArgs;
using OpenSpeed.Core.Models;
using OpenSpeed.Core.Models.Configuration;
using OpenSpeed.Core.Provider;

namespace OpenSpeed.Core.UnitTests.Controller;

[TestFixture]
public class MeasurementControllerTests
{
    private ISpeedStepMeasurementProvider _provider = null!;
    private ISpeedStepMeasurementController _stepCtrl = null!;
    private MeasurementController _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _provider = A.Fake<ISpeedStepMeasurementProvider>();
        _stepCtrl = A.Fake<ISpeedStepMeasurementController>();
        _sut = new MeasurementController(_provider, _stepCtrl);
    }

    private static LocomotiveConfiguration MakeLocoConfig(DccSpeedMode mode = DccSpeedMode.Steps128)
    {
        var store = new InMemoryKeyValueStore()
            .Set(nameof(LocomotiveConfiguration.DccSpeedMode), mode)
            .Set(nameof(LocomotiveConfiguration.DecoderAddress), 3)
            .Set(nameof(LocomotiveConfiguration.LocomotiveScale), 87);
        return new LocomotiveConfiguration(store);
    }

    private static EndPointConfiguration MakeEndpointConfig()
    {
        var store = new InMemoryKeyValueStore()
            .Set(nameof(EndPointConfiguration.SpeedSensorIpAddress), "http://192.168.1.1")
            .Set(nameof(EndPointConfiguration.Z21IpAddress), "192.168.1.2");
        return new EndPointConfiguration(store);
    }

    private static MeasurementConfiguration MakeMeasurementConfig(int maxSpeed = 0)
    {
        var store = new InMemoryKeyValueStore()
            .Set(nameof(MeasurementConfiguration.MaxSpeed), maxSpeed)
            .Set(nameof(MeasurementConfiguration.StartingSpeedStep), 1)
            .Set(nameof(MeasurementConfiguration.SpeedStepInterval), 1);
        return new MeasurementConfiguration(store);
    }

    private static List<SpeedStepMeasurement> MakeSteps(params int[] steps) =>
        steps.Select(s => new SpeedStepMeasurement { SpeedStep = s }).ToList();

    [Test]
    public async Task StartMeasurementAsync_TwoStepsProvided_MeasuresForwardAndBackwardForEachStep()
    {
        A.CallTo(() => _provider.Provide(A<MeasurementConfiguration>._, A<DccSpeedMode>._))
            .Returns(MakeSteps(1, 2));
        A.CallTo(() => _stepCtrl.MeasureAsync(A<int>._, A<bool>._, A<LocomotiveConfiguration>._, A<EndPointConfiguration>._, A<CancellationToken>._))
            .Returns(Task.FromResult(10));

        await _sut.StartMeasurementAsync(MakeLocoConfig(), MakeEndpointConfig(), MakeMeasurementConfig());

        A.CallTo(() => _stepCtrl.MeasureAsync(A<int>._, A<bool>._, A<LocomotiveConfiguration>._, A<EndPointConfiguration>._, A<CancellationToken>._))
            .MustHaveHappened(4, Times.Exactly);
    }

    [Test]
    public async Task StartMeasurementAsync_TwoStepsProvided_RaisesOnSpeedStepMeasuredTwice()
    {
        A.CallTo(() => _provider.Provide(A<MeasurementConfiguration>._, A<DccSpeedMode>._))
            .Returns(MakeSteps(5, 10));
        A.CallTo(() => _stepCtrl.MeasureAsync(A<int>._, A<bool>._, A<LocomotiveConfiguration>._, A<EndPointConfiguration>._, A<CancellationToken>._))
            .Returns(Task.FromResult(50));

        var raised = new List<SpeedStepMeasurementEventArgs>();
        _sut.OnSpeedStepMeasured += (_, e) => raised.Add(e);

        await _sut.StartMeasurementAsync(MakeLocoConfig(), MakeEndpointConfig(), MakeMeasurementConfig());

        Assert.That(raised, Has.Count.EqualTo(2));
        Assert.That(raised[0].Measurement.SpeedStep, Is.EqualTo(5));
        Assert.That(raised[1].Measurement.SpeedStep, Is.EqualTo(10));
    }

    [Test]
    public void StartMeasurementAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        A.CallTo(() => _provider.Provide(A<MeasurementConfiguration>._, A<DccSpeedMode>._))
            .Returns(MakeSteps(1, 2, 3));

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAsync<OperationCanceledException>(() =>
            _sut.StartMeasurementAsync(MakeLocoConfig(), MakeEndpointConfig(), MakeMeasurementConfig(), cts.Token));

        A.CallTo(() => _stepCtrl.MeasureAsync(A<int>._, A<bool>._, A<LocomotiveConfiguration>._, A<EndPointConfiguration>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task StartMeasurementAsync_MaxSpeedIsZero_MeasuresAllSteps()
    {
        A.CallTo(() => _provider.Provide(A<MeasurementConfiguration>._, A<DccSpeedMode>._))
            .Returns(MakeSteps(1, 2, 3));
        A.CallTo(() => _stepCtrl.MeasureAsync(A<int>._, A<bool>._, A<LocomotiveConfiguration>._, A<EndPointConfiguration>._, A<CancellationToken>._))
            .Returns(Task.FromResult(999));

        await _sut.StartMeasurementAsync(MakeLocoConfig(), MakeEndpointConfig(), MakeMeasurementConfig(maxSpeed: 0));

        A.CallTo(() => _stepCtrl.MeasureAsync(A<int>._, A<bool>._, A<LocomotiveConfiguration>._, A<EndPointConfiguration>._, A<CancellationToken>._))
            .MustHaveHappened(6, Times.Exactly);
    }

    [Test]
    public async Task StartMeasurementAsync_AverageExceedsMaxSpeed_StopsAfterFirstStep()
    {
        A.CallTo(() => _provider.Provide(A<MeasurementConfiguration>._, A<DccSpeedMode>._))
            .Returns(MakeSteps(1, 2, 3));
        A.CallTo(() => _stepCtrl.MeasureAsync(A<int>._, A<bool>._, A<LocomotiveConfiguration>._, A<EndPointConfiguration>._, A<CancellationToken>._))
            .Returns(Task.FromResult(60));

        await _sut.StartMeasurementAsync(MakeLocoConfig(), MakeEndpointConfig(), MakeMeasurementConfig(maxSpeed: 50));

        A.CallTo(() => _stepCtrl.MeasureAsync(A<int>._, A<bool>._, A<LocomotiveConfiguration>._, A<EndPointConfiguration>._, A<CancellationToken>._))
            .MustHaveHappened(2, Times.Exactly);
    }

    [Test]
    public async Task StartMeasurementAsync_MeasuredValues_AssignedToCorrectPassFields()
    {
        A.CallTo(() => _provider.Provide(A<MeasurementConfiguration>._, A<DccSpeedMode>._))
            .Returns(MakeSteps(1));
        A.CallTo(() => _stepCtrl.MeasureAsync(A<int>._, true, A<LocomotiveConfiguration>._, A<EndPointConfiguration>._, A<CancellationToken>._))
            .Returns(Task.FromResult(111));
        A.CallTo(() => _stepCtrl.MeasureAsync(A<int>._, false, A<LocomotiveConfiguration>._, A<EndPointConfiguration>._, A<CancellationToken>._))
            .Returns(Task.FromResult(222));

        SpeedStepMeasurementEventArgs? raised = null;
        _sut.OnSpeedStepMeasured += (_, e) => raised = e;

        await _sut.StartMeasurementAsync(MakeLocoConfig(), MakeEndpointConfig(), MakeMeasurementConfig());

        Assert.That(raised!.Measurement.ForwardPass, Is.EqualTo(111));
        Assert.That(raised.Measurement.BackwardPass, Is.EqualTo(222));
    }
}
