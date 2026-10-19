using System;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

using SharpHook;


namespace clicker.Views;

public partial class ClickRow : UserControl
{
  private const string GetPositionText = "Get Position";
  private EventLoopGlobalHook? _captureHook;
  public ClickRow()
  {
    InitializeComponent();
    GetPositionButton.Content = GetPositionText;
  }
  public event EventHandler<string>? StatusChanged;

  public string? Label
  {
    get => LabelText.Text;
    set => LabelText.Text = value;
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

  private async void GetPositionButtonClick(object? sender, RoutedEventArgs e)
  {
    if (_captureHook != null) return;
    GetPositionButton.Content = "Tracking...";
    _captureHook = new EventLoopGlobalHook();
    _captureHook.MousePressed += CaptureHook_MousePressed;
    StatusChanged?.Invoke(this, "Tracking cursor. Click anywhere to set the position");
    try
    {
      await _captureHook.RunAsync();
      StatusChanged?.Invoke(this, "");
    }
    catch (HookException)
    {
      StatusChanged?.Invoke(this, "Failed to start mouse capture");
    }
    finally
    {
      _captureHook?.Dispose();
      _captureHook = null;
      GetPositionButton.Content = GetPositionText;
    }
  }

  private void CaptureHook_MousePressed(object? sender, MouseHookEventArgs e)
  {
    if (_captureHook == null) return;
    _captureHook.Dispose();

    Dispatcher.UIThread.Post(() =>
  {
    PositionBox.Text = $"{e.Data.X},{e.Data.Y}";
  });
  }

}