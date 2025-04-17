using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using UnityPeek.UI;

namespace UnityPeek.Handlers
{
	class HierachyHandler
	{
		private static int selectedTransformID = -1;



		/// <summary>
		/// Decodes array into HierachyStructure
		/// </summary>
		/// <param name="array">Hierarchy data array</param>
		/// <returns></returns>
		private static Helpers.HierachyStructure DecodeRecivedHierarchyData(byte[] array)
		{
			Debug.WriteLine("Decoding");
			Helpers.HierachyStructure root = Helpers.DeserializeHierarchy(array);
			return root;
		}


		public static void GatherHierarchyChunkData(NetworkStream stream)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Debug.WriteLine("Reading Chunk");
				byte[] buffer2 = new byte[1024];
				int bytesRead2;
				//int tempCounter = 0;

				//Loop over each chunk in the stream and write to a memory stream
				while (stream.DataAvailable && (bytesRead2 = stream.Read(buffer2, 0, buffer2.Length)) > 0)
				{
					ms.Write(buffer2, 0, bytesRead2);
					//Debug.WriteLine("Inside while loop " + tempCounter++);
				}
				Helpers.HierachyStructure root = DecodeRecivedHierarchyData(ms.ToArray());
				UpdateHierarchy(root);
				Debug.WriteLine("Received: " + root.name + " With " + root.children.Count + " children");

			}
		}

		private static void AddChildrenToHierachyNode(Helpers.HierachyStructure child, HierarchyNode parentNode, Helpers.HierachyStructure rootNode)
		{

			var childNode = new HierarchyNode { Name = child.name };
			//if child.id is already in idList, skip adding it

			if (IDList.Contains(int.Parse(child.id)))
			{
				Debug.WriteLine("Duplicate ID found: " + child.id);
				if (Helpers.RemoveNode(child.id, rootNode))
				{
					Debug.WriteLine("Cleared the found duplicate node");
				}
				return;
			}

			IDList.Add(int.Parse(child.id));
			childNode.Id = int.Parse(child.id);
			parentNode.Children.Add(childNode);



			// Sort children by siblingIndex (lower siblingIndex comes first)
			var sortedChildren = child.children.OrderBy(c => c.siblingIndex).ToList();

			foreach (var grandChild in sortedChildren)
			{
				AddChildrenToHierachyNode(grandChild, childNode, rootNode);
			}

		}







		private static List<int> IDList = new List<int>();
		//Update the visual of the hierrarchy and delete any duplicate nodes
		public static void UpdateHierarchy(Helpers.HierachyStructure root)
		{
			IDList.Clear();
			int index = root.siblingIndex;
			var rootNode = new HierarchyNode { Name = root.name };

			var sortedChildren = root.children.OrderBy(c => c.siblingIndex).ToList();

			// Add sorted children to the root node
			foreach (var child in sortedChildren)
			{
				AddChildrenToHierachyNode(child, rootNode, root);
			}

			Avalonia.Threading.Dispatcher.UIThread.Post(() =>
			{



				// Set the items source for the tree view
				//UIManager.UpdateHierarchyView(rootNode);
			});


			//check if ids are unique
			var duplicates = IDList.GroupBy(x => x)
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

			Avalonia.Threading.Dispatcher.UIThread.Post(() =>
			{
				UIManager.UpdateHierarchyView(rootNode);
			});
		}


		public static void SelectedNode(HierarchyNode selectedNode)
		{
			selectedTransformID = selectedNode.Id;
			Connection.SendSelectedNode(selectedTransformID);
			Debug.WriteLine($"Selected node: {selectedNode.Name}");
		}


		public static void SendToggleTransformActive(bool isActive)
		{
			Connection.SendToggleTransformActive(selectedTransformID, isActive);
		}
	}
}
