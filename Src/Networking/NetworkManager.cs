using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Riptide;
using Riptide.Utils;

namespace SpatialEngine.Networking
{
    public static class NetworkManager
    {
        public static SpatialClient client;
        public static SpatialServer server;
        public static bool serverStarted;
        public static bool clientStarted;
        public static bool didInit;

        public static void Init()
        {
            RiptideLogger.Initialize(Console.WriteLine, true);
        }

        public static void InitClient()
        {
            client = new SpatialClient();
            client.Start("127.0.0.1", 58301);
            didInit = true;
            clientStarted = true;
        }

        public static void InitServer(bool headless) 
        {
            if (headless)
            {
                server = new SpatialServer("127.0.0.1");
                server.Start();
                didInit = true;
                serverStarted = true;
            }
            else
            {
                server = new SpatialServer("127.0.0.1");
                server.Start();
                didInit = true;
                serverStarted = true;
                InitClient();
            }
        }

        public static void Cleanup()
        {
            if(didInit)
            {
                if(serverStarted)
                {
                    server.Close();
                }
                if(clientStarted)
                {
                    client.Close();
                }
            }
        }
    }
}