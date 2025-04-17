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
			Vector3 angles = new Vector3();

			// Roll (X-axis rotation)
			double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
			double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
			angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

			// Pitch (Y-axis rotation)
			double sinp = 2 * (q.W * q.Y - q.Z * q.X);
			if (Math.Abs(sinp) >= 1)
				angles.Y = (float)(Math.CopySign(Math.PI / 2, sinp)); // use 90 degrees if out of range
			else
				angles.Y = (float)Math.Asin(sinp);

			// Yaw (Z-axis rotation)
			double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
			double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
			angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

			// Convert from radians to degrees
			angles.X = ToDegrees(angles.X);
			angles.Y = ToDegrees(angles.Y);
			angles.Z = ToDegrees(angles.Z);

			return angles;

		}

		public static float ToDegrees(float radians) => radians * (180f / (float)Math.PI);
		public static float ToDegrees(double radians) => (float)(radians * (180.0 / Math.PI));



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
