namespace UnityPeek.UI
{
	using System;
	using System.Collections.Generic;
	using System.Numerics;
	using Avalonia.Platform.Storage;
	using MsBox.Avalonia;
	using MsBox.Avalonia.Enums;
	using UnityPeek.Handlers;
	using Debug = System.Diagnostics.Debug;

	public class UIManager
	{
		public static UIManager UIManagerSingleton;

		private MainWindow mainWindow;
		public enum LogType 
		{
			Message,
			Warn,
			Error

		}


		// Constructor to get all of the button methods and the mainWindow itself
		public UIManager(MainWindow mainWindow)
		{
			this.mainWindow = mainWindow;
			mainWindow.AttachButtonClicked += this.AttachButtonPressed;
			mainWindow.ConnectButtonClicked += this.ConnectButtonPressed;
			mainWindow.DisconnectButtonClicked += this.DisconnectButtonPressed;
			mainWindow.FetchHirarchyClicked += this.FetchHirarchyPressed;
			mainWindow.EnabledCheckedBoxChanged += (sender, isEnabled) => InspectorHandler.EnabledCheckedBoxChanged(isEnabled);
			mainWindow.SelectedHierachyNode += this.SelectedHierachyNode;
			UIManagerSingleton = this;
		}

		public static void LogMessage(string message, LogType type = LogType.Message)
		{
			TimeOnly time = TimeOnly.FromDateTime(DateTime.Now);
			string timeString = time.ToString("HH:mm:ss");
			Debug.WriteLine(message);
			if (UIManagerSingleton.mainWindow.LogTextBox.Text == null)
			{
				UIManagerSingleton.mainWindow.LogTextBox.Text = type.ToString() + " " + timeString + " - " + message;
			}
			else
			{
				UIManagerSingleton.mainWindow.LogTextBox.Text = UIManagerSingleton.mainWindow.LogTextBox.Text + "\n" + type.ToString() + " " + timeString + " - " + message;
			}
		}

		/// <summary>
		/// Show a popup error message.
		/// </summary>
		/// <param name="title">Error box tittle.</param>
		/// <param name="message">Error box message.</param>
		/// <param name="icon">Icon for icon to display.</param>
		public static async void ShowPopup(string title, string message, Icon icon)
		{
			var messageBox = MessageBoxManager
				.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);

			await messageBox.ShowAsync();
		}

		// Update the connection text
		public static void ChangeConnectedText(bool connected)
		{
			Avalonia.Threading.Dispatcher.UIThread.Post(() =>
			{
				UIManagerSingleton.mainWindow.ConnectedText.Text = connected ? "Connected" : "Disconnected";

				// we also clear the hierarchy view
				UIManagerSingleton.mainWindow.HierarchyTreeView.ItemsSource = null;
			});
		}

		public static void UpdateHierarchyView(HierarchyNode root)
		{
			UIManagerSingleton.mainWindow.HierarchyTreeView.ItemsSource = new List<HierarchyNode> { root };
		}

		public static void UpdateSelectedNodeTransform(string name, bool enabled, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			Vector3 roationVector = Helpers.GetQuaternionEulerAngle(rotation);
			Avalonia.Threading.Dispatcher.UIThread.Post(() =>
			{
				UIManagerSingleton.mainWindow.ObjectName.Text = name;
				UIManagerSingleton.mainWindow.EnabledCheckedBox.IsChecked = enabled;
				UIManagerSingleton.mainWindow.PositionX.Text = "X:" + position.X;
				UIManagerSingleton.mainWindow.PositionY.Text = "Y:" + position.Y;
				UIManagerSingleton.mainWindow.PositionZ.Text = "Z:" + position.Z;
				UIManagerSingleton.mainWindow.RotationX.Text = "X:" + roationVector.X;
				UIManagerSingleton.mainWindow.RotationY.Text = "Y:" + roationVector.Y;
				UIManagerSingleton.mainWindow.RotationZ.Text = "Z:" + roationVector.Z;
				UIManagerSingleton.mainWindow.ScaleX.Text = "X:" + scale.X;
				UIManagerSingleton.mainWindow.ScaleY.Text = "Y:" + scale.X;
				UIManagerSingleton.mainWindow.ScaleZ.Text = "Z:" + scale.X;
			});
		}

		public void Start()
		{
			// Load or create config
			ConfigManager.LoadConfig();
			UIManager.LogMessage("App has booted successfully!");

			// Set the values inside the window
			this.mainWindow.IpText.Text = ConfigManager.IP;
			this.mainWindow.PortText.Text = ConfigManager.Port;

			/*

			_statusTextBlock.Text = "yoyoyo";

			var module = Mono.Cecil.AssemblyDefinition.ReadAssembly(filePath);
			// Load the assembly using PEFile (instead of Mono.Cecil)
			var module2 = new PEFile(filePath);
			var decompiler = new CSharpDecompiler(filePath, new DecompilerSettings()
			{
				ThrowOnAssemblyResolveErrors = false
			});

			// Read metadata using MetadataReader
			var reader = module2.Metadata;

			// Get the target type by index or name
			var targetType = reader.TypeDefinitions
				.Select(handle => reader.GetTypeDefinition(handle))
				.ElementAt(20); // 4 = Index of type

			string typeName = reader.GetString(targetType.Name);
			UIManager.LogMessage($"Type: {typeName}");

			foreach (var handle in reader.MethodDefinitions)
			{
				var method = reader.GetMethodDefinition(handle);
				var name = reader.GetString(method.Name);

				// Ensure this method belongs to the target type
				if (method.GetDeclaringType() == reader.TypeDefinitions.ElementAt(20))
				{
					UIManager.LogMessage($"  Method: {name}");

					try
					{
						// Decompile using the EntityHandle
						string code = decompiler.DecompileAsString(handle);
						UIManager.LogMessage($"Decompiled Code:\n{code}");
					}
					catch (Exception ex)
					{
						UIManager.LogMessage($"Error decompiling {name}: {ex.Message}");
					}
				}
			}

			*/
			/*
			int index = 4;
			_outputTextBlock.Text = module.MainModule.Types[index].Name;
			foreach (var method in module.MainModule.Types[index].Methods)
			{
				Avalonia.Threading.Dispatcher.UIThread.Post(() =>
				{
					_methodsTextBlock.Text += $"Method: {method.Name}\n";
				});
				Avalonia.Threading.Dispatcher.UIThread.Post(() =>
				{
					_methodsTextBlock.Text += method.NoInlining   +"\n";
				});

				foreach (var item in method.Body.Instructions)
				{
					Avalonia.Threading.Dispatcher.UIThread.Post(() =>
					{
						_methodsTextBlock.Text += $"Instruction: {item}\n";
					});
				}

			}
			foreach (var type in module.MainModule.Types)
			{

				//UIManager.LogMessage($"Class: {type.Name}");
				Avalonia.Threading.Dispatcher.UIThread.Post(() =>
				{
					//_outputTextBlock.Text += $"Class: {type.Name}\n";
				});
				//foreach (var method in type.Methods)
				//{
				//    Console.WriteLine($" - Method: {method.Name}");
				//}
			}

			var processes = System.Diagnostics.Process.GetProcesses()
				.Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
				.ToList();

			foreach (var process in processes)
			{
				UIManager.LogMessage($"{process.Id} - {process.MainWindowTitle}");
			}
			*/
		}

		private async void AttachButtonPressed(object? sender, EventArgs e)
		{
			UIManager.LogMessage("Attach Button clicked!");
			if (this.mainWindow.StorageProvider != null) // Ensure StorageProvider is available
			{
				// Not sure if this will work on Linux or MacOS
				var files = await this.mainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
				{
					Title = "Select a Unity Game .EXE",
					AllowMultiple = false,
					FileTypeFilter = new[]
					{
					new FilePickerFileType("Executable Files")
					{
						Patterns = new[] { "*.exe" },
					},
					new FilePickerFileType("All Files")
					{
						Patterns = new[] { "*" },
					},
				},
				});

				// User selected a file
				if (files != null && files.Count > 0)
				{
					var selectedFile = files[0].Path.LocalPath;

					AttachedProcess.AttachToProcess(selectedFile);

					this.HandAttachErrors();

					if (AttachedProcess.IsAttached)
					{
						this.mainWindow.ProcessTextBlock.Text = $"Attached to: {AttachedProcess.ProcessName}";

						// put the dll in the plugins folder
					}
					else
					{
						this.mainWindow.ProcessTextBlock.Text = "Failed to attach";
					}

					// Console.WriteLine($"Selected file: {selectedFile}");
				}
			}
			else
			{
				UIManager.LogMessage("StorageProvider is not available.");
				ShowPopup("Error", "Could not start the file browser.", Icon.Error);
			}
		}

		private void HandAttachErrors()
		{
			if (AttachedProcess.Error)
			{
				ShowPopup("Error", "An error occured while trying to attach to the process.", Icon.Error);
			}
			else
			{
				// Different popups for different errors
				if (AttachedProcess.IsIL2CPP)
				{
					ShowPopup("Error", "IL2CPP is not currently supported, please select a non IL2CPP Unity Game.", Icon.Error);
				}
				else if (!AttachedProcess.HasBepInExInstalled)
				{
					ShowPopup("Error", "You should install BepInEx v6.0.0 before running this tool.", Icon.Error);
				}
			}
		}

		// Connect button event
		private void ConnectButtonPressed(object? sender, EventArgs e)
		{
			if (this.mainWindow.IpText.Text == null || this.mainWindow.PortText.Text == null)
			{
				return;
			}

			ConfigManager.SaveIPPort(this.mainWindow.IpText.Text, this.mainWindow.PortText.Text);
			Connection.AttemptConnection(this.mainWindow.IpText.Text, int.Parse(this.mainWindow.PortText.Text));
		}

		// Disconnect button event
		private void DisconnectButtonPressed(object? sender, EventArgs e)
		{
			Connection.Disconnect();
		}

		// Fetch hierarchy button event
		private void FetchHirarchyPressed(object? sender, EventArgs e)
		{
			Connection.FetchHierarchy();
		}

		private void SelectedHierachyNode(object? sender, EventArgs e)
		{
			if (this.mainWindow.HierarchyTreeView.SelectedItem is HierarchyNode selectedNode)
			{
				HierachyHandler.SelectedNode(selectedNode);
			}
		}
	}
}
