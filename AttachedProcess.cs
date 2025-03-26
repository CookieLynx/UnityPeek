using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPeek
{
    class AttachedProcess
    {
        public static string ProcessName { get; private set; } = ""; //Does not inlcude the .exe extension
        public static string ProcessRootPath { get; private set; } = "";
        public static string ProcessManagedPath { get; private set; } = "";
        public static string ProcessPluginsPath { get; private set; } = "";

        public static bool IsAttached => !string.IsNullOrEmpty(ProcessName) && !string.IsNullOrEmpty(ProcessManagedPath) && !string.IsNullOrEmpty(ProcessPluginsPath);
        public static bool IsIL2CPP { get; private set; } = false;
        public static bool HasBepInExInstalled { get; private set; } = false; 

        public static bool Error { get; private set; } = false;
        public static void AttachToProcess(string filePath)
        {
            //Reset the paths
            Error = false;
            ProcessName = string.Empty;
            ProcessRootPath = string.Empty;
            ProcessManagedPath = string.Empty;
            ProcessPluginsPath = string.Empty;
            IsIL2CPP = false;
            HasBepInExInstalled = false;


            try
            {
                if (!System.IO.File.Exists(filePath))
                {
                    Error = true;
                    return;
                }
                ProcessName = filePath.Substring(filePath.LastIndexOf("\\") + 1, filePath.Length - filePath.LastIndexOf("\\") - 5); //Get the name of the process without the .exe extension

                ProcessRootPath = System.IO.Directory.GetParent(filePath).ToString();
                if (ProcessRootPath == null)
                {
                    Error = true;
                    return; //Error
                }

                //Check if the process is a Unity game
                string DataFolder = $@"{ProcessRootPath}\{ProcessName + "_Data"}\";
                if (!System.IO.Directory.Exists(DataFolder))
                {
                    Error = true;
                    Debug.WriteLine("Folder does not exist");
                    return; //Not a unity game
                }

                //Check if the process is using IL2CPP
                if (System.IO.Directory.Exists($@"{DataFolder}\il2cpp_data"))
                {
                    Debug.WriteLine("IS IL2CPP");
                    IsIL2CPP = true; //We dont currently support IL2CPP
                    return;
                }


                ProcessManagedPath = $@"{ProcessRootPath}\{ProcessName + "_Data"}\Managed\"; //The managed folder will not exist if the process is not a Unity game or is using IL2CPP

                if (System.IO.Directory.Exists($@"{ProcessRootPath}\BepInEx"))
                {
                    HasBepInExInstalled = true;
                    if (!System.IO.Directory.Exists($@"{ProcessRootPath}\BepInEx\plugins"))
                    {
                        System.IO.Directory.CreateDirectory($@"{ProcessRootPath}\BepInEx\plugins");
                    }
                    ProcessPluginsPath = $@"{ProcessRootPath}\BepInEx\plugins\"; //Path may not exist if the user has not ran the game with BepInEx installed before
                }

            }
            catch (Exception e)
            {
                Error = true;
                Debug.WriteLine(e.Message);



            }
        }

    }
}
