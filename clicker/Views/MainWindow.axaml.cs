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
    Debug.WriteLine("Click position");
  }
}