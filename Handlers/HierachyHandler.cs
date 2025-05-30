﻿namespace UnityPeek.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
	using System.Threading;
	using Avalonia.Platform.Storage;
	using MsBox.Avalonia.Enums;
	using UnityPeek.UI;

	public class HierachyHandler
	{
		private static int selectedTransformID = -1;
		private static List<int> iDList = new List<int>();

		private static Helpers.HierachyStructure cachedRoot;

		// Update the visual of the hierrarchy and delete any duplicate nodes
		public static void UpdateHierarchy(Helpers.HierachyStructure root)
		{
			cachedRoot = root;
			iDList.Clear();
			int index = root.SiblingIndex!.Value;
			var rootNode = new HierarchyNode { Name = root.Name! };

			var sortedChildren = root.Children!.OrderBy(c => c.SiblingIndex).ToList();

			// Add sorted children to the root node
			foreach (var child in sortedChildren)
			{
				AddChildrenToHierachyNode(child, rootNode, root);
			}

			Avalonia.Threading.Dispatcher.UIThread.Post(() =>
			{
				// Set the items source for the tree view
				// UIManager.UpdateHierarchyView(rootNode);
			});

			// check if ids are unique
			var duplicates = iDList.GroupBy(x => x)
				.Where(g => g.Count() > 1)
				.Select(g => g.Key)
				.ToList();
			if (duplicates.Count > 0)
			{
				UIManager.LogMessage("Duplicate IDs found: " + string.Join(", ", duplicates));
			}
			else
			{
				UIManager.LogMessage("No duplicate IDs found.");
			}

			Avalonia.Threading.Dispatcher.UIThread.Post(() =>
			{
				UIManager.UpdateHierarchyView(rootNode);
			});
		}

		public static void SelectedNode(HierarchyNode selectedNode)
		{
			selectedTransformID = selectedNode.Id;
			InspectorHandler.selectedTransformID = selectedNode.Id;
			Connection.SendSelectedNode(selectedTransformID);
			UIManager.LogMessage($"Selected node: {selectedNode.Name}");
		}

		

		public static void GatherHierarchyChunkData(NetworkStream stream)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				UIManager.LogMessage("Reading Chunk");
				byte[] buffer2 = new byte[1024];
				int bytesRead2;

				// int tempCounter = 0;
				// Loop over each chunk in the stream and write to a memory stream
				while (stream.DataAvailable && (bytesRead2 = stream.Read(buffer2, 0, buffer2.Length)) > 0)
				{
					ms.Write(buffer2, 0, bytesRead2);

					// UIManager.LogMessage("Inside while loop " + tempCounter++);
				}

				Helpers.HierachyStructure root = DecodeRecivedHierarchyData(ms.ToArray());
				UpdateHierarchy(root);
				UIManager.LogMessage("Received: " + root.Name + " With " + root.Children?.Count + " children");
			}
		}

		private static void AddChildrenToHierachyNode(Helpers.HierachyStructure child, HierarchyNode parentNode, Helpers.HierachyStructure rootNode)
		{
			var childNode = new HierarchyNode { Name = child.Name! };

			// if child.id is already in idList, skip adding it
			if (iDList.Contains(int.Parse(child.ID!)))
			{
				UIManager.LogMessage("Duplicate ID found: " + child.ID);
				if (Helpers.RemoveNode(child.ID!, rootNode))
				{
					UIManager.LogMessage("Cleared the found duplicate node");
				}

				return;
			}

			iDList.Add(int.Parse(child.ID!));
			childNode.Id = int.Parse(child.ID!);
			parentNode.Children.Add(childNode);

			// Sort children by siblingIndex (lower siblingIndex comes first)
			var sortedChildren = child.Children!.OrderBy(c => c.SiblingIndex).ToList();

			foreach (var grandChild in sortedChildren)
			{
				AddChildrenToHierachyNode(grandChild, childNode, rootNode);
			}
		}

		/// <summary>
		/// Decodes array into HierachyStructure.
		/// </summary>
		/// <param name="array">Hierarchy data array.</param>
		/// <returns>Returns a HierachyStructure root node.</returns>
		private static Helpers.HierachyStructure DecodeRecivedHierarchyData(byte[] array)
		{
			UIManager.LogMessage("Decoding");
			Helpers.HierachyStructure root = Helpers.DeserializeHierarchy(array);
			CreateJsonFromHierachy(array);
			return root;
		}


		private static void CreateJsonFromHierachy(byte[] array)
		{
			string json = System.Text.Json.JsonSerializer.Serialize(array);
			string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Hierarchy.json");
			File.WriteAllText(filePath, json);
			UIManager.LogMessage("Hierarchy JSON saved to: " + filePath);
		}

		public static int timeBetweenFetch = 5000; // default 5 seconds
		private static Thread fetchThread;
		private static bool isRunning = false;

		public static void StartAutoFetch()
		{
			if (isRunning) return;

			isRunning = true;
			fetchThread = new Thread(() =>
			{
				while (isRunning)
				{
					FetchData(); // Your method to fetch something
					Thread.Sleep(timeBetweenFetch);
				}
			});

			fetchThread.IsBackground = true;
			fetchThread.Start();
		}

		public static void StopAutoFetch()
		{
			isRunning = false;
			if (fetchThread != null && fetchThread.IsAlive)
			{
				fetchThread.Join(); // Optional: waits for the thread to finish
			}
		}

		public static void SetAutoFetchTime(int time)
		{
			timeBetweenFetch = time * 1000;
		}

		private static void FetchData()
		{
			Connection.FetchHierarchy();
		}

		public static void SaveHierachy(string path)
		{
			// serialize the cached root
			string json = System.Text.Json.JsonSerializer.Serialize(Helpers.SerializeHierarchy(cachedRoot));

			// save the json to the path
			File.WriteAllText(path, json);
			UIManager.LogMessage("Hierarchy JSON saved to: " + path);
		}

	}
}
