using OpenSpeed.Core.Controller;
using OpenSpeed.Core.EventArgs;
using OpenSpeed.UI.Localization;
using OxyPlot;
using OxyPlot.Series;

namespace OpenSpeed.UI.ViewModel
{
  public class SpeedPlotViewModel
  {
    private readonly LocalizationManager _localization;

    public PlotModel Plot { get; } = new();

    public LineSeries DirectionForwards { get; } = new()
                                                   {
                                                     MarkerType = MarkerType.Circle,
                                                     MarkerSize = 3
                                                   };

    public LineSeries DirectionBackwards { get; } = new()
                                                    {
                                                      MarkerType = MarkerType.Circle,
                                                      MarkerSize = 3
                                                    };

    public SpeedPlotViewModel(IMeasurementController measurementController, LocalizationManager localization)
    {
      _localization = localization;
      ApplyTitles();
      Plot.Series.Add(DirectionForwards);
      Plot.Series.Add(DirectionBackwards);
      measurementController.OnSpeedStepMeasured += MeasurementController_OnOnSpeedStepMeasured;
      _localization.LanguageChanged += (_, _) =>
                                       {
                                         ApplyTitles();
                                         Plot.InvalidatePlot(false);
                                       };
    }

    private void ApplyTitles()
    {
      Plot.Title = _localization["PlotTitle"];
      DirectionForwards.Title = _localization["PlotForward"];
      DirectionBackwards.Title = _localization["PlotBackward"];
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
