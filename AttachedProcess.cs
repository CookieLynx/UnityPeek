namespace UnityPeek
{
    using System;
    using System.Diagnostics;
	using System.Net.Http;
	using System.Text.Json;
	using UnityPeek.UI;

	public class AttachedProcess
	{
		public static string ProcessName { get; private set; } = string.Empty; // Does not inlcude the .exe extension

		public static string ProcessRootPath { get; private set; } = string.Empty;

		public static string ProcessManagedPath { get; private set; } = string.Empty;

		public static string ProcessPluginsPath { get; private set; } = string.Empty;

		public static bool IsAttached => !string.IsNullOrEmpty(ProcessName) && !string.IsNullOrEmpty(ProcessManagedPath) && !string.IsNullOrEmpty(ProcessPluginsPath);

		public static bool IsIL2CPP { get; private set; } = false;

		public static bool HasBepInExInstalled { get; private set; } = false;

		public static bool Error { get; private set; } = false;

		public static string currentlyInstalledVersion = string.Empty;

		public static void AttachToProcess(string filePath)
		{
			// Reset the paths
			Error = false;
			ProcessName = string.Empty;
			ProcessRootPath = string.Empty;
			ProcessManagedPath = string.Empty;
			ProcessPluginsPath = string.Empty;
			IsIL2CPP = false;
			HasBepInExInstalled = false;

			try
			{
				// Check if the file exists
				if (!System.IO.File.Exists(filePath))
				{
					Error = true;
					return;
				}

				// Get the name of the process without the .exe extension
				ProcessName = filePath.Substring(filePath.LastIndexOf("\\") + 1, filePath.Length - filePath.LastIndexOf("\\") - 5);

				// Get the process root path
				if (System.IO.Directory.GetParent(filePath) == null)
				{
					return;
				}

				ProcessRootPath = System.IO.Directory.GetParent(filePath) !.ToString();
				if (ProcessRootPath == null)
				{
					Error = true;
					return; // Error somehow the directory does not exist
				}

				// Check if the process is a Unity game
				string dataFolder = $@"{ProcessRootPath}\{ProcessName + "_Data"}\";
				if (!System.IO.Directory.Exists(dataFolder))
				{
					Error = true;
					UIManager.LogMessage("Folder does not exist");
					return; // Not a unity game since there is no Data folder
				}

				// Check if the process is using IL2CPP
				if (System.IO.Directory.Exists($@"{dataFolder}\il2cpp_data"))
				{
					UIManager.LogMessage("IS IL2CPP");
					IsIL2CPP = true; // We dont currently support IL2CPP
					return;
				}

				// The managed folder will not exist if the process is not a Unity game or is using IL2CPP
				ProcessManagedPath = $@"{ProcessRootPath}\{ProcessName + "_Data"}\Managed\";

				if (System.IO.Directory.Exists($@"{ProcessRootPath}\BepInEx"))
				{
					HasBepInExInstalled = true;
					if (!System.IO.Directory.Exists($@"{ProcessRootPath}\BepInEx\plugins"))
					{
						System.IO.Directory.CreateDirectory($@"{ProcessRootPath}\BepInEx\plugins");
					}

					ProcessPluginsPath = $@"{ProcessRootPath}\BepInEx\plugins\"; // Path may not exist if the user has not ran the game with BepInEx installed before

					//Check if unitypeekplugin.dll exists
					if (System.IO.File.Exists($@"{ProcessPluginsPath}\UnityPeekPlugin.dll"))
					{
						UIManager.LogMessage("here");
						//User might have an old version of the plugin
						string versionString = System.Diagnostics.FileVersionInfo.GetVersionInfo($@"{ProcessPluginsPath}\UnityPeekPlugin.dll").FileVersion;
						if (versionString != null)
						{
							UIManager.LogMessage(versionString);
							currentlyInstalledVersion = versionString;
							CheckGithubVersion();
						}
					}
				}
			}
			catch (Exception e)
			{
				Error = true;
				UIManager.LogMessage(e.Message);
			}
		}

		private static void CheckGithubVersion()
		{
			string latestVersion = string.Empty;
			string url = "https://api.github.com/repos/UnityPeek/UnityPeekPlugin/releases/latest";

			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

				var response = client.GetAsync(url).Result;
				if (response.IsSuccessStatusCode)
				{
					var json = response.Content.ReadAsStringAsync().Result;

					try
					{
						// No way to test this currently without a github release, will come back
						using (JsonDocument doc = JsonDocument.Parse(json))
						{
							JsonElement root = doc.RootElement;
							if (root.TryGetProperty("tag_name", out JsonElement tagElement))
							{
								latestVersion = tagElement.GetString();
								if (Version.TryParse(latestVersion.TrimStart('v'), out Version version)) // trim 'v' if version is like "v1.0"
								{
									UIManager.LogMessage("Github fetch was good");
									/*
									if (version > currentlyInstalledVersion)
									{
										UIManager.LogMessage("A new version of the plugin is available: " + latestVersion);
									}
									else
									{
										UIManager.LogMessage("No newer version of UnityPeekPlugin was found");
									}
									*/
								}
							}
						}
					}
					catch (JsonException ex)
					{
						UIManager.LogMessage("Failed to parse version info: " + ex.Message, UIManager.LogType.Error);
					}
				}
				else
				{
					UIManager.LogMessage("Failed to fetch the latest version from GitHub", UIManager.LogType.Error);
				}
			}
		}
	}
}
