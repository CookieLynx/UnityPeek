using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace UnityPeek;

public partial class MainWindow : Window
{
    public TextBlock OutputDisplay => this.FindControl<TextBlock>("OutputTextBlock");
    public TextBlock MethodsDisplay => this.FindControl<TextBlock>("MethodsTextBlock");

    public TextBlock ProcessTextBlock => this.FindControl<TextBlock>("ProcessText");

    public event EventHandler? AttachButtonClicked;
    public MainWindow()
    {
        InitializeComponent();
    }

    private void AttachButton_Click(object? sender, RoutedEventArgs e)
    {
        AttachButtonClicked?.Invoke(sender, e);

    }
}