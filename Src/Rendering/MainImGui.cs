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

namespace SpatialEngine.Rendering
{
    public static class MainImGui
    {
        static bool ShowSceneViewerMenu, ShowObjectViewerMenu, ShowConsoleViewerMenu, ShowNetworkViewerMenu;
        static int IMM_counter = 0;
        static Vector3 IMM_selposition = new Vector3();
        static Vector3 IMM_selrotation = new Vector3();
        static Vector3 IMM_selvelocity = new Vector3();
        static Vector3 IMM_selvelocityrot = new Vector3();
        static int IMM_IcoSphereSub = 0;
        static float IMM_SpikerSphereSize = 0;
        static string IMM_input = "";
        static bool IMM_static = false;
        static ImFontPtr font;
        
        static float fpsCount;
        static float fpsTotal;
        static float msCount;
        static float msTotal;
        static float fpsTime;
        
        public static void Init()
        {
            font = ImGui.GetFont();
            font.Scale = 1.35f;
        }
        
        public static void ImGuiMenu(float deltaTime)
        {
            ImGuiWindowFlags window_flags = 0;
            window_flags |= ImGuiWindowFlags.NoTitleBar;
            window_flags |= ImGuiWindowFlags.MenuBar;

            ImGui.Begin("SpaceTesting", window_flags);

            //needs to be io.framerate because the actal deltatime is polled too fast and the 
            //result is hard to read
            ImGui.Text("Version " + EngVer);
            ImGui.Text("OpenGl " + OpenGlVersion);
            ImGui.Text("Gpu: " + Gpu);
            ImGui.Text(String.Format("{0:N3} ms/frame ({1:N1} FPS)", 1.0f / ImGui.GetIO().Framerate * 1000.0f, ImGui.GetIO().Framerate));
            ImGui.Text(String.Format("{0:N3} ms Avg ({1:N1} FPS Avg)", msTotal / fpsCount, fpsTotal / fpsCount));
            ImGui.Text(String.Format("DrawCall per frame: ({0:N1})", MathF.Round(drawCallCount)));
            ImGui.Text(String.Format("{0} verts, {1} indices ({2} tris)", vertCount, indCount, indCount / 3));
            ImGui.Text(String.Format("RenderSets: {0}", Renderer.renderSets.Count));
            ImGui.Text(String.Format("Amount of Spatials: ({0})", scene.SpatialObjects.Count()));
            ImGui.Text(String.Format("DrawCall Avg: ({0:N1}) DC/frame, DrawCall Total ({1})", MathF.Round(drawCallCount / (totalTime / deltaTime)), drawCallCount));
            ImGui.Text(String.Format("Time Open {0:N1} minutes", totalTime / 60.0f));
            
            fpsTotal += ImGui.GetIO().Framerate;
            msTotal += 1.0f / ImGui.GetIO().Framerate * 1000f;
            fpsCount++;
            msTotal++;
            fpsTime += deltaTime;
            if(fpsTime >= 10)
            {
                fpsTime = 0f;
                fpsTotal = 0f;
                fpsCount = 0;
                msTotal = 0;
                msCount = 0;
            }

            drawCallCount = 0;
            
            ImGui.Checkbox("Wire Frame", ref showWireFrame);
            if (ImGui.Checkbox("Vsync", ref vsync))
            {
                Globals.vsync = vsync;
            }

            ImGui.Spacing();
            ImGui.DragFloat3("Player Position", ref player.position, 1.0f, -50.0f, 50.0f);
            ImGui.DragFloat3("Player Rotation", ref player.rotation, 1.0f, -360.0f, 360.0f);
            ImGui.SliderFloat("Cam Fov", ref player.camera.zoom, 179.9f, 0.01f);

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Menus"))
                {
                    ImGui.MenuItem("Scene Viewer", null, ref ShowSceneViewerMenu);
                    ImGui.MenuItem("Object Viewer", null, ref ShowObjectViewerMenu);
                    ImGui.MenuItem("Console Viewer", null, ref ShowConsoleViewerMenu);
                    ImGui.MenuItem("Network Viewer", null, ref ShowNetworkViewerMenu);
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            if (ShowSceneViewerMenu)
            {
                SceneViewer();
            }

            if (ShowObjectViewerMenu)
            {
                ObjectViewer();
            }

            if (ShowConsoleViewerMenu)
            {
                ConsoleViewer();
            }

            if(ShowNetworkViewerMenu) 
            {
                NetworkViewer();
            }
        }

        static void SceneViewer()
        {
            ImGui.SetNextWindowSize(new Vector2(600, 420), ImGuiCond.FirstUseEver);
            ImGui.Begin("Scene Viewer");

            if (ImGui.TreeNode("Objects"))
            {
                for (int i = 0; i < scene.SpatialObjects.Count(); i++)
                {
                    if (ImGui.TreeNode(string.Format("Object {0}", scene.SpatialObjects[i].SO_id)))
                    {
                        ImGui.DragFloat3("Object Position", ref scene.SpatialObjects[i].SO_mesh.position, 0.05f, -100000.0f, 100000.0f);
                        ImGui.TreePop();
                    }
                }
                ImGui.TreePop();
            }
            /*if(ImGui.TreeNode("Scenes"))
            {
                std.vector<std::string> files = GetFiles(sceneLoc);
                for (unsigned int i = 0; i < files.size(); i++)
                {
                    if (ImGui::TreeNode((void*)(intptr_t)i, "Scene: %s", files[i].c_str()))
                    {
                        ImGui::SameLine();
                        if(ImGui::Button("Load"))
                        {
                            LoadScene(sceneLoc, files[i], mainScene);
                        }
                        ImGui::TreePop();
                    }
                }
                ImGui::TreePop();
            }*/

            ImGui.End();
        }

        static void ObjectViewer()
        {
            ImGui.SetNextWindowSize(new Vector2(600, 420), ImGuiCond.FirstUseEver);
            ImGui.Begin("Object Viewer");

            //select the object you want gives properties 
            //or create a object and that selects by default
            ImGui.Text("Select Object Properties");
            ImGui.Spacing();
            if (ImGui.ArrowButton("##left", ImGuiDir.Left))
            {
                if (IMM_counter > (int)MeshType.First)
                    IMM_counter--;
            }
            ImGui.SameLine(0.0f, 1.0f);
            if (ImGui.ArrowButton("##right", ImGuiDir.Right))
            {
                if (IMM_counter < (int)MeshType.Last)
                    IMM_counter++;
            }
            ImGui.SameLine();
            ImGui.Text(((MeshType)IMM_counter).ToString());
            if (IMM_counter == (int)MeshType.IcoSphereMesh)
                ImGui.SliderInt("Subdivison level", ref IMM_IcoSphereSub, 0, 10);
            if (IMM_counter == (int)MeshType.SpikerMesh)
            {
                ImGui.SliderInt("Subdivison level", ref IMM_IcoSphereSub, 0, 10);
                ImGui.SliderFloat("Size", ref IMM_SpikerSphereSize, 0.01f, 10);
            }
            if (IMM_counter == (int)MeshType.FileMesh)
            {
                if (ImGui.TreeNode("Models"))
                {
                    /*static std::vector<std::string> files = GetFiles(modelLoc);
                    for (unsigned int i = 0; i < files.size(); i++)
                    {
                        if (i == 0)
                            ImGui::SetNextItemOpen(true, ImGuiCond_Once);

                        if (ImGui::TreeNode((void*)(intptr_t)i, "Model: %s", files[i].c_str()))
                        {
                            ImGui::SameLine();
                            if(ImGui::Button("Set"))
                            {
                                input = files[i];
                            }
                            ImGui::TreePop();
                        }
                    }*/
                    ImGui.TreePop();
                }
            }
            else
            {
                IMM_input = ((MeshType)IMM_counter).ToString();
            }
            ImGui.Checkbox("Static", ref IMM_static);
            ImGui.InputFloat3("Object Position", ref IMM_selposition);
            ImGui.InputFloat3("Object Rotation", ref IMM_selrotation);
            ImGui.InputFloat3("Object Velocity", ref IMM_selvelocity);
            ImGui.InputFloat3("Object Rotation Velocity", ref IMM_selvelocityrot);
            ImGui.Text("Current Model: " + IMM_input);

            if (ImGui.Button("Add Object"))
            {
                Mesh selmesh;
                int id = scene.SpatialObjects.Count;
                switch (IMM_counter)
                {
                    case (int)MeshType.CubeMesh:
                        selmesh = CreateCubeMesh(IMM_selposition, new Quaternion(IMM_selrotation, 1.0f));
                        vertCount += (uint)selmesh.vertexes.Length;
                        indCount += (uint)selmesh.indices.Length;
                        if (IMM_static)
                            scene.AddSpatialObject(selmesh, new Vector3(1.0f), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
                        else
                            scene.AddSpatialObject(selmesh, new Vector3(1.0f), MotionType.Dynamic, Layers.MOVING, Activation.Activate);
                        break;
                    case (int)MeshType.IcoSphereMesh:
                        selmesh = CreateSphereMesh(IMM_selposition, new Quaternion(IMM_selrotation * MathF.PI / 180.0f, 1.0f), (uint)IMM_IcoSphereSub);
                        vertCount += (uint)selmesh.vertexes.Length;
                        indCount += (uint)selmesh.indices.Length;
                        if (IMM_static)
                            scene.AddSpatialObject(selmesh, 1.0f, MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
                        else
                            scene.AddSpatialObject(selmesh, 1.0f, MotionType.Dynamic, Layers.MOVING, Activation.Activate);
                        break;
                    case (int)MeshType.SpikerMesh:
                        selmesh = CreateSpikerMesh(IMM_selposition, new Quaternion(IMM_selrotation * MathF.PI / 180.0f, 1.0f), IMM_SpikerSphereSize, IMM_IcoSphereSub);
                        vertCount += (uint)selmesh.vertexes.Length;
                        indCount += (uint)selmesh.indices.Length;
                        if (IMM_static)
                            scene.AddSpatialObject(selmesh, MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
                        else
                            scene.AddSpatialObject(selmesh, MotionType.Dynamic, Layers.MOVING, Activation.Activate);
                        break;
                    case (int)MeshType.TriangleMesh:
                        selmesh = Create2DTriangle(IMM_selposition, new Quaternion(IMM_selrotation * MathF.PI / 180.0f, 1.0f));
                        vertCount += (uint)selmesh.vertexes.Length;
                        indCount += (uint)selmesh.indices.Length;
                        if (IMM_static)
                            scene.AddSpatialObject(selmesh, MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
                        else
                            scene.AddSpatialObject(selmesh, MotionType.Dynamic, Layers.MOVING, Activation.Activate);
                        break;
                    case (int)MeshType.FileMesh:
                        if (!File.Exists(ModelPath + IMM_input))
                        {
                            ImGui.OpenPopup("Error");
                        }
                        else
                        {
                            scene.AddSpatialObject(LoadModel(IMM_selposition, new Quaternion(IMM_selrotation * MathF.PI / 180.0f, 1.0f), IMM_input));
                            vertCount += (uint)scene.SpatialObjects[id].SO_mesh.vertexes.Length;
                            indCount += (uint)scene.SpatialObjects[id].SO_mesh.indices.Length;
                            scene.SpatialObjects[id].SO_rigidbody.SetVelocity(IMM_selvelocity);
                            scene.SpatialObjects[id].SO_rigidbody.SetAngularVelocity(IMM_selvelocityrot);
                        }
                        break;
                }
            }
            if (ImGui.BeginPopup("Error"))
            {
                string text = "Model Not Found " + IMM_input;
                ImGui.Text(text);
                ImGui.EndPopup();
            }

            ImGui.End();
        }

        static void ConsoleViewer()
        {
            ImGui.SetNextWindowSize(new Vector2(600, 420), ImGuiCond.FirstUseEver);
            ImGui.Begin("Console Viewer");
            ImGui.End();
        }

        static int port = 0;
        static string ip = "";
        static byte[] byteBuf = new byte[24];
        static void NetworkViewer()
        {
            ImGui.SetNextWindowSize(new Vector2(600, 420), ImGuiCond.FirstUseEver);
            ImGui.Begin("Network Viewer");
            if(!NetworkManager.didInit)
            {
                if(ImGui.Button("StartServer"))
                {
                    NetworkManager.InitServer();
                }
                if (ImGui.Button("StartClient"))
                {
                    NetworkManager.InitClient();
                }
            }
            else
            {
                if(NetworkManager.isServer)
                {
                    if(NetworkManager.server.IsRunning())
                    {
                        ImGui.Text("Running Server on: ");
                        ImGui.SameLine();
                        ImGui.TextColored(new Vector4(1, 0, 0, 1), $"{NetworkManager.server.ip}:{NetworkManager.server.port}");
                        int currentSel = 0;
                        Connection[] connections = NetworkManager.server.GetServerConnections();
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
                            NetworkManager.server.Stop();
                        }
                    }
                    else
                    {
                        if (ImGui.Button("StartServer"))
                        {
                            NetworkManager.InitServer();
                        }
                    }
                }
                else
                {
                    if(NetworkManager.client.IsConnected())
                    {
                        ImGui.Text("Connected to Server: ");
                        ImGui.SameLine();
                        ImGui.TextColored(new Vector4(1, 0, 0, 1), $"{NetworkManager.client.connectIp}:{NetworkManager.client.connectPort}");
                        Connection clientConnect = NetworkManager.client.GetConnection();
                        ImGui.Text("Client on: ");
                        ImGui.SameLine();
                        ImGui.TextColored(new Vector4(1, 0, 0, 1), $"{clientConnect}");
                        ImGui.Text(string.Format($"Ping: {NetworkManager.client.GetPing() * 1000f:0.00}ms"));

                        if(ImGui.Button("Disconnect"))
                        {
                            NetworkManager.client.Disconnect();
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
                                NetworkManager.client.Connect(address.ToString(), (ushort)port);
                            }
                        }
                    }
                }    
            }
            ImGui.End();
        }
    }
}
