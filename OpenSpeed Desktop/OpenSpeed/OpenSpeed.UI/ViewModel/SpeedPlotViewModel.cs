using System;
using OpenSpeed.Core.Controller;
using OpenSpeed.Core.EventArgs;
using OpenSpeed.UI.Localization;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace OpenSpeed.UI.ViewModel
{
  public class SpeedPlotViewModel
  {
    private const double DefaultMaxSpeed = 200;

    private readonly LocalizationManager _localization;
    private readonly LinearAxis _xAxis;
    private readonly LinearAxis _yAxis;

    public PlotModel Plot { get; } = new();

    public LineSeries DirectionForwards { get; } = new()
                                                   {
                                                     Color = OxyColor.Parse("#3182CE"),
                                                     MarkerFill = OxyColor.Parse("#3182CE"),
                                                     MarkerType = MarkerType.Circle,
                                                     MarkerSize = 3
                                                   };

    public LineSeries DirectionBackwards { get; } = new()
                                                    {
                                                      Color = OxyColor.Parse("#DD6B20"),
                                                      MarkerFill = OxyColor.Parse("#DD6B20"),
                                                      MarkerType = MarkerType.Circle,
                                                      MarkerSize = 3
                                                    };

    public SpeedPlotViewModel(IMeasurementController measurementController, LocalizationManager localization)
    {
      _localization = localization;

      _xAxis = new LinearAxis
               {
                 Position = AxisPosition.Bottom,
                 AbsoluteMinimum = 0,
                 AbsoluteMaximum = 128,
                 Minimum = 0,
                 Maximum = 128,
                 MajorGridlineStyle = LineStyle.Solid,
                 MajorGridlineColor = OxyColor.Parse("#E2E8F0"),
                 MinorGridlineStyle = LineStyle.None
               };

      _yAxis = new LinearAxis
               {
                 Position = AxisPosition.Left,
                 AbsoluteMinimum = 0,
                 Minimum = 0,
                 Maximum = DefaultMaxSpeed,
                 MajorGridlineStyle = LineStyle.Solid,
                 MajorGridlineColor = OxyColor.Parse("#E2E8F0"),
                 MinorGridlineStyle = LineStyle.None
               };

      Plot.Axes.Add(_xAxis);
      Plot.Axes.Add(_yAxis);

      Plot.Legends.Add(new Legend
                       {
                         LegendPosition = LegendPosition.TopLeft,
                         LegendPlacement = LegendPlacement.Inside,
                         LegendBackground = OxyColor.FromAColor(220, OxyColors.White),
                         LegendBorder = OxyColor.Parse("#E2E8F0")
                       });

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
      _xAxis.Title = _localization["AxisSpeedStep"];
      _yAxis.Title = _localization["AxisSpeed"];
    }

    private void MeasurementController_OnOnSpeedStepMeasured(object? sender, SpeedStepMeasurementEventArgs args)
    {
      if (args.Measurement.ForwardPass is { } forwardSpeed)
        DirectionForwards.Points.Add(new(args.Measurement.SpeedStep, forwardSpeed));

      if (args.Measurement.BackwardPass is { } backwardSpeed)
        DirectionBackwards.Points.Add(new(args.Measurement.SpeedStep, backwardSpeed));

      ExpandYAxisToFitData();
      Plot.InvalidatePlot(true);
    }

    private void ExpandYAxisToFitData()
    {
      var maxObserved = DefaultMaxSpeed;
      foreach (var point in DirectionForwards.Points)
        maxObserved = Math.Max(maxObserved, point.Y);
      foreach (var point in DirectionBackwards.Points)
        maxObserved = Math.Max(maxObserved, point.Y);

      _yAxis.Maximum = Math.Max(DefaultMaxSpeed, Math.Ceiling(maxObserved / 20.0) * 20);
    }

    public void ClearPoints()
    {
      DirectionForwards.Points.Clear();
      DirectionBackwards.Points.Clear();
      _yAxis.Maximum = DefaultMaxSpeed;
    }
  }
}
