namespace UnityPeek
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Sockets;
    using System.Numerics;
    using System.Text;
    using System.Threading;
    using MsBox.Avalonia.Enums;
    using UnityPeek.Handlers;
    using UnityPeek.UI;

	public class Connection
	{
		public static bool IsConnected = false;

		private static string ip = string.Empty;
		private static int port = 0;
		private static Thread? clientThread;
		private static bool isServerRunning = true;
		private static string messageToSend = "Hello from External Program!";

		public static void AttemptConnection(string newIP, int newPort)
		{
			if (IsConnected)
			{
				return;
			}

			// check if port and ip is valid (not empty, has ip and port format
			// attempt to connect
			if (newIP == string.Empty || newPort < 1)
			{
				UIManager.ShowPopup("Error Connecting", "Your IP or port is invalid, please check them again", Icon.Error);
				UIManager.LogMessage("Invalid IP or Port");
				return;
			}

			// does the ip follow an ip format
			if (!System.Net.IPAddress.TryParse(newIP, out _))
			{
				UIManager.ShowPopup("Error Connecting", "Your IP is not in a valid format, please check it again", Icon.Error);
				UIManager.LogMessage("Invalid IP");
				return;
			}

			ip = newIP;
			port = newPort;

			if (clientThread != null)
			{
				isServerRunning = false;
				Disconnect();
			}

			// Start the server thread
			Thread newClientThread = new Thread(StartClient);

			clientThread = newClientThread;
			clientThread.IsBackground = true;
			isServerRunning = true;
			clientThread.Start();
		}

		public static void Disconnect()
		{
			messageToSend = string.Empty;
			isServerRunning = false;
			UIManager.ChangeConnectedText(false);
		}

		public static void FetchHierarchy()
		{
			if (IsConnected)
			{
				messageToSend = "FetchHierarchy";
			}
			else
			{
				UIManager.ShowPopup("Error", "You are not connected to a game", Icon.Error);
			}
		}

		public static void SendSelectedNode(int id)
		{
			if (IsConnected)
			{
				messageToSend = "SelectedNode:" + id;
			}
			else
			{
				// I dont think this can trigger but just in case
				UIManager.ShowPopup("Error", "You are not connected to a game", Icon.Error);
			}
		}

		public static void SendToggleTransformActive(int id, bool enabled)
		{
			messageToSend = "ToggleTransformActive:" + id + ":" + enabled;
		}

		public static void DeleteSelectedNode(int id)
		{
			messageToSend = "DeleteSelectedNode:" + id;
		}

		private static void StartClient()
		{
			// check if we have disconnected from the server
			try
			{
				using (TcpClient client = new TcpClient(ip, port))
				{
					// Inital connect
					UIManager.LogMessage("Connected to server!");
					UIManager.ChangeConnectedText(true);

					NetworkStream stream = client.GetStream();

					// Start receiving messages in a loop
					byte[] buffer = new byte[1024];

					// Check if the server is running, if not the thread will die
					while (isServerRunning)
					{
						IsConnected = true;
						SendData(stream, messageToSend);

						if (client.Client.Poll(0, SelectMode.SelectRead) && client.Client.Available == 0)
						{
							UIManager.LogMessage("Server closed connection.");
							Disconnect();
							break;
						}

						if (stream.DataAvailable)
						{
							UIManager.LogMessage("Data Available");

							var (typeId, messageLength) = ReadMessageLengthAndType(stream);
							if (typeId == -1 || messageLength == -1)
							{
								continue;
							}

							// Get the message type
							// int typeId = ReadMessageType(stream);
							UIManager.LogMessage("OUR TYPE ID: " + typeId);

							if (typeId == -1)
							{
								break;
							}

							// Hierarchy type
							if (typeId == 1)
							{
								UIManager.LogMessage("Fetch Hierarchy Data");
								HierachyHandler.GatherHierarchyChunkData(stream);
							}
							else if (typeId == 3)
							{
								InspectorHandler.ReadTransformData(stream);
							}
							else
							{
								ReadStringData(stream, buffer);
							}
						}

						Thread.Sleep(10); // Prevent high CPU usage
					}

					// Connection has dropped
					Disconnect();
					UIManager.ChangeConnectedText(false);
					IsConnected = false;
				}
			}
			catch (Exception ex)
			{
				Avalonia.Threading.Dispatcher.UIThread.Post(() =>
				{
					UIManager.ShowPopup("Error Connecting", ex.Message, Icon.Error);
				});
				IsConnected = false;
				Disconnect();
				UIManager.LogMessage("Error: " + ex.Message);
			}
		}

		/// <summary>
		/// Read data of string type.
		/// </summary>
		/// <param name="stream">Network Stream.</param>
		/// <param name="buffer">Buffer to read from.</param>
		private static void ReadStringData(NetworkStream stream, byte[] buffer)
		{
			int bytesRead = stream.Read(buffer, 0, buffer.Length);
			if (bytesRead > 0)
			{
				UIManager.LogMessage("Received: " + DecodeRecivedStringData(buffer, bytesRead));
			}
		}

		/*
		/// <summary>
		/// Reads the first 4 bytes of a message to get type ID (0 for string, 1 for Hierarchy data)
		/// </summary>
		/// <param name="stream">Network Stream</param>
		/// <returns></returns>
		private static int ReadMessageType(NetworkStream stream)
		{
			UIManager.LogMessage("HERE 1");
			byte[] typeBuffer = new byte[4]; // First 4 bytes = type ID
			UIManager.LogMessage("HERE 2");
			int output = stream.Read(typeBuffer, 0, typeBuffer.Length);
			UIManager.LogMessage("HERE 3");

			if (output <= 0) return -1;
			UIManager.LogMessage("HERE 4");

			int typeId = BitConverter.ToInt32(typeBuffer, 0);
			UIManager.LogMessage("HERE 5");
			return typeId;
		}
		*/

		/// <summary>
		/// Reads the first 4 bytes of a message to get its length not including those 4 bytes.
		/// </summary>
		/// <param name="stream">Network Stream.</param>
		/// <returns>an int for the message type and length.</returns>
		private static (int typeId, int messageLength) ReadMessageLengthAndType(NetworkStream stream)
		{
			byte[] headerBuffer = new byte[8];

			int totalRead = 0;
			while (totalRead < 8)
			{
				UIManager.LogMessage("HERE TOTAL READ: " + totalRead);
				int bytesRead = stream.Read(headerBuffer, totalRead, 8 - totalRead);
				if (bytesRead == 0)
				{
					return (-1, -1); // Stream closed
				}

				totalRead += bytesRead;
			}

			int length = BitConverter.ToInt32(headerBuffer, 0);
			int typeId = BitConverter.ToInt32(headerBuffer, 4);

			if (length < 4)
			{
				return (-1, -1);
			}

			return (typeId, length);
		}

		/// <summary>
		/// Send data over a stream.
		/// </summary>
		/// <param name="stream">The stream to send on.</param>
		/// <param name="message">The message to send, will not send empty messages.</param>
		private static void SendData(NetworkStream stream, string message)
		{
			if (message != string.Empty)
			{
				byte[] data = Encoding.ASCII.GetBytes(message);
				stream.Write(data, 0, data.Length);
				messageToSend = string.Empty;
				UIManager.LogMessage("Sent: " + message);
			}
		}

		/// <summary>
		/// Decodes recived string data.
		/// </summary>
		/// <param name="buffer">Data buffer.</param>
		/// <param name="bytesRead">The amount of bytes to decode.</param>
		private static string DecodeRecivedStringData(byte[] buffer, int bytesRead)
		{
			return Encoding.ASCII.GetString(buffer, 0, bytesRead);
		}
	}
}
