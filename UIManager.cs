using System;
using Avalonia.Controls;
using Debug = System.Diagnostics.Debug;
using Avalonia.Platform.Storage;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;


namespace UnityPeek
{
    class UIManager
    {
        private TextBlock _statusTextBlock;
        private MainWindow mainWindow;
        public UIManager(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            mainWindow.AttachButtonClicked += AttachButtonPressed;
            _statusTextBlock = mainWindow.ProcessTextBlock;
        }




        private async void AttachButtonPressed(object? sender, EventArgs e)
        {
            Debug.WriteLine("Attach Button clicked!");
            if (mainWindow.StorageProvider != null) // Ensure StorageProvider is available
            {
                var files = await mainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select a Unity Game .EXE",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                    new FilePickerFileType("Executable Files")
                    {
                        Patterns = new[] { "*.exe" }
                    },
                    new FilePickerFileType("All Files")
                    {
                        Patterns = new[] { "*" }
                    }
                }
                });

                if (files != null && files.Count > 0)
                {
                    var selectedFile = files[0].Path.LocalPath;
                    AttachedProcess.AttachToProcess(selectedFile);
                    HandAttachErrors();

                    if (AttachedProcess.IsAttached)
                    {
                        _statusTextBlock.Text = $"Attached to: {AttachedProcess.ProcessName}";
                    }
                    else
                    {

                        _statusTextBlock.Text = "Failed to attach";

                    }

                    //Console.WriteLine($"Selected file: {selectedFile}");
                }
            }
            else
            {
                Debug.WriteLine("StorageProvider is not available.");
                ShowErrorPopup("Error", "Could not start the file browser.");
            }
        }





        private async void ShowErrorPopup(string title, string message)
        {
            var messageBox = MessageBoxManager
                .GetMessageBoxStandard(title, message, ButtonEnum.Ok, Icon.Error);

            await messageBox.ShowAsync();
        }

        private void HandAttachErrors()
        {
            if (AttachedProcess.Error)
            {
                ShowErrorPopup("Error", "An error occured while trying to attach to the process.");
            }
            else
            {


                //Different popups for different errors
                if (AttachedProcess.IsIL2CPP)
                {
                    ShowErrorPopup("Error", "IL2CPP is not currently supported, please select a non IL2CPP Unity Game.");
                }
                else if (!AttachedProcess.HasBepInExInstalled)
                {
                    ShowErrorPopup("Error", "You should install BepInEx v6.0.0 before running this tool.");
                }
            }
        }

        





        public void Start()
        {

            Debug.WriteLine("App has booted successfully!");
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
            Debug.WriteLine($"Type: {typeName}");

            foreach (var handle in reader.MethodDefinitions)
            {
                var method = reader.GetMethodDefinition(handle);
                var name = reader.GetString(method.Name);

                // Ensure this method belongs to the target type
                if (method.GetDeclaringType() == reader.TypeDefinitions.ElementAt(20))
                {
                    Debug.WriteLine($"  Method: {name}");

                    try
                    {
                        // Decompile using the EntityHandle
                        string code = decompiler.DecompileAsString(handle);
                        Debug.WriteLine($"Decompiled Code:\n{code}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error decompiling {name}: {ex.Message}");
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
                
                //Debug.WriteLine($"Class: {type.Name}");
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
                Debug.WriteLine($"{process.Id} - {process.MainWindowTitle}");
            }
            */






        }
    }
}
