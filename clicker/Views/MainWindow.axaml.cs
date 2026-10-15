using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace clicker.Views;

public partial class MainWindow : Window
{
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
    TextBox? CorrespondingTextBox = this.FindControl<TextBox>(textBox);
    if (CorrespondingTextBox == null) return;
    CorrespondingTextBox.Text = "Clicked!";
    Debug.WriteLine($"Corresponding text box: {textBox}");
  }
}