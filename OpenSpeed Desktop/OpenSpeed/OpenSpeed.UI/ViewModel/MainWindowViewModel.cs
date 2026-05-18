using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using OpenSpeed.Core.Controller;
using OpenSpeed.Core.Helper;
using OpenSpeed.Core.Interfaces;
using OpenSpeed.Core.Models;
using OpenSpeed.Core.Models.Configuration;

namespace OpenSpeed.UI.ViewModel
{
  public class MainWindowViewModel : INotifyPropertyChanged
  {
    private readonly IMeasurementController _measurementController;
    private readonly IZ21Controller _z21Controller;
    private ReachabilityMonitor _z21ReachabilityMonitor;
    private ReachabilityMonitor _speedSensorReachabilityMonitor;
    private bool _speedSensorConnected;
    private bool _z21Connected;
    private bool _measurementInProgress;
    private CancellationTokenSource? _cancellationTokenSource;

    public SpeedPlotViewModel PlotViewModel { get; }

    public MeasurementConfiguration MeasurementConfiguration { get; }

    public EndPointConfiguration EndPointConfiguration { get; }

    public LocomotiveConfiguration LocomotiveConfiguration { get; }

    public ObservableCollection<SpeedStepMeasurement> Steps { get; } = [];

    public MainWindowViewModel(IMeasurementController measurementController, SpeedPlotViewModel plotViewModel, MeasurementConfiguration measurementConfiguration, EndPointConfiguration endPointConfiguration,
                               LocomotiveConfiguration locomotiveConfiguration, IZ21Controller z21Controller)
    {
      PlotViewModel = plotViewModel;
      MeasurementConfiguration = measurementConfiguration;
      EndPointConfiguration = endPointConfiguration;
      LocomotiveConfiguration = locomotiveConfiguration;

      _measurementController = measurementController;
      _z21Controller = z21Controller;
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
      }
    }

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
        var result = MessageBox.Show($"Please set CV3 and CV4 of the locomotive decoder to 0!", "Input required", MessageBoxButton.OKCancel);
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
        MessageBox.Show(e.Message, "Error", MessageBoxButton.OK);
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
        MessageBox.Show(e.Message, "Error", MessageBoxButton.OK);
      } finally
      {
        MeasurementInProgress = false;
      }
    }
  }
}