using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SharpHook;
using SharpHook.Simulation;
using System.Threading;
using System.Threading.Tasks;
using SharpHook.Data;
namespace clicker.Views;

public partial class MainWindow : Window
{
  private EventLoopGlobalHook? _hook;
  private EventLoopGlobalHook? _keyboardHook;
  private TextBox? _pendingTarget;
  private CancellationTokenSource? _cancellationTokenSource;
  public MainWindow()
  {
    InitializeComponent();
  }
  private async void Start_Click(object? sender, RoutedEventArgs e)
  {
    if (_cancellationTokenSource != null) return;
    if (!TryParseCoordinates(Click1Position.Text, out var x1, out var y1) || !TryParseCoordinates(Click2Position.Text, out var x2, out var y2)) return;
    var simulator = EventSimulator.Create("clicker");

    _keyboardHook = new EventLoopGlobalHook();
    _keyboardHook.KeyPressed += Stop_Click;
    _ = _keyboardHook.RunAsync();

    _cancellationTokenSource = new CancellationTokenSource();
    try
    {
      while (!_cancellationTokenSource.Token.IsCancellationRequested)
      {
        await ClickAtAsync(simulator, x1, y1, 3, _cancellationTokenSource.Token);
        await ClickAtAsync(simulator, x2, y2, 3, _cancellationTokenSource.Token);
      }
    }
    catch (OperationCanceledException)
    {
      // Task was canceled, do nothing
    }
    finally
    {
      _keyboardHook?.Dispose();
      _keyboardHook = null;
      _cancellationTokenSource?.Dispose();
      _cancellationTokenSource = null;
      simulator.Dispose();
    }
  }

  private void Get_Position(object? sender, RoutedEventArgs e)
  {
    var button = (sender as Button)?.Name;
    var textBox = button?.Remove(0, 3);
    if (textBox == null) return;
    _pendingTarget = this.FindControl<TextBox>(textBox);
    _hook = new EventLoopGlobalHook();
    _hook.MousePressed += OnMousePressed;
    _ = _hook.RunAsync();
  }

  private void Stop_Click(object? sender, KeyboardHookEventArgs e)
  {
    _cancellationTokenSource?.Cancel();
  }
  private void OnMousePressed(object? sender, MouseHookEventArgs e)
  {
    if (_pendingTarget == null) return;

    if (_hook == null) return;
    _hook.MousePressed -= OnMousePressed;
    _hook.Dispose();
    _hook = null;

    Dispatcher.UIThread.Post(() =>
  {
    _pendingTarget.Text = $"{e.Data.X},{e.Data.Y}";
  });
  }

  private static bool TryParseCoordinates(string? input, out short x, out short y)
  {
    x = 0;
    y = 0;
    if (string.IsNullOrWhiteSpace(input)) return false;
    var parts = input.Split(',');
    if (parts.Length != 2) return false;
    return short.TryParse(parts[0], out x) && short.TryParse(parts[1], out y);
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

}