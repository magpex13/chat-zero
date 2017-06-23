using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace ChatServer
{
    static class SocketExtensions
    {
        public static bool IsConnected(this Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }
    }
    class Program
    {
        class ChatServer
        {
            public int port {get;set;}
            public string IPHost {get;set;}
            private List<TcpClient> lstClients;
            private Mutex semaphore;
            public ChatServer(string IPHost, int port)
            {
                this.IPHost = IPHost;
                this.port = port;
                lstClients = new List<TcpClient>();
                semaphore =  new Mutex();
            }
            public ChatServer(int port)
            {
                this.IPHost = IPAddress.Any.ToString();
                this.port = port;
                lstClients = new List<TcpClient>();
                semaphore =  new Mutex();
            }
            public void connection()
            {
                TcpListener tcpServer = new TcpListener(IPAddress.Parse(IPHost),port);
                tcpServer.Start();
                while(true)
                {
                    Console.WriteLine("Listening...");
                    lstClients.Add(tcpServer.AcceptTcpClientAsync().Result);
                    new Thread(() => clientCallback(lstClients[lstClients.Count - 1])).Start();
                }
            }
            public void broadcast(string message)
            {
                foreach(TcpClient item in lstClients)
                {
                    NetworkStream stream = item.GetStream();
                    byte[] bytesSend = Encoding.ASCII.GetBytes(message);
                    stream.Write(bytesSend,0,bytesSend.Length);
                    stream.Flush();
                }
            }
            public void clientCallback(TcpClient tcpClient)
            {
                using(NetworkStream stream = tcpClient.GetStream())
                {
                    while(tcpClient.Client.IsConnected())
                    {
                        if(stream.DataAvailable)
                        {
                            Console.WriteLine("Received");
                            byte[] bytesReceived = new byte[256];
                            stream.Read(bytesReceived,0,bytesReceived.Length);
                            string messageReceived = Encoding.ASCII.GetString(bytesReceived);
                            Console.WriteLine("Message Received : " + messageReceived.Substring(0,messageReceived.IndexOf('\0'))); //Al ser el buffer de tamaño 256, se obtiene la cadena mandada por el cliente (Que normalmente, su tamaño es menor a 256) y el resto basura.
                            semaphore.WaitOne();

                            broadcast(messageReceived);
                            
                            semaphore.ReleaseMutex();
                        }
                    }
                    Console.WriteLine("Client Disconnect");
                }

                semaphore.WaitOne();

                tcpClient.Dispose();
                lstClients.RemoveAt(lstClients.IndexOf(tcpClient));
                Console.WriteLine("Client deleted, Total Clients Now: " + lstClients.Count);

                semaphore.ReleaseMutex();
            }

        }
        
        static void Main(string[] args)
        {
            ChatServer server = new ChatServer(1234);
            server.connection();
        }

        
    }
}
