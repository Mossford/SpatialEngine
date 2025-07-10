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
    public class SettingsViewer
    {
        public static void Draw()
        {
            ImGui.SetNextWindowSize(new Vector2(600, 420), ImGuiCond.FirstUseEver);
            ImGui.Begin("Settings");

            ImGui.PushItemWidth(100);
            ImGui.SliderInt("RenderBufferMode", ref Settings.RendererSettings.OptimizeUpdatingBuffers, 0, 2);
            ImGui.PopItemWidth();
            string optimizeBufferSet;
            if (Settings.RendererSettings.OptimizeUpdatingBuffers == 0)
                optimizeBufferSet = "Updates every frame";
            else if (Settings.RendererSettings.OptimizeUpdatingBuffers == 1)
                optimizeBufferSet = "Updates every second or when a new object is added";
            else
                optimizeBufferSet = "Updates when a new object is added";
            MainImGui.HelpMarker(optimizeBufferSet);

            ImGui.Checkbox("UseMultiDraw", ref Settings.RendererSettings.UseMultiDraw);
            MainImGui.HelpMarker("Enables MultiDraw for lower draw calls");
            
            ImGui.Checkbox("EnableRayTracing", ref Settings.RendererSettings.EnableRayTracing);
            MainImGui.HelpMarker("Enables gpu RayTracing");
            
            ImGui.End();
        }
    }
}