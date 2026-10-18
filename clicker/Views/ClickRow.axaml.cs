using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

using SharpHook;


namespace clicker.Views;

public partial class ClickRow : UserControl
{
  private const string GetPositionText = "Get Position";
  private EventLoopGlobalHook? _hook;
  public ClickRow()
  {
    InitializeComponent();
  }

  public int ClickCount => (int)CountBox.Value.GetValueOrDefault(1);

  public (short X, short Y)? Position => TryParseCoordinates(PositionBox.Text, out var x, out var y) ? (x, y) : null;

  private static bool TryParseCoordinates(string? input, out short x, out short y)
  {
    x = 0;
    y = 0;
    if (string.IsNullOrWhiteSpace(input)) return false;
    var parts = input.Split(',');
    if (parts.Length != 2) return false;
    return short.TryParse(parts[0], out x) && short.TryParse(parts[1], out y);
  }

  private void Get_Position(object? sender, RoutedEventArgs e)
  {
    if (_hook != null) return;
    GetClickPosition.Content = "Click anywhere...";
    _hook = new EventLoopGlobalHook();
    _hook.MousePressed += OnMousePressed;
    _ = _hook.RunAsync();
  }

  private void OnMousePressed(object? sender, MouseHookEventArgs e)
  {

    if (_hook == null) return;
    _hook.MousePressed -= OnMousePressed;
    _hook.Dispose();
    _hook = null;

    Dispatcher.UIThread.Post(() =>
  {
    PositionBox.Text = $"{e.Data.X},{e.Data.Y}";
    GetClickPosition.Content = GetPositionText;
  });
  }

}

