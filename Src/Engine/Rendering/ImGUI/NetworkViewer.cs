using ImGuiNET;
using JoltPhysicsSharp;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Riptide;

//engine stuff
using static SpatialEngine.Globals;
using static SpatialEngine.Rendering.MeshUtils;
using static SpatialEngine.Resources;
using SpatialEngine.Networking;
using System.Net;

namespace SpatialEngine.Rendering.ImGUI
{
    public class NetworkViewer
    {
        static int port = 58301;
        static string ip = "127.0.0.1";
        static byte[] byteBuf = new byte[24];
        public static void Draw()
        {
            ImGui.SetNextWindowSize(new Vector2(600, 420), ImGuiCond.FirstUseEver);
            ImGui.Begin("Network");
            if(!NetworkManager.didInit)
            {
                if(ImGui.Button("StartServer"))
                {
                    NetworkManager.StartServer(false);
                }
                if (ImGui.Button("StartClient"))
                {
                    NetworkManager.StartClient();
                }
            }
            else
            {
                if(NetworkManager.serverStarted)
                {
                    if(SpatialServer.IsRunning())
                    {
                        ImGui.Text("Running Server on: ");
                        ImGui.SameLine();
                        ImGui.TextColored(new Vector4(1, 0, 0, 1), $"{SpatialServer.ip}:{SpatialServer.port}");
                        int currentSel = 0;
                        Connection[] connections = SpatialServer.GetServerConnections();
                        if (ImGui.BeginListBox("##Connections", new Vector2(ImGui.GetWindowSize().X, (connections.Length + 1) * ImGui.GetTextLineHeightWithSpacing())))
                        {
                            for (int n = 0; n < connections.Length; n++)
                            {
                                bool is_selected = (currentSel == n);
                                string name = $"Client {n + 1} at {connections[n]}";
                                if (ImGui.Selectable(name, is_selected))
                                    currentSel = n;

                                if (is_selected)
                                    ImGui.SetItemDefaultFocus();
                            }
                            ImGui.EndListBox();
                        }

                        if (ImGui.Button("Stop Server"))
                        {
                            SpatialServer.Stop();
                        }
                    }
                    else
                    {
                        if (ImGui.Button("StartServer"))
                        {
                            NetworkManager.StartServer(false);
                        }
                    }
                }
                if(SpatialClient.IsConnected())
                {
                    ImGui.Text("Connected to Server: ");
                    ImGui.SameLine();
                    ImGui.TextColored(new Vector4(1, 0, 0, 1), $"{SpatialClient.connectIp}:{SpatialClient.connectPort}");
                    Connection clientConnect = SpatialClient.GetConnection();
                    ImGui.Text("Client on: ");
                    ImGui.SameLine();
                    ImGui.TextColored(new Vector4(1, 0, 0, 1), $"{clientConnect}");
                    ImGui.Text(string.Format($"Ping: {SpatialClient.GetPing() * 1000f:0.00}ms"));

                    if(ImGui.Button("Disconnect"))
                    {
                        SpatialClient.Disconnect();
                    }
                }
                else
                {
                    ImGui.Text("Client not connected to server");
                    ImGui.InputText("IP Address", ref ip, 24, ImGuiInputTextFlags.CharsNoBlank);
                    ImGui.InputInt("Port", ref port);
                    port = (int)MathF.Abs(port);

                    if (ImGui.Button("Connect"))
                    {
                        if (IPAddress.TryParse(ip, out IPAddress address))
                        {
                            if (!NetworkManager.clientInit)
                            {
                                NetworkManager.InitClient();
                            }
                            NetworkManager.ConnectClient(address.ToString(), (ushort)port);
                        } 
                    }
                }    
            }
            ImGui.End();
        }
    }
}