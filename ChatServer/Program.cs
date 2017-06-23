using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 1234;
            TcpListener tcpServer = new TcpListener(IPAddress.Any,port);
            tcpServer.Start();
            TcpClient tcpClient;
            while(true)
            {
                Console.WriteLine("Listening...");
                tcpClient = tcpServer.AcceptTcpClientAsync().Result;
                new Thread(() => Program.connection(tcpClient)).Start();
            }
        }
        
        static void connection(TcpClient tcpClient)
        {
            using(NetworkStream stream = tcpClient.GetStream())
            {
                while(true)
                {
                    if(stream.DataAvailable)
                    {
                        Console.WriteLine("Received");
                        byte[] bytesReceived = new byte[256];
                        stream.Read(bytesReceived,0,bytesReceived.Length);
                        string messageReceived = Encoding.ASCII.GetString(bytesReceived);
                        Console.WriteLine("Message Received : " + messageReceived.Substring(0,messageReceived.IndexOf('\0'))); //Al ser el buffer de tamaño 256, se obtiene la cadena mandada por el cliente (Que normalmente, su tamaño es menor a 256) y el resto basura.
                        byte[] bytesSend = Encoding.ASCII.GetBytes(messageReceived);
                        stream.Write(bytesSend,0,bytesSend.Length);
                    }
                }
            }

            //tcpClient.Dispose();
        }
    }
}
