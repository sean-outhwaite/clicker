using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SharpHook;
namespace clicker.Views;

public partial class MainWindow : Window
{
  private EventLoopGlobalHook? _hook;
  private TextBox? _pendingTarget;
  public MainWindow()
  {
    InitializeComponent();
  }
  private void Start_Click(object? sender, RoutedEventArgs e)
  {
    Debug.WriteLine("Click");
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

    Dispatcher.UIThread.Post(() =>
  {
    _pendingTarget.Text = $"{e.Data.X}, {e.Data.Y}";
  });
  }

}