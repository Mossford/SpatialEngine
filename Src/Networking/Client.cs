using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using static SpatialEngine.Networking.PacketHandler;

namespace SpatialEngine.Networking
{
    public class Client
    {
        UdpClient udpClient;
        IPEndPoint serverEndPoint;
        IPEndPoint clientEndPoint;
    
        //for averaging the ping
        int pingTotal;
        int pings;
    
        public Client(int port, string ip)
        {
            udpClient = new UdpClient();
            serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        }
    
        //may need to be async
        public void Send(byte[] data)
        {
            udpClient.Send(data, data.Length, serverEndPoint);
        }
    
        void ReciveAsync(IAsyncResult result)
        {
            byte[] data = udpClient.EndReceive(result, ref clientEndPoint);
            udpClient.BeginReceive(ReciveAsync, null);
            HandlePacket(data);
        }
    
        //reset after some time like 10 seconds
        public void GetPing()
        {
            PingPacket pingPacket = new PingPacket();
            byte[] data = pingPacket.ConvertToByte();
            udpClient.Send(data, data.Length, serverEndPoint);
        }
    
        public void Close()
        {
            udpClient.Close();
            Console.WriteLine("Client Stopped");
        }
    }
}