using System;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Interactivity;

using SharpHook;
using SharpHook.Data;
using SharpHook.Simulation;
namespace clicker.Views;

public partial class MainWindow : Window
{
  private EventLoopGlobalHook? _stopHook;
  private CancellationTokenSource? _cancellationTokenSource;
  public MainWindow()
  {
    InitializeComponent();
    Row1.StatusChanged += OnRowStatusChanged;
    Row2.StatusChanged += OnRowStatusChanged;
  }
  private async void Start_Click(object? sender, RoutedEventArgs e)
  {
    if (_cancellationTokenSource != null) return;
    if (Row1.Position is not { } p1 || Row2.Position is not { } p2)
    {
      StatusText.Text = "Please set the position for both clicks.";
      return;
    }

    var simulator = EventSimulator.Create("clicker");

    var click1Count = Row1.ClickCount;
    var click2Count = Row2.ClickCount;
    if (click1Count < 1 || click2Count < 1) return;

    var result = simulator.SimulateMouseMovement(p1.X, p1.Y);
    if (result != UioHookResult.Success)
    {
      StatusText.Text = "Failed to move mouse, please check accessibility permissions in System Settings.";
      return;
    }

    _stopHook = new EventLoopGlobalHook();
    _stopHook.MouseMoved += HandleMouseMoved;
    _stopHook.MouseDragged += HandleMouseMoved;
    _stopHook.KeyPressed += HandleKeyPressed;
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
      _stopHook?.Dispose();
      _stopHook = null;
      _cancellationTokenSource?.Dispose();
      _cancellationTokenSource = null;
      simulator.Dispose();
      StatusText.Text = "";
    }
  }

  private void HandleKeyPressed(object? sender, KeyboardHookEventArgs e)
  {
    _cancellationTokenSource?.Cancel();
  }

  private void HandleMouseMoved(object? sender, MouseHookEventArgs e)
  {
    if (!e.IsEventSimulated) _cancellationTokenSource?.Cancel();
  }

  private async Task ClickAtAsync(EventSimulator simulator, short x, short y, int times, CancellationToken token)
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