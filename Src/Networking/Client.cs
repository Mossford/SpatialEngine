using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SpatialEngine
{
    namespace Networking
    {
        public class Client
        {
            UdpClient udpClient;
            IPEndPoint serverEndPoint;

            public Client(int port, string ip)
            {
                udpClient = new UdpClient();
                serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            }

            public void Send(byte[] data)
            {
                udpClient.Send(data, data.Length, serverEndPoint);
            }

            public void Close()
            {
                udpClient.Close();
                Console.WriteLine("Client Stopped");
            }
        }
    }
}