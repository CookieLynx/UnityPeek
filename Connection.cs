using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Metsys.Bson;
using MsBox.Avalonia.Enums;
using Tmds.DBus.Protocol;

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
            if(isConnected)
            {
                return;
            }
            //check if port and ip is valid (not empty, has ip and port format
            //attempt to connect

            if(newIP == "" || newPort < 1)
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

            if(clientThread != null)
            {
                isServerRunning = false;
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
                        SendData(stream, messageToSend);

                        if (stream.DataAvailable)
                        {
                            Debug.WriteLine("Data Available");


                            //Get the message type
                            int typeId = ReadMessageType(stream);

                            //Hierarchy type
                            if (typeId == 1)
                            {
                                Debug.WriteLine("Fetch Hierarchy Data");
                                GatherHierarchyChunkData(stream);
                            }
                            else
                            {
                                ReadStringData(stream, buffer);
                            }
                        }

                        Thread.Sleep(10); // Prevent high CPU usage
                    }
                }
            }
            catch (Exception ex)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    UIManager.ShowPopup("Error Connecting", ex.Message, Icon.Error);
                });
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

        
        private static void GatherHierarchyChunkData(NetworkStream stream)
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
                UIManager.UpdateHierarchy(root);
                Debug.WriteLine("Received: " + root.name + " With " + root.children.Count + " children");
            }
        }

        /// <summary>
        /// Reads the first 4 bytes of a message to get type ID (0 for string, 1 for Hierarchy data)
        /// </summary>
        /// <param name="stream">Network Stream</param>
        /// <returns></returns>
        private static int ReadMessageType(NetworkStream stream)
        {
            byte[] typeBuffer = new byte[4]; // First 4 bytes = type ID
            stream.Read(typeBuffer, 0, typeBuffer.Length);
            int typeId = BitConverter.ToInt32(typeBuffer, 0);
            return typeId;
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
            messageToSend = "FetchHierarchy";
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
    }
}
