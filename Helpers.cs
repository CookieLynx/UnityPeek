namespace UnityPeek
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Numerics;
    using System.Text;

	[Serializable]

	// Helper class
	public class Helpers
	{
		public static Vector3 GetQuaternionEulerAngle(Quaternion q)
		{
			Vector3 angles = new Vector3();

			// Roll (X-axis rotation)
			double sinr_cosp = 2 * ((q.W * q.X) + (q.Y * q.Z));
			double cosr_cosp = 1 - (2 * ((q.X * q.X) + (q.Y * q.Y)));
			angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

			// Pitch (Y-axis rotation)
			double sinp = 2 * ((q.W * q.Y) - (q.Z * q.X));
			if (Math.Abs(sinp) >= 1)
			{
				angles.Y = (float)Math.CopySign(Math.PI / 2, sinp); // use 90 degrees if out of range
			}
			else
			{
				angles.Y = (float)Math.Asin(sinp);
			}

			// Yaw (Z-axis rotation)
			double siny_cosp = 2 * ((q.W * q.Z) + (q.X * q.Y));
			double cosy_cosp = 1 - (2 * ((q.Y * q.Y) + (q.Z * q.Z)));
			angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

			// Convert from radians to degrees
			angles.X = ToDegrees(angles.X);
			angles.Y = ToDegrees(angles.Y);
			angles.Z = ToDegrees(angles.Z);

			return angles;
		}

		public static float ToDegrees(float radians) => radians * (180f / (float)Math.PI);

		public static float ToDegrees(double radians) => (float)(radians * (180.0 / Math.PI));

		/// <summary>
		/// Recursively remove a node from the hierarchy by its ID.
		/// </summary>
		/// <param name="id">The ID to remove.</param>
		/// <param name="rootNode">The root node to loop through.</param>
		/// <returns>True if removed.</returns>
		public static bool RemoveNode(string id, HierachyStructure rootNode)
		{
			if (rootNode == null)
			{
				return false;
			}

			// Convert id to string for comparison
			string idStr = id;

			// Check if the root node is the one to be removed
			if (rootNode.ID == idStr)
			{
				if (rootNode.Parent != null)
				{
					rootNode.Parent.Children?.Remove(rootNode);
					return true;
				}

				return false; // Cannot remove root node if it has no parent
			}

			// Use a queue for breadth-first search
			Queue<HierachyStructure> queue = new Queue<HierachyStructure>();
			queue.Enqueue(rootNode);

			while (queue.Count > 0)
			{
				var currentNode = queue.Dequeue();

				// Check all children of the current node
				foreach (var child in currentNode.Children!)
				{
					if (child.ID == idStr)
					{
						currentNode.Children.Remove(child);
						return true;
					}

					queue.Enqueue(child);
				}
			}

			return false; // Node with the given id was not found
		}

		public static HierachyStructure DeserializeHierarchy(byte[] data)
		{
			using (MemoryStream ms = new MemoryStream(data))
			using (BinaryReader reader = new BinaryReader(ms, Encoding.UTF8))
			{
				return ReadNode(reader);
			}
		}

		private static HierachyStructure ReadNode(BinaryReader reader)
		{
			HierachyStructure node = new Helpers.HierachyStructure
			{
				Name = reader.ReadString(),
				ID = reader.ReadString(),
				SiblingIndex = reader.ReadInt32(),
				Children = new List<Helpers.HierachyStructure>(),
			};

			int childCount = reader.ReadInt32(); // Read the number of children
			for (int i = 0; i < childCount; i++)
			{
				var child = ReadNode(reader);
				child.Parent = node; // Set parent reference
				node.Children.Add(child);
			}

			return node;
		}

		public static byte[] SerializeHierarchy(Helpers.HierachyStructure root)
		{
			using (MemoryStream ms = new MemoryStream())
			using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8))
			{
				// Plugin.Logger.LogInfo("Using BinaryWriter");
				WriteNode(writer, root);
				return ms.ToArray();
			}
		}
		private static void WriteNode(BinaryWriter writer, Helpers.HierachyStructure node)
		{
			try
			{
				// Plugin.Logger.LogInfo("Writing Node!" + node.name);
				writer.Write(node.Name);

				// Plugin.Logger.LogInfo("Writing Node Name!");
				writer.Write(node.ID);

				// Plugin.Logger.LogInfo("Writing Node ID!");
				writer.Write(node.SiblingIndex.Value);

				writer.Write(node.Children.Count); // Write number of children

				// Plugin.Logger.LogInfo("Writing Child Count");
				foreach (var child in node.Children)
				{
					// Plugin.Logger.LogInfo("Writing Node Child!");
					WriteNode(writer, child); // Recursively write child nodes
				}

				// Plugin.Logger.LogInfo("Done with those nodes");
			}
			catch (Exception e)
			{
				
			}
		}

		[Serializable]
		public class HierachyStructure
		{
			public string? Name;
			public string? ID;
			public int? SiblingIndex;
			public HierachyStructure? Parent;
			public List<HierachyStructure>? Children = new List<HierachyStructure>();
		}
	}
}
