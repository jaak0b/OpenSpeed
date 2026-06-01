using System;
using OpenSpeed.Core.Controller;
using OpenSpeed.Core.EventArgs;
using OpenSpeed.Core.Models;
using OpenSpeed.UI.Localization;
using OpenSpeed.UI.Theming;
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
    private readonly ThemeManager _themeManager;
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

    public SpeedPlotViewModel(IMeasurementController measurementController, LocalizationManager localization, ThemeManager themeManager)
    {
      _localization = localization;
      _themeManager = themeManager;

      _xAxis = new LinearAxis
               {
                 Position = AxisPosition.Bottom,
                 AbsoluteMinimum = 0,
                 AbsoluteMaximum = 128,
                 Minimum = 0,
                 Maximum = 128,
                 IsZoomEnabled = false,
                 IsPanEnabled = false,
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
                 IsZoomEnabled = false,
                 IsPanEnabled = false,
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
      ApplyTheme();
      Plot.Series.Add(DirectionForwards);
      Plot.Series.Add(DirectionBackwards);
      measurementController.OnSpeedStepMeasured += MeasurementController_OnOnSpeedStepMeasured;
      _localization.LanguageChanged += (_, _) =>
                                       {
                                         ApplyTitles();
                                         Plot.InvalidatePlot(false);
                                       };
      _themeManager.ThemeChanged += (_, _) =>
                                    {
                                      ApplyTheme();
                                      Plot.InvalidatePlot(false);
                                    };
    }

    private void ApplyTheme()
    {
      var dark = _themeManager.Current == Theme.Dark;

      var surface = dark ? OxyColor.Parse("#2D3748") : OxyColors.White;
      var text = dark ? OxyColor.Parse("#F7FAFC") : OxyColor.Parse("#1A202C");
      var grid = dark ? OxyColor.Parse("#4A5568") : OxyColor.Parse("#E2E8F0");

      Plot.Background = surface;
      Plot.PlotAreaBackground = surface;
      Plot.TextColor = text;
      Plot.PlotAreaBorderColor = grid;

      foreach (var axis in new[] { _xAxis, _yAxis })
      {
        axis.MajorGridlineColor = grid;
        axis.TicklineColor = grid;
        axis.AxislineColor = grid;
        axis.TextColor = text;
        axis.TitleColor = text;
      }

      foreach (var legend in Plot.Legends)
      {
        legend.LegendBackground = OxyColor.FromAColor(220, surface);
        legend.LegendBorder = grid;
        legend.LegendTextColor = text;
      }
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
