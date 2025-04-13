using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace UnityPeek
{
	[Serializable]
	//Helper class
	class Helpers
	{

		public static Vector3 GetQuaternionEulerAngle(Quaternion q)
		{
			float x = q.X;
			float y = q.Y;
			float z = q.Z;
			float w = q.W;

			float rollX = MathF.Atan2(2.0f * (w * x + y * z), 1.0f - 2.0f * (x * x + y * y));
			float pitchY = MathF.Asin(2.0f * (w * y - z * x));
			float yawZ = MathF.Atan2(2.0f * (w * z + x * y), 1.0f - 2.0f * (y * y + z * z));
			return new Vector3(rollX, pitchY, yawZ);

		}



		[Serializable]
		public class HierachyStructure
		{
			public string name;
			public string id;
			public int siblingIndex;
			public HierachyStructure parent;
			public List<HierachyStructure> children = new List<HierachyStructure>();
		}

		/// <summary>
		/// Recursively remove a node from the hierarchy by its ID.
		/// </summary>
		/// <param name="id">The ID to remove</param>
		/// <param name="rootNode">The root node to loop through</param>
		/// <returns></returns>
		public static bool RemoveNode(string id, HierachyStructure rootNode)
		{
			if (rootNode == null)
				return false;

			// Convert id to string for comparison
			string idStr = id;

			// Check if the root node is the one to be removed
			if (rootNode.id == idStr)
			{
				if (rootNode.parent != null)
				{
					rootNode.parent.children.Remove(rootNode);
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
				foreach (var child in currentNode.children)
				{
					if (child.id == idStr)
					{
						currentNode.children.Remove(child);
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
				name = reader.ReadString(),
				id = reader.ReadString(),
				siblingIndex = reader.ReadInt32(),
				children = new List<Helpers.HierachyStructure>()
			};

			int childCount = reader.ReadInt32(); // Read the number of children
			for (int i = 0; i < childCount; i++)
			{
				var child = ReadNode(reader);
				child.parent = node; // Set parent reference
				node.children.Add(child);
			}

			return node;
		}



	}
}
