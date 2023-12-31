using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

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

            public Host(int port, string ip)
            {
                this.port = port;
                this.ip = ip;
                InitHost();
            }

            void InitHost()
            {
                udpServer = new UdpClient();
                //endpoint that accept from any ip but on correct port?
                endPoint = new IPEndPoint(IPAddress.Any, port);
                udpServer.Client.Bind(endPoint);
                udpServer.BeginReceive(Recive, null);
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

            public void Recive(IAsyncResult result)
            {
                
            }

            public void HandlePacket(byte[] data)
            {
                //run whatever packet it is
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