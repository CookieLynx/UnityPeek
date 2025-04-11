using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace UnityPeek;

public partial class MainWindow : Window
{
	public TextBlock OutputDisplay => this.FindControl<TextBlock>("OutputTextBlock");
	public TextBlock MethodsDisplay => this.FindControl<TextBlock>("MethodsTextBlock");

	public TextBlock ProcessTextBlock => this.FindControl<TextBlock>("ProcessText");

	public TextBox IpText => this.FindControl<TextBox>("IP");
	public TextBox PortText => this.FindControl<TextBox>("Port");

	public TextBlock ConnectedText => this.FindControl<TextBlock>("ConnectionStatus");

	public event EventHandler? AttachButtonClicked, ConnectButtonClicked, DisconnectButtonClicked, FetchHirarchyClicked, SelectedHierachyNode;
	public MainWindow()
	{
		InitializeComponent();

		HierarchyTreeView.SelectionChanged += HierarchyTreeView_SelectionChanged;

		//Change of text color when box is selected
		IpText.GotFocus += (sender, e) =>
		{
			IpText.SetValue(TextBox.ForegroundProperty, Brushes.Black);
		};

		IpText.LostFocus += (sender, e) =>
		{
			IpText.SetValue(TextBox.ForegroundProperty, Brushes.White);
		};

		PortText.GotFocus += (sender, e) =>
		{
			PortText.SetValue(TextBox.ForegroundProperty, Brushes.Black);
		};

		PortText.LostFocus += (sender, e) =>
		{
			PortText.SetValue(TextBox.ForegroundProperty, Brushes.White);
		};
	}

	//All the button events

	private void AttachButton_Click(object? sender, RoutedEventArgs e)
	{
		AttachButtonClicked?.Invoke(sender, e);

	}


	private void ConnectButton_Click(object? sender, RoutedEventArgs e)
	{
		ConnectButtonClicked?.Invoke(sender, e);
	}

	private void DisconnectButton_Click(object? sender, RoutedEventArgs e)
	{
		DisconnectButtonClicked?.Invoke(sender, e);
	}

	private void FetchHirarchy_Click(object? sender, RoutedEventArgs e)
	{
		FetchHirarchyClicked?.Invoke(sender, e);
	}


	private void HierarchyTreeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		SelectedHierachyNode.Invoke(sender, e);
	}
}