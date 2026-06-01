using FakeItEasy;
using OpenSpeed.Core.Controller;
using OpenSpeed.Core.EventArgs;
using OpenSpeed.Core.Models;
using OpenSpeed.UI.Localization;
using OpenSpeed.UI.Theming;
using OpenSpeed.UI.ViewModel;
using OxyPlot;

namespace OpenSpeed.UI.UnitTests.ViewModel;

[TestFixture]
public class SpeedPlotViewModelTests
{
    private IMeasurementController _controller = null!;
    private SpeedPlotViewModel _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _controller = A.Fake<IMeasurementController>();
        _sut = new SpeedPlotViewModel(_controller, LocalizationManager.Instance, ThemeManager.Instance);
    }

    private void RaiseEvent(int speedStep, int? forwardPass, int? backwardPass)
    {
        var measurement = new SpeedStepMeasurement
        {
            SpeedStep = speedStep,
            ForwardPass = forwardPass,
            BackwardPass = backwardPass
        };
        _controller.OnSpeedStepMeasured += Raise.With(new SpeedStepMeasurementEventArgs(measurement));
    }

    [Test]
    public void ClearPoints_WithExistingDataPoints_ClearsForwardSeries()
    {
        _sut.DirectionForwards.Points.Add(new DataPoint(1, 100));
        _sut.DirectionForwards.Points.Add(new DataPoint(2, 200));

        _sut.ClearPoints();

        Assert.That(_sut.DirectionForwards.Points, Is.Empty);
    }

    [Test]
    public void ClearPoints_WithExistingDataPoints_ClearsBackwardSeries()
    {
        _sut.DirectionBackwards.Points.Add(new DataPoint(1, 80));

        _sut.ClearPoints();

        Assert.That(_sut.DirectionBackwards.Points, Is.Empty);
    }

    [Test]
    public void ClearPoints_AfterHighValuePoint_ResetsYAxisMaximumToTwoHundred()
    {
        RaiseEvent(speedStep: 1, forwardPass: 500, backwardPass: null);

        _sut.ClearPoints();

        RaiseEvent(speedStep: 2, forwardPass: 50, backwardPass: null);
        Assert.That(_sut.Plot.Axes.First(a => a.Position == OxyPlot.Axes.AxisPosition.Left).Maximum,
            Is.EqualTo(200).Within(1));
    }

    [Test]
    public void OnSpeedStepMeasured_ForwardPassPresent_AddsPointToForwardSeries()
    {
        RaiseEvent(speedStep: 5, forwardPass: 80, backwardPass: null);

        Assert.That(_sut.DirectionForwards.Points, Has.Count.EqualTo(1));
        Assert.That(_sut.DirectionForwards.Points[0].X, Is.EqualTo(5));
        Assert.That(_sut.DirectionForwards.Points[0].Y, Is.EqualTo(80));
        Assert.That(_sut.DirectionBackwards.Points, Is.Empty);
    }

    [Test]
    public void OnSpeedStepMeasured_BackwardPassPresent_AddsPointToBackwardSeries()
    {
        RaiseEvent(speedStep: 10, forwardPass: null, backwardPass: 60);

        Assert.That(_sut.DirectionBackwards.Points, Has.Count.EqualTo(1));
        Assert.That(_sut.DirectionBackwards.Points[0].X, Is.EqualTo(10));
        Assert.That(_sut.DirectionBackwards.Points[0].Y, Is.EqualTo(60));
        Assert.That(_sut.DirectionForwards.Points, Is.Empty);
    }

    [Test]
    public void OnSpeedStepMeasured_MultipleEvents_AccumulatesPointsInOrder()
    {
        RaiseEvent(speedStep: 1, forwardPass: 10, backwardPass: null);
        RaiseEvent(speedStep: 2, forwardPass: 20, backwardPass: null);
        RaiseEvent(speedStep: 3, forwardPass: 30, backwardPass: null);

        Assert.That(_sut.DirectionForwards.Points, Has.Count.EqualTo(3));
        Assert.That(_sut.DirectionForwards.Points.Select(p => p.X), Is.EqualTo(new[] { 1.0, 2.0, 3.0 }));
    }
}
