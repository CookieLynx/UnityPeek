namespace UnityPeek.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;
    using UnityPeek.UI;

    public class InspectorHandler
    {
		public static void ReadTransformData(NetworkStream stream)
		{
			// Temporary memory stream to hold incoming data
			using (MemoryStream memoryStream = new MemoryStream())
			{
				byte[] buffer = new byte[1024]; // Can increase if needed
				int bytesRead;

				// Read all available data from the stream
				do
				{
					bytesRead = stream.Read(buffer, 0, buffer.Length);
					memoryStream.Write(buffer, 0, bytesRead);
				}
				while (stream.DataAvailable); // Continue until all data has been read

				memoryStream.Position = 0; // Reset stream position to the beginning

				using (BinaryReader reader = new BinaryReader(memoryStream))
				{
					try
					{
						// Read the string (automatically reads the length prefix too)
						string name = reader.ReadString();

						// Read active state
						bool isActive = reader.ReadBoolean();

						// Read position
						float posX = reader.ReadSingle();
						float posY = reader.ReadSingle();
						float posZ = reader.ReadSingle();

						// Read rotation
						float rotX = reader.ReadSingle();
						float rotY = reader.ReadSingle();
						float rotZ = reader.ReadSingle();
						float rotW = reader.ReadSingle();

						// Read scale
						float scaleX = reader.ReadSingle();
						float scaleY = reader.ReadSingle();
						float scaleZ = reader.ReadSingle();

						// Debug output
						UIManager.LogMessage($"Name: {name}, Active: {isActive}");
						UIManager.LogMessage($"Position: ({posX}, {posY}, {posZ})");
						UIManager.LogMessage($"Rotation: ({rotX}, {rotY}, {rotZ}, {rotW})");
						UIManager.LogMessage($"Scale: ({scaleX}, {scaleY}, {scaleZ})");

						// Update UI
						UIManager.UpdateSelectedNodeTransform(
							name,
							isActive,
							new System.Numerics.Vector3(posX, posY, posZ),
							new System.Numerics.Quaternion(rotX, rotY, rotZ, rotW),
							new System.Numerics.Vector3(scaleX, scaleY, scaleZ));
					}
					catch (EndOfStreamException)
					{
						UIManager.LogMessage("Error: Incomplete or corrupted transform data.");
					}
				}
			}
		}

		public static void EnabledCheckedBoxChanged(bool isEnabled)
		{
			HierachyHandler.SendToggleTransformActive(isEnabled);
		}
	}
}
