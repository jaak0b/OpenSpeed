using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using OpenSpeed.Core.Controller;
using OpenSpeed.Core.Helper;
using OpenSpeed.Core.Interfaces;
using OpenSpeed.Core.Models;
using OpenSpeed.Core.Models.Configuration;
using OpenSpeed.UI.Localization;

namespace OpenSpeed.UI.ViewModel
{
  public class MainWindowViewModel : INotifyPropertyChanged
  {
    private readonly IMeasurementController _measurementController;
    private readonly ILengthMeasurementController _lengthMeasurementController;
    private readonly IZ21Controller _z21Controller;
    private readonly AppConfiguration _appConfiguration;
    private readonly LocalizationManager _localization;
    private ReachabilityMonitor _z21ReachabilityMonitor;
    private ReachabilityMonitor _speedSensorReachabilityMonitor;
    private bool _speedSensorConnected;
    private bool _z21Connected;
    private bool _measurementInProgress;
    private bool _lengthMeasurementInProgress;
    private double? _trainLengthCm;
    private CancellationTokenSource? _cancellationTokenSource;

    public SpeedPlotViewModel PlotViewModel { get; }

    public MeasurementConfiguration MeasurementConfiguration { get; }

    public EndPointConfiguration EndPointConfiguration { get; }

    public LocomotiveConfiguration LocomotiveConfiguration { get; }

    public LengthMeasurementConfiguration LengthMeasurementConfiguration { get; }

    public Language SelectedLanguage
    {
      get => _appConfiguration.Language;
      set
      {
        if (value == _appConfiguration.Language)
          return;
        _appConfiguration.Language = value;
        _localization.SetLanguage(value);
        OnPropertyChanged();
      }
    }

    public ObservableCollection<SpeedStepMeasurement> Steps { get; } = [];

    public MainWindowViewModel(IMeasurementController measurementController, SpeedPlotViewModel plotViewModel, MeasurementConfiguration measurementConfiguration, EndPointConfiguration endPointConfiguration,
                               LocomotiveConfiguration locomotiveConfiguration, IZ21Controller z21Controller,
                               ILengthMeasurementController lengthMeasurementController, LengthMeasurementConfiguration lengthMeasurementConfiguration,
                               AppConfiguration appConfiguration, LocalizationManager localization)
    {
      PlotViewModel = plotViewModel;
      MeasurementConfiguration = measurementConfiguration;
      EndPointConfiguration = endPointConfiguration;
      LocomotiveConfiguration = locomotiveConfiguration;
      LengthMeasurementConfiguration = lengthMeasurementConfiguration;

      _measurementController = measurementController;
      _lengthMeasurementController = lengthMeasurementController;
      _z21Controller = z21Controller;
      _appConfiguration = appConfiguration;
      _localization = localization;
      _measurementController.OnSpeedStepMeasured += (_, args) => Application.Current.Dispatcher.BeginInvoke(() => Steps.Add(args.Measurement));
      _z21ReachabilityMonitor = new(() => EndPointConfiguration.Z21IpAddress, b => Z21Connected = b);
      _speedSensorReachabilityMonitor = new(() => EndPointConfiguration.SpeedSensorIpAddress, b => SpeedSensorConnected = b);
    }

    public bool MeasurementInProgress
    {
      get => _measurementInProgress;
      set
      {
        if (value == _measurementInProgress)
          return;
        _measurementInProgress = value;
        OnPropertyChanged();
        OnPropertyChanged(nameof(AnyMeasurementInProgress));
      }
    }

    public bool LengthMeasurementInProgress
    {
      get => _lengthMeasurementInProgress;
      set
      {
        if (value == _lengthMeasurementInProgress)
          return;
        _lengthMeasurementInProgress = value;
        OnPropertyChanged();
        OnPropertyChanged(nameof(AnyMeasurementInProgress));
      }
    }

    public bool AnyMeasurementInProgress => MeasurementInProgress || LengthMeasurementInProgress;

    public double? TrainLengthCm
    {
      get => _trainLengthCm;
      set
      {
        _trainLengthCm = value;
        OnPropertyChanged();
        OnPropertyChanged(nameof(TrainLengthDisplay));
      }
    }

    public string TrainLengthDisplay => _trainLengthCm.HasValue ? $"{_trainLengthCm.Value:F1} cm" : "—";

    public bool Z21Connected
    {
      get => _z21Connected;
      set
      {
        if (value == _z21Connected)
          return;
        _z21Connected = value;
        OnPropertyChanged();
      }
    }

    public bool SpeedSensorConnected
    {
      get => _speedSensorConnected;
      set
      {
        if (value == _speedSensorConnected)
          return;
        _speedSensorConnected = value;
        OnPropertyChanged();
      }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new(propertyName));
    }

    public async Task StartMeasurement()
    {
      try
      {
        var result = MessageBox.Show(_localization["MsgSetCv"], _localization["MsgInputRequired"], MessageBoxButton.OKCancel);
        if (result != MessageBoxResult.OK)
          return;

        MeasurementInProgress = true;
        _cancellationTokenSource = new();
        PlotViewModel.ClearPoints();
        Steps.Clear();
        _z21Controller.Configure(EndPointConfiguration.Z21IpAddress);
        await _measurementController.StartMeasurementAsync(LocomotiveConfiguration, EndPointConfiguration, MeasurementConfiguration, _cancellationTokenSource.Token);
      }
      catch (OperationCanceledException) { }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, _localization["MsgError"], MessageBoxButton.OK);
      } finally
      {
        MeasurementInProgress = false;
      }
    }

    public async Task CancelMeasurement()
    {
      try
      {
        await (_cancellationTokenSource?.CancelAsync() ?? Task.CompletedTask);
      }
      catch (OperationCanceledException) { }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, _localization["MsgError"], MessageBoxButton.OK);
      } finally
      {
        MeasurementInProgress = false;
      }
    }

    public async Task StartLengthMeasurement()
    {
      var confirm = MessageBox.Show(
        _localization["MsgPositionLoco"],
        _localization["BtnMeasureLength"], MessageBoxButton.OKCancel);
      if (confirm != MessageBoxResult.OK)
        return;

      try
      {
        LengthMeasurementInProgress = true;
        TrainLengthCm = null;
        _cancellationTokenSource = new();
        _z21Controller.Configure(EndPointConfiguration.Z21IpAddress);
        TrainLengthCm = await _lengthMeasurementController.MeasureAsync(
          LocomotiveConfiguration, EndPointConfiguration, LengthMeasurementConfiguration,
          _cancellationTokenSource.Token);
      }
      catch (OperationCanceledException) { }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, _localization["MsgError"], MessageBoxButton.OK);
      }
      finally
      {
        LengthMeasurementInProgress = false;
      }
    }

    public async Task CancelLengthMeasurement()
    {
      try
      {
        await (_cancellationTokenSource?.CancelAsync() ?? Task.CompletedTask);
      }
      catch (OperationCanceledException) { }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, _localization["MsgError"], MessageBoxButton.OK);
      }
      finally
      {
        LengthMeasurementInProgress = false;
      }
    }
  }
}