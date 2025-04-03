using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPeek
{
    static class ConfigManager
    {

        public static string port = "6500";
        public static string IP = "192.168.1.1";


        //Fetch the config directory which will be next to the dl
        public static string getConfigDir()
        {
            string dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            Debug.WriteLine(dllPath);  
            //Chance of Dllpath not existing
            string dllDirectory = System.IO.Path.GetDirectoryName(dllPath) ?? string.Empty;
            Debug.WriteLine(dllDirectory);
            string configPath = System.IO.Path.Combine(dllDirectory, "UnityPeekConfig.txt");
            Debug.WriteLine(configPath);
            return configPath;
        }

        /// <summary>
        /// Save IP and Port to the config
        /// </summary>
        /// <param name="newIp">IP</param>
        /// <param name="newPort">Port</param>
        public static void SaveIPPort(string newIp, string newPort)
        {
            //Need a way better system for this
            if(newIp != null && newIp != "")
            {
                IP = newIp;
            }
            if (port != null && port != "")
            {
                port = newPort;
            }

            string configFile = "" +
                "## The last used ip used for connecting\n"
                + "IP=" + IP + "\n"
                + "## The last used port for connecting\n"
                + "port=" + port;

            System.IO.File.WriteAllText(getConfigDir(), configFile);

        }


        public static void LoadConfig()
        {
            Debug.WriteLine("Loading Config");
            string configPath = getConfigDir();
            if (System.IO.File.Exists(configPath))
            {
                //Load the config by looping over all of the lines, ignoring ones with # and reading the key and value of the others
                string[] lines = System.IO.File.ReadAllLines(configPath);
                foreach (string line in lines)
                {
                    if (line.StartsWith("#"))
                    {
                        continue;
                    }

                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        string key = parts[0];
                        string value = parts[1];
                        switch (key)
                        {
                            case "port":
                                Debug.WriteLine("Setting port to " + value);
                                port = value;
                                break;
                            case "IP":
                                Debug.WriteLine("Setting IP to " + value);
                                IP = value;
                                break;
                            default:
                                Debug.WriteLine("Unknown config key: " + key);
                                break;
                        }

                    }
                    else
                    {
                        Debug.WriteLine("Invalid config line: " + line);
                    }
                }
            }
            else
            {
                Debug.WriteLine("Config not found, creating config");
                CreateConfig();
            }


        }

        //Creates a new config with default values
        public static void CreateConfig()
        {
            string configFile = "" +
                "## The last used ip used for connecting\n"
                + "IP=192.168.1.1\n"
                + "## The last used port for connecting\n"
                + "port=6500";


            System.IO.File.WriteAllText(getConfigDir(), configFile);
            Debug.WriteLine("Created Config");

        }
    }
}
