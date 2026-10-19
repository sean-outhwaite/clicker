using System;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

using SharpHook;
using SharpHook.Data;
using SharpHook.Simulation;
namespace clicker.Views;

public partial class MainWindow : Window
{

  public MainWindow()
  {
    InitializeComponent();
    Row1.StatusChanged += OnRowStatusChanged;
    Row2.StatusChanged += OnRowStatusChanged;
  }

  private EventLoopGlobalHook? _stopHook;
  private CancellationTokenSource? _cancellationTokenSource;
  private async void StartButton_Click(object? sender, RoutedEventArgs e)
  {
    if (_cancellationTokenSource != null)
    {
      _cancellationTokenSource.Cancel();
      return;
    }
    if (Row1.Position is not { } p1 || Row2.Position is not { } p2)
    {
      StatusText.Text = "Please set the position for both clicks.";
      return;
    }

    using var simulator = EventSimulator.Create("clicker");

    var click1Count = Row1.ClickCount;
    var click2Count = Row2.ClickCount;
    if (click1Count < 1 || click2Count < 1)
    {
      StatusText.Text = "Must click at least once for each position.";
      return;
    }

    var result = simulator.SimulateMouseMovement(p1.X, p1.Y);
    if (result != UioHookResult.Success)
    {
      StatusText.Text = "Failed to move mouse, please check accessibility permissions in System Settings.";
      return;
    }

    StartButton.Content = "Stop";

    _stopHook = new EventLoopGlobalHook();
    _stopHook.MouseMoved += StopHook_MouseMoved;
    _stopHook.MouseDragged += StopHook_MouseMoved;
    _stopHook.KeyPressed += StopHook_KeyPressed;
    _ = _stopHook.RunAsync();

    _cancellationTokenSource = new CancellationTokenSource();
    StatusText.Text = "Clicking... Press any key or move the cursor to stop.";
    try
    {
      while (!_cancellationTokenSource.Token.IsCancellationRequested)
      {
        await ClickAtAsync(simulator, p1.X, p1.Y, click1Count, _cancellationTokenSource.Token);
        await ClickAtAsync(simulator, p2.X, p2.Y, click2Count, _cancellationTokenSource.Token);
      }
    }
    catch (OperationCanceledException)
    {
      // Task was canceled, do nothing
    }
    finally
    {
      StartButton.Content = "Start";
      _stopHook?.Dispose();
      _stopHook = null;
      _cancellationTokenSource?.Dispose();
      _cancellationTokenSource = null;
      StatusText.Text = "";
    }
  }

  private void StopHook_KeyPressed(object? sender, KeyboardHookEventArgs e)
  {
    Dispatcher.UIThread.Post(() => _cancellationTokenSource?.Cancel());
  }

  private void StopHook_MouseMoved(object? sender, MouseHookEventArgs e)
  {
    if (!e.IsEventSimulated) Dispatcher.UIThread.Post(() => _cancellationTokenSource?.Cancel());
  }

  private static async Task ClickAtAsync(EventSimulator simulator, short x, short y, int times, CancellationToken token)
  {
    for (int i = 0; i < times; i++)
    {
      simulator.SimulateMouseMovement(x, y);
      simulator.SimulateMousePress(MouseButton.Button1);
      // No token on purpose, the release below should always run
      await Task.Delay(Random.Shared.Next(20, 60));
      simulator.SimulateMouseRelease(MouseButton.Button1);
      await Task.Delay(Random.Shared.Next(200, 1000), token);
    }
  }

  private void OnRowStatusChanged(object? sender, string message) => StatusText.Text = message;
}