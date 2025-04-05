using System;
using Avalonia.Controls;
using Debug = System.Diagnostics.Debug;
using Avalonia.Platform.Storage;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Collections;


namespace UnityPeek
{
    class UIManager
    {
        public static UIManager uiManager;

        private MainWindow mainWindow;

        //Constructor to get all of the button methods and the mainWindow itself
        public UIManager(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            mainWindow.AttachButtonClicked += AttachButtonPressed;
            mainWindow.ConnectButtonClicked += ConnectButtonPressed;
            mainWindow.DisconnectButtonClicked += DisconnectButtonPressed;
            mainWindow.FetchHirarchyClicked += FetchHirarchyPressed;
            uiManager = this;
        }




        private async void AttachButtonPressed(object? sender, EventArgs e)
        {
            Debug.WriteLine("Attach Button clicked!");
            if (mainWindow.StorageProvider != null) // Ensure StorageProvider is available
            {
                //Not sure if this will work on Linux or MacOS
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

                //User selected a file
                if (files != null && files.Count > 0)
                {
                    var selectedFile = files[0].Path.LocalPath;


                    AttachedProcess.AttachToProcess(selectedFile);


                    HandAttachErrors();

                    if (AttachedProcess.IsAttached)
                    {
                        mainWindow.ProcessTextBlock.Text = $"Attached to: {AttachedProcess.ProcessName}";

                        //put the dll in the plugins folder
                    }
                    else
                    {

                        mainWindow.ProcessTextBlock.Text = "Failed to attach";

                    }

                    //Console.WriteLine($"Selected file: {selectedFile}");
                }
            }
            else
            {
                Debug.WriteLine("StorageProvider is not available.");
                ShowPopup("Error", "Could not start the file browser.", Icon.Error);
            }
        }




        /// <summary>
        /// Show a popup error message
        /// </summary>
        /// <param name="title">Error box tittle</param>
        /// <param name="message">Error box message</param>
        public static async void ShowPopup(string title, string message, Icon icon)
        {
            var messageBox = MessageBoxManager
                .GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);

            await messageBox.ShowAsync();
        }



        private void HandAttachErrors()
        {
            if (AttachedProcess.Error)
            {
                ShowPopup("Error", "An error occured while trying to attach to the process.", Icon.Error);
            }
            else
            {


                //Different popups for different errors
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


        //Update the connection text
        public static void ChangeConnectedText(bool connected)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                uiManager.mainWindow.connectedText.Text = connected ? "Connected" : "Disconnected";
            });
        }



        //Connect button event
        private void ConnectButtonPressed(object? sender, EventArgs e)
        {
            if(mainWindow.IpText.Text == null || mainWindow.PortText.Text == null)
            {
                return;
            }
            ConfigManager.SaveIPPort(mainWindow.IpText.Text, mainWindow.PortText.Text);
            Connection.AttemptConnection(mainWindow.IpText.Text, Int32.Parse(mainWindow.PortText.Text));
        }

        //Disconnect button event
        private void DisconnectButtonPressed(object? sender, EventArgs e)
        {
            Connection.Disconnect();
        }

        //Fetch hierarchy button event
        private void FetchHirarchyPressed(object? sender, EventArgs e)
        {
            Connection.FetchHierarchy();
        }


        private static List<int> idList = new List<int>();



        private static void AddChildrenToHierachyNode(Helpers.HierachyStructure child, HierarchyNode parentNode)
        {

            var childNode = new HierarchyNode { Name = child.name };
            idList.Add(Int32.Parse(child.id));
            parentNode.Children.Add(childNode);
            
            // Sort children by siblingIndex (lower siblingIndex comes first)
            var sortedChildren = child.children.OrderBy(c => c.siblingIndex).ToList();
            
            foreach (var grandChild in sortedChildren)
            {
                AddChildrenToHierachyNode(grandChild, childNode);
            }
        }


        public static void UpdateHierarchy(Helpers.HierachyStructure root)
        {
            int index = root.siblingIndex;

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var rootNode = new HierarchyNode { Name = root.name };

                var sortedChildren = root.children.OrderBy(c => c.siblingIndex).ToList();

                // Add sorted children to the root node
                foreach (var child in sortedChildren)
                {
                    AddChildrenToHierachyNode(child, rootNode);
                }

                // Set the items source for the tree view
                uiManager.mainWindow.HierarchyTreeView.ItemsSource = new List<HierarchyNode> { rootNode };
            });


            //check if ids are unique
            var duplicates = idList.GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            if (duplicates.Count > 0)
            {
                Debug.WriteLine("Duplicate IDs found: " + string.Join(", ", duplicates));
            }
            else
            {
                Debug.WriteLine("No duplicate IDs found.");
            }
        }



        public void Start()
        {
            //Load or create config
            ConfigManager.LoadConfig();
            Debug.WriteLine("App has booted successfully!");

            //Set the values inside the window
            mainWindow.IpText.Text = ConfigManager.IP;
            mainWindow.PortText.Text = ConfigManager.port;



            


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
