using System.Runtime.CompilerServices;
using JoltPhysicsSharp;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.OpenGL.Extensions.ImGui;

namespace SpatialEngine
{
    public static class Globals
    {
        public static GL gl;
        public static GraphicsAPI glApi = GraphicsAPI.Default;
        public static IWindow snWindow;
        public static string EngVer = "ENG:0.76 Stable";
        public static string OpenGlVersion = "";
        public static string Gpu = "";
            
        public static Scene scene;
        public static Physics physics;
        public static PhysicsSystem physicsSystem;
        public static BodyInterface bodyInterface;
            
        public static ImGuiController controller;
        public static bool showImguiDebug = false;
        public static bool showWireFrame = false;
        //going to be true because my gpu squeals if vsync is off
        public static bool vsync = true;
        public static uint vertCount;
        public static uint indCount;

        public static Player player;

        public static uint drawCallCount = 0;
        public static float totalTime = 0.0f;
        public static float deltaTime = 0.0f;
        public const float fixedUpdateTime = 16.667f;

        /// <summary>
        /// In Seconds
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetTime() => totalTime;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetDeltaTime() => deltaTime;
    }
}