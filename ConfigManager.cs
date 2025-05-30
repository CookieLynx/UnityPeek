﻿namespace UnityPeek
{
	using UnityPeek.UI;
    using System.Diagnostics;

	public static class ConfigManager
	{
		public static string Port = "6500";
		public static string IP = "192.168.1.1";

		// Fetch the config directory which will be next to the dl
		public static string GetConfigDir()
		{
			string dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
			UIManager.LogMessage(dllPath);

			// Chance of Dllpath not existing
			string dllDirectory = System.IO.Path.GetDirectoryName(dllPath) ?? string.Empty;
			UIManager.LogMessage(dllDirectory);
			string configPath = System.IO.Path.Combine(dllDirectory, "UnityPeekConfig.txt");
			UIManager.LogMessage(configPath);
			return configPath;
		}

		/// <summary>
		/// Save IP and Port to the config.
		/// </summary>
		/// <param name="newIp">IP.</param>
		/// <param name="newPort">Port.</param>
		public static void SaveIPPort(string newIp, string newPort)
		{
			// Need a way better system for this
			if (newIp != null && newIp != string.Empty)
			{
				IP = newIp;
			}

			if (Port != null && Port != string.Empty)
			{
				Port = newPort;
			}

			string configFile =
				"## The last used ip used for connecting\n"
				+ "IP=" + IP + "\n"
				+ "## The last used port for connecting\n"
				+ "port=" + Port;

			System.IO.File.WriteAllText(GetConfigDir(), configFile);
		}

		public static void LoadConfig()
		{
			UIManager.LogMessage("Loading Config");
			string configPath = GetConfigDir();
			if (System.IO.File.Exists(configPath))
			{
				// Load the config by looping over all of the lines, ignoring ones with # and reading the key and value of the others
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
								UIManager.LogMessage("Setting port to " + value);
								Port = value;
								break;
							case "IP":
								UIManager.LogMessage("Setting IP to " + value);
								IP = value;
								break;
							default:
								UIManager.LogMessage("Unknown config key: " + key);
								break;
						}
					}
					else
					{
						UIManager.LogMessage("Invalid config line: " + line);
					}
				}
			}
			else
			{
				UIManager.LogMessage("Config not found, creating config");
				CreateConfig();
			}
		}

		// Creates a new config with default values
		public static void CreateConfig()
		{
			string configFile =
				"## The last used ip used for connecting\n"
				+ "IP=192.168.1.1\n"
				+ "## The last used port for connecting\n"
				+ "port=6500";

			System.IO.File.WriteAllText(GetConfigDir(), configFile);
			UIManager.LogMessage("Created Config");
		}
	}
}
