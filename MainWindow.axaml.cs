namespace UnityPeek
{
	using System;
	using Avalonia.Controls;
	using Avalonia.Interactivity;
	using Avalonia.Media;

	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			this.InitializeComponent();

			this.HierarchyTreeView.SelectionChanged += this.HierarchyTreeView_SelectionChanged;

			// Change of text color when box is selected
			this.IpText.GotFocus += (sender, e) =>
			{
				this.IpText.SetValue(TextBox.ForegroundProperty, Brushes.Black);
			};

			this.IpText.LostFocus += (sender, e) =>
			{
				this.IpText.SetValue(TextBox.ForegroundProperty, Brushes.White);
			};

			this.PortText.GotFocus += (sender, e) =>
			{
				this.PortText.SetValue(TextBox.ForegroundProperty, Brushes.Black);
			};

			this.PortText.LostFocus += (sender, e) =>
			{
				this.PortText.SetValue(TextBox.ForegroundProperty, Brushes.White);
			};

			this.EnabledCheckedBox.Click += (sender, e) =>
			{
				this.EnabledCheckedBoxChanged?.Invoke(sender, (bool)this.EnabledCheckedBox.IsChecked!);
			};

			this.LogTextBox.GotFocus += (sender, e) =>
			{
				this.LogTextBox.SetValue(TextBox.ForegroundProperty, Brushes.Black);
				//this.LogTextBox.SetValue(TextBox.BackgroundProperty, Brushes.Black);
			};

			this.LogTextBox.LostFocus += (sender, e) =>
			{
				this.LogTextBox.SetValue(TextBox.ForegroundProperty, Brushes.White);
				//this.LogTextBox.SetValue(TextBox.BackgroundProperty, Brushes.Black);
			};
		}

		public event EventHandler? AttachButtonClicked;

		public event EventHandler? ConnectButtonClicked;

		public event EventHandler? DisconnectButtonClicked;

		public event EventHandler? FetchHirarchyClicked;

		public event EventHandler? SelectedHierachyNode;

		public event EventHandler? DeleteButtonPressed;

		public event EventHandler<bool>? EnabledCheckedBoxChanged;

		public TextBlock OutputDisplay => this.FindControl<TextBlock>("OutputTextBlock") !;

		public TextBlock MethodsDisplay => this.FindControl<TextBlock>("MethodsTextBlock") !;

		public TextBlock ProcessTextBlock => this.FindControl<TextBlock>("ProcessText") !;

		public TextBox IpText => this.FindControl<TextBox>("IP") !;

		public TextBox PortText => this.FindControl<TextBox>("Port") !;

		public TextBlock ConnectedText => this.FindControl<TextBlock>("ConnectionStatus") !;

		// All the button events
		private void AttachButton_Click(object? sender, RoutedEventArgs e)
		{
			this.AttachButtonClicked?.Invoke(sender, e);
		}

		private void ConnectButton_Click(object? sender, RoutedEventArgs e)
		{
			this.ConnectButtonClicked?.Invoke(sender, e);
		}

		private void DisconnectButton_Click(object? sender, RoutedEventArgs e)
		{
			this.DisconnectButtonClicked?.Invoke(sender, e);
		}

		private void FetchHirarchy_Click(object? sender, RoutedEventArgs e)
		{
			this.FetchHirarchyClicked?.Invoke(sender, e);
		}

		private void HierarchyTreeView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
		{
			this.SelectedHierachyNode!.Invoke(sender, e);
		}

		private void DeleteButton_Click(object? sender, RoutedEventArgs e)
		{
			this.DeleteButtonPressed?.Invoke(sender, e);
		}
	}
}