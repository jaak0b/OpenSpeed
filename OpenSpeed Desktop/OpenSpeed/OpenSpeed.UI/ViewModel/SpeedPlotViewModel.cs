using OpenSpeed.Core.Controller;
using OpenSpeed.Core.EventArgs;
using OxyPlot;
using OxyPlot.Series;

namespace OpenSpeed.UI.ViewModel
{
  public class SpeedPlotViewModel
  {
    public PlotModel Plot { get; } = new() { Title = "Speeds" };

    public LineSeries DirectionForwards { get; } = new()
                                                   {
                                                     Title = "Speed Forwards",
                                                     MarkerType = MarkerType.Circle,
                                                     MarkerSize = 3
                                                   };

    public LineSeries DirectionBackwards { get; } = new()
                                                    {
                                                      Title = "Speed Backwards",
                                                      MarkerType = MarkerType.Circle,
                                                      MarkerSize = 3
                                                    };

    public SpeedPlotViewModel(IMeasurementController measurementController)
    {
      Plot.Series.Add(DirectionForwards);
      Plot.Series.Add(DirectionBackwards);
      measurementController.OnSpeedStepMeasured += MeasurementController_OnOnSpeedStepMeasured;
    }

    private void MeasurementController_OnOnSpeedStepMeasured(object? sender, SpeedStepMeasurementEventArgs args)
    {
      if (args.Measurement.ForwardPass is { } forwardSpeed)
        DirectionForwards.Points.Add(new(args.Measurement.SpeedStep, forwardSpeed));

      if (args.Measurement.BackwardPass is { } backwardSpeed)
        DirectionBackwards.Points.Add(new(args.Measurement.SpeedStep, backwardSpeed));

      Plot.InvalidatePlot(true);
    }

    public void ClearPoints()
    {
      DirectionForwards.Points.Clear();
      DirectionBackwards.Points.Clear();
    }
  }
}