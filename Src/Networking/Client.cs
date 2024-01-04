using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Riptide;
using Riptide.Transports;
using Riptide.Utils;

using static SpatialEngine.Networking.PacketHandler;

namespace SpatialEngine.Networking
{
    public class SpatialClient
    {
        public static Client client;
        public static Thread clientThread;

        public static ushort connectPort;
        public static string connectIp;

        public SpatialClient()
        {

        }

        public void Start(string ip, ushort port)
        {
            connectIp = ip;
            connectPort = port;
            client = new Client();
            client.Connected += Connected;
            client.Disconnected += Disconnected;
            Connect(connectIp, connectPort);
            clientThread = new Thread(Update);
            clientThread.Start();
        }

        public void Connect(string ip, ushort port)
        {
            client.Connect($"{ip}:{port}");
        }

        public void Update()
        {
            Message msg = Message.Create(MessageSendMode.Reliable, 0);
            msg.AddInt(1);
            client.Send(msg);
            while(true)
            {
                client.Update();
            }
        }

        void Connected(object sender, EventArgs e)
        {
               
        }

        void Disconnected(object sender, EventArgs e)
        {
            
        }

        [MessageHandler((ushort)0)]
        public static void MessageTest(Message message)
        {
            int test = message.GetInt();
            Console.WriteLine("got message " + test);
        }
    }
}