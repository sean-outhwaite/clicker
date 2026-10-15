using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SharpHook;
using SharpHook.Simulation;
namespace clicker.Views;

public partial class MainWindow : Window
{
  private EventLoopGlobalHook? _hook;
  private TextBox? _pendingTarget;
  public MainWindow()
  {
    InitializeComponent();
  }
  private async void Start_Click(object? sender, RoutedEventArgs e)
  {
    if (Click1Position.Text == null || Click2Position.Text == null) return;
    var click1 = Array.ConvertAll(Click1Position.Text.Split(','), s => short.Parse(s));
    var click2 = Array.ConvertAll(Click2Position.Text.Split(','), s => short.Parse(s));
    var simulator = EventSimulator.Create("clicker");
    simulator.SimulateMouseMovement(click1[0], click1[1]);
  }

  private void Get_Position(object? sender, RoutedEventArgs e)
  {
    var button = (sender as Button)?.Name;
    var textBox = button?.Remove(0, 3);
    if (textBox == null) return;
    _pendingTarget = this.FindControl<TextBox>(textBox);
    _hook = new EventLoopGlobalHook();
    _hook.MousePressed += OnMousePressed;
    _hook.RunAsync();
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

}