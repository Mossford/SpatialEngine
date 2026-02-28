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
        public static bool serverStarted;
        public static bool clientStarted;
        public static bool didInit;
        public static bool clientInit;

        public static void Init()
        {
            RiptideLogger.Initialize(Console.WriteLine, true);
        }

        public static void InitClient()
        {
            SpatialClient.Init();
            clientInit = true;
            didInit = true;
        }
        
        public static void StartClient(string ip = "127.0.0.1", ushort port = 58301)
        {
            SpatialClient.Init();
            SpatialClient.Connect(ip, port);
            didInit = true;
            clientInit = true;
            clientStarted = true;
        }
        
        public static void ConnectClient(string ip = "127.0.0.1", ushort port = 58301)
        {
            SpatialClient.Connect("127.0.0.1", 58301);
            didInit = true;
            clientInit = true;
            clientStarted = true;
        }

        public static void StartServer(bool headless, string ip = "127.0.0.1", ushort port = 58301, int maxConnections = 10) 
        {
            if (headless)
            {
                SpatialServer.Init(ip, port, maxConnections);
                SpatialServer.Start();
                didInit = true;
                serverStarted = true;
            }
            else
            {
                SpatialServer.Init(ip, port, maxConnections);
                SpatialServer.Start();
                didInit = true;
                serverStarted = true;
                StartClient();
            }
        }
        
        public static void StartServerNoClient(bool headless, string ip = "127.0.0.1", ushort port = 58301, int maxConnections = 10) 
        {
            if (headless)
            {
                SpatialServer.Init(ip, port, maxConnections);
                SpatialServer.Start();
                didInit = true;
                serverStarted = true;
            }
            else
            {
                SpatialServer.Init(ip, port, maxConnections);
                SpatialServer.Start();
                didInit = true;
                serverStarted = true;
            }
        }

        public static void Cleanup()
        {
            if(didInit)
            {
                if(serverStarted)
                {
                    SpatialServer.Close();
                    serverStarted = false;
                }
                if(clientStarted)
                {
                    SpatialClient.Close();
                    clientStarted = false;
                }
            }
        }
    }
}