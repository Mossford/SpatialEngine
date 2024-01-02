using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

//engine stuff
using static SpatialEngine.Globals;
using static SpatialEngine.Networking.PacketHandler;

namespace SpatialEngine
{
    namespace Networking
    {
        //client and server
        //updates get sent here and sent out here
        //Client Server arch:
        /*
            client sends input updates to server and server will apply that update
            server can also be host
            host computer will auto update its self and send that update to all clients
            
            every frame:
            check for stuff from clients (input)
            if there is input
                run the update based on that input (enum with switch?)
            run host update
            send all update to client (position of things, rotation of things)

        */

        public class Host
        {
            public int port;
            public string ip;

            UdpClient udpServer;
            IPEndPoint endPoint;
            bool closed = false;

            int clientAmt;
            string[] ips;
            int[] ports;

            public Host(int port, string ip)
            {
                this.port = port;
                this.ip = ip;
                InitHost();
            }

            void InitHost()
            {
                udpServer = new UdpClient();
                
                endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            }

            public void Start()
            {
                udpServer.Client.Bind(endPoint);
                udpServer.BeginReceive(ReciveAsync, null);
                Console.WriteLine($"UDP Server started on {ip}:{port}");
            }

            public void SetIpPort(int port, string ip)
            {
                this.port = port;
                this.ip = ip;
                endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                udpServer.Client.Bind(endPoint);
                Console.WriteLine($"UDP Server now on {ip}:{port}");
            }

            void ReciveAsync(IAsyncResult result)
            {
                byte[] data = udpServer.EndReceive(result, ref endPoint);
                udpServer.BeginReceive(ReciveAsync, null);
                HandlePacket(data);
            }

            public void Close()
            {
                closed = true;
                udpServer.Close();

                Console.WriteLine("Server stopped");
            }
        }
    }
}