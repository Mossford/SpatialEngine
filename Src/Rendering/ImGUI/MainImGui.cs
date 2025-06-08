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
using SpatialEngine.Rendering.ImGUI;
using ImGui = ImGuiNET.ImGui;

namespace SpatialEngine.Rendering
{
    public static class MainImGui
    {
        static bool ShowSceneMenu,
            ShowObjectMenu,
            ShowConsoleMenu,
            ShowNetworkMenu,
            ShowSettingsMenu;
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
                Globals.snWindow.VSync = vsync;
            }

            ImGui.Spacing();
            ImGui.DragFloat3("Player Position", ref player.position, 1.0f, -50.0f, 50.0f);
            ImGui.DragFloat3("Player Rotation", ref player.rotation, 1.0f, -360.0f, 360.0f);
            ImGui.SliderFloat("Cam Fov", ref player.camera.zoom, 179.9f, 0.01f);

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Menus"))
                {
                    ImGui.MenuItem("Scene", null, ref ShowSceneMenu);
                    ImGui.MenuItem("Object", null, ref ShowObjectMenu);
                    ImGui.MenuItem("Console", null, ref ShowConsoleMenu);
                    ImGui.MenuItem("Network", null, ref ShowNetworkMenu);
                    ImGui.MenuItem("Settings", null, ref ShowSettingsMenu);
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            if (ShowSceneMenu)
            {
                SceneViewer.Draw();
            }

            if (ShowObjectMenu)
            {
                ObjectViewer.Draw();
            }

            if (ShowConsoleMenu)
            {
                ConsoleViewer.Draw();
            }

            if(ShowNetworkMenu) 
            {
                NetworkViewer.Draw();
            }

            if (ShowSettingsMenu)
            {
                SettingsViewer.Draw();
            }
        }
        
        public static void HelpMarker(string desc)
        {
            ImGui.SameLine();
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted(desc);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }
        
    }
}
