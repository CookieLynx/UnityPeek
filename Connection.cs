using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            //check if port and ip is valid (not empty, has ip and port format
            //attempt to connect

            if(newIP == "" || newPort < 1)
            {
                UIManager.ShowErrorPopup("Error Connecting", "Your IP or port is invalid, please check them again");
                Debug.WriteLine("Invalid IP or Port");
                return;
            }

            //does the ip follow an ip format
            if (!System.Net.IPAddress.TryParse(newIP, out _))
            {
                UIManager.ShowErrorPopup("Error Connecting", "Your IP is not in a valid format, please check it again");
                Debug.WriteLine("Invalid IP");
                return;
            }

            ip = newIP;
            port = newPort;

            if(clientThread != null)
            {
                isServerRunning = false;
            }

            Thread newClientThread = new Thread(StartClient);

            clientThread = newClientThread;
            clientThread.IsBackground = true;
            isServerRunning = true;
            clientThread.Start();
        }

        public static void Disconnect()
        {
            isServerRunning = false;
        }


        private static bool isServerRunning = true;
        private static void StartClient()
        {

            try
            {
                using (TcpClient client = new TcpClient(ip, port))
                {
                    Debug.WriteLine("Connected to server!");

                    NetworkStream stream = client.GetStream();

                    // Send data to the Unity server
                    string message = "Hello from External Program!";
                    byte[] data = Encoding.ASCII.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                    Debug.WriteLine("Sent: " + message);

                    // Start receiving messages in a loop
                    byte[] buffer = new byte[1024];
                    while (isServerRunning)
                    {
                        if (stream.DataAvailable)
                        {
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);
                            if (bytesRead > 0)
                            {
                                string received = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                                Debug.WriteLine("Received: " + received);
                            }
                        }

                        Thread.Sleep(10); // Prevent high CPU usage
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
