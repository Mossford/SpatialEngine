using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Riptide;

//engine stuff
using static SpatialEngine.Globals;
using static SpatialEngine.Networking.PacketHandler;

namespace SpatialEngine.Networking
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
    
    public class SpatialServer
    {
        public static Server server;
        public static Thread serverThread;

        public ushort port;
        public string ip;
        public int maxConnections { get; protected set; }

        bool stopping;

        public SpatialServer(string ip, ushort port = 58301, int maxConnections = 10)
        {
            this.ip = ip;
            this.port = port;
            this.maxConnections = maxConnections;
        }

        public void Start()
        {
            server = new Server();
            serverThread = new Thread(Update);
            server.ClientConnected += ClientConnected;
            server.Start(port, (ushort)maxConnections);
            serverThread.Start();
        }

        public void Update()
        {
            while(true)
            {
                if(stopping)
                    return;
                server.Update();
            }
        }

        public void ClientConnected(object sender, EventArgs e)
        {

        }

        public Connection[] GetServerConnections()
        {
            return server.Clients;
        }

        [MessageHandler((ushort)0)]
        public static void MessageTest(ushort fromClientId, Message message)
        {
            int test = message.GetInt();
            Console.WriteLine("got message " + test);
            Message msg = Message.Create(MessageSendMode.Reliable, 0);
            msg.AddInt(test);
            server.Send(msg, fromClientId);
            Console.WriteLine("sending message to " + server.Clients[fromClientId - 1]);
        }

        public void Close()
        {
            server.Stop();
            stopping = true;
            serverThread.Interrupt();
            server = null;
            serverThread = null;
        }
    }
}