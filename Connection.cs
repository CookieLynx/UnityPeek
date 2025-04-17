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

namespace UnityPeek
{
	class Connection
	{
		private static string ip = "";
		private static int port = 0;

		public static bool isConnected = false;

		private static Thread? clientThread;

		public static void AttemptConnection(string newIP, int newPort)
		{
			if (isConnected)
			{
				return;
			}
			//check if port and ip is valid (not empty, has ip and port format
			//attempt to connect

			if (newIP == "" || newPort < 1)
			{
				UIManager.ShowPopup("Error Connecting", "Your IP or port is invalid, please check them again", Icon.Error);
				Debug.WriteLine("Invalid IP or Port");
				return;
			}

			//does the ip follow an ip format
			if (!System.Net.IPAddress.TryParse(newIP, out _))
			{
				UIManager.ShowPopup("Error Connecting", "Your IP is not in a valid format, please check it again", Icon.Error);
				Debug.WriteLine("Invalid IP");
				return;
			}

			ip = newIP;
			port = newPort;

			if (clientThread != null)
			{
				isServerRunning = false;
				Disconnect();
			}


			//Start the server thread

			Thread newClientThread = new Thread(StartClient);

			clientThread = newClientThread;
			clientThread.IsBackground = true;
			isServerRunning = true;
			clientThread.Start();
		}

		public static void Disconnect()
		{
			messageToSend = "";
			isServerRunning = false;
			UIManager.ChangeConnectedText(false);
		}


		private static bool isServerRunning = true;

		private static string messageToSend = "Hello from External Program!";

		private static void StartClient()
		{
			//check if we have disconnected from the server



			try
			{
				using (TcpClient client = new TcpClient(ip, port))
				{
					//Inital connect
					Debug.WriteLine("Connected to server!");
					UIManager.ChangeConnectedText(true);

					NetworkStream stream = client.GetStream();

					// Start receiving messages in a loop
					byte[] buffer = new byte[1024];



					//Check if the server is running, if not the thread will die
					while (isServerRunning)
					{
						isConnected = true;
						SendData(stream, messageToSend);


						if (client.Client.Poll(0, SelectMode.SelectRead) && client.Client.Available == 0)
						{
							Debug.WriteLine("Server closed connection.");
							Disconnect();
							break;
						}

						if (stream.DataAvailable)
						{
							Debug.WriteLine("Data Available");


							var (typeId, messageLength) = ReadMessageLengthAndType(stream);


							if (typeId == -1 || messageLength == -1) continue;

							//Get the message type
							//int typeId = ReadMessageType(stream);

							Debug.WriteLine("OUR TYPE ID: " + typeId);

							if(typeId == -1)
							{
								break;
							}

							//Hierarchy type
							if (typeId == 1)
							{
								Debug.WriteLine("Fetch Hierarchy Data");
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

					//Connection has dropped
					Disconnect();
					UIManager.ChangeConnectedText(false);
					isConnected = false;
				}
			}
			catch (Exception ex)
			{
				Avalonia.Threading.Dispatcher.UIThread.Post(() =>
				{
					UIManager.ShowPopup("Error Connecting", ex.Message, Icon.Error);
				});
				isConnected = false;
				Disconnect();
				Debug.WriteLine("Error: " + ex.Message);
			}
		}

		





		/// <summary>
		/// Read data of string type
		/// </summary>
		/// <param name="stream">Network Stream</param>
		/// <param name="buffer">Buffer to read from</param>
		private static void ReadStringData(NetworkStream stream, byte[] buffer)
		{
			int bytesRead = stream.Read(buffer, 0, buffer.Length);
			if (bytesRead > 0)
			{

				Debug.WriteLine("Received: " + DecodeRecivedStringData(buffer, bytesRead));

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
			Debug.WriteLine("HERE 1");
			byte[] typeBuffer = new byte[4]; // First 4 bytes = type ID
			Debug.WriteLine("HERE 2");
			int output = stream.Read(typeBuffer, 0, typeBuffer.Length);
			Debug.WriteLine("HERE 3");

			if (output <= 0) return -1;
			Debug.WriteLine("HERE 4");

			int typeId = BitConverter.ToInt32(typeBuffer, 0);
			Debug.WriteLine("HERE 5");
			return typeId;
		}
		*/

		/// <summary>
		/// Reads the first 4 bytes of a message to get its length not including those 4 bytes
		/// </summary>
		/// <param name="stream">Network Stream</param>
		/// <returns></returns>
		private static (int typeId, int messageLength) ReadMessageLengthAndType(NetworkStream stream)
		{
			byte[] headerBuffer = new byte[8];

			int totalRead = 0;
			while (totalRead < 8)
			{
				Debug.WriteLine("HERE TOTAL READ: " + totalRead);
				int bytesRead = stream.Read(headerBuffer, totalRead, 8 - totalRead);
				if (bytesRead == 0) return (-1, -1); // Stream closed
				totalRead += bytesRead;
			}

			int length = BitConverter.ToInt32(headerBuffer, 0);
			int typeId = BitConverter.ToInt32(headerBuffer, 4);

			if (length < 4) return (-1, -1);

			return (typeId, length);
		}

		/// <summary>
		/// Send data over a stream
		/// </summary>
		/// <param name="stream">The stream to send on</param>
		/// <param name="message">The message to send, will not send empty messages</param>
		private static void SendData(NetworkStream stream, string message)
		{
			if (message != "")
			{
				byte[] data = Encoding.ASCII.GetBytes(message);
				stream.Write(data, 0, data.Length);
				messageToSend = "";
				Debug.WriteLine("Sent: " + message);
			}
		}

		public static void FetchHierarchy()
		{

			if (isConnected)
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
			if (isConnected)
			{
				messageToSend = "SelectedNode:" + id;
			}
			else
			{
				//I dont think this can trigger but just in case
				UIManager.ShowPopup("Error", "You are not connected to a game", Icon.Error);
			}
		}

		public static void SendToggleTransformActive(int id, bool enabled)
		{
			messageToSend = "ToggleTransformActive:" + id + ":" + enabled;
		}

		/// <summary>
		/// Decodes recived string data
		/// </summary>
		/// <param name="buffer">Data buffer</param>
		/// <param name="bytesRead">The amount of bytes to decode</param>
		private static string DecodeRecivedStringData(byte[] buffer, int bytesRead)
		{
			return Encoding.ASCII.GetString(buffer, 0, bytesRead);
		}


	}
}
