using System;
using Silk.NET.Windowing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using ImGuiNET;
using Silk.NET.OpenGL.Extensions.ImGui;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using JoltPhysicsSharp;
using SpatialEngine.Networking;
using SpatialEngine.Networking.Packets;
using static SpatialEngine.Globals;
using static SpatialEngine.Rendering.MainImGui;
using SpatialEngine.Rendering;
using static SpatialEngine.Debugging;
using static SpatialEngine.Input;
using SpatialGame;
using Shader = SpatialEngine.Rendering.Shader;
using SilkNetWindow = Silk.NET.Windowing.Window;
using Texture = SpatialEngine.Rendering.Texture;

namespace SpatialEngine
{
    public class Window
    {
        public const int SCR_WIDTH = 1920;
        public const int SCR_HEIGHT = 1080;
        public static int MAX_SCR_WIDTH;
        public static int MAX_SCR_HEIGHT;
        public static Vector2 size;
        public static Vector2 windowScale;
        public static Vector2 scaleFromBase;

        static Action init;
        static Action<float> update;
        static Action<float> fixedUpdate;

        public static void Init(Action init, Action<float> update, Action<float> fixedUpdate)
        {
            Window.init = init;
            Window.update = update;
            Window.fixedUpdate = fixedUpdate;
            
            glApi.Version = new APIVersion(4, 6);
            WindowOptions options = WindowOptions.Default with
            {
                Size = new Vector2D<int>(SCR_WIDTH, SCR_HEIGHT),
                Title = "SpatialEngine - " + EngVer,
                VSync = vsync,
                PreferredDepthBufferBits = 24,
                API = glApi,
            };
            snWindow = SilkNetWindow.Create(options);
            snWindow.Load += OnLoad;
            snWindow.Update += OnUpdate;
            snWindow.Render += OnRender;
            snWindow.Closing += Clean;
            snWindow.Run();
        }

        static unsafe void OnLoad()
        {
            gl = snWindow.CreateOpenGL();
            
            //get the display size
            snWindow.WindowState = WindowState.Fullscreen;
            MAX_SCR_WIDTH = snWindow.GetFullSize().X;
            MAX_SCR_HEIGHT = snWindow.GetFullSize().Y;
            snWindow.WindowState = WindowState.Normal;
            size = (Vector2)snWindow.FramebufferSize;
            windowScale = size / (Vector2)snWindow.Size;
            scaleFromBase = size / new Vector2(SCR_WIDTH, SCR_HEIGHT);
            
            byte* text = gl.GetString(GLEnum.Renderer);
            int textLength = 0;
            while (text[textLength] != 0)
                textLength++;
            byte[] textArray = new byte[textLength];
            Marshal.Copy((IntPtr)text, textArray, 0, textLength);
            Gpu = System.Text.Encoding.Default.GetString(textArray);
            text = gl.GetString(GLEnum.Version);
            textLength = 0;
            while (text[textLength] != 0)
                textLength++;
            textArray = new byte[textLength];
            Marshal.Copy((IntPtr)text, textArray, 0, textLength);
            OpenGlVersion = System.Text.Encoding.Default.GetString(textArray);
            
            //check for mesa drivers as it has a gl_drawId bug
            if (OpenGlVersion.ToLower().Contains("mesa"))
                Settings.RendererSettings.UseMultiDraw = false;
            
            gl.Enable(GLEnum.DepthTest);
            gl.Enable(GLEnum.CullFace);
            gl.Enable(GLEnum.DebugOutput);
            gl.DebugMessageCallback(DebugProc, null);
            gl.DebugMessageControl(GLEnum.DontCare, GLEnum.DontCare, GLEnum.DebugSeverityNotification, 0, null, false);

            //init systems
            currentScene = new Scene();
            physics = new Physics();
            physics.InitPhysics();
            TextureManager.Init();
            
            Renderer.Init(currentScene);
            ParticleManager.Init();
            RayTracer.Init(currentScene);
            UiRenderer.Init();
            UiTextHandler.Init();

            NetworkManager.Init();

            player = new Player(15.0f, new Vector3(-33, 12, -20), new Vector3(300, 15, 0));

            //input stuffs
            Input.Init();
            Mouse.Init();
            for (int i = 0; i < input.Keyboards.Count; i++)
                input.Keyboards[i].KeyDown += KeyDown;
            
            //imgui control stuff
            controller = new ImGuiController(gl, snWindow, input);
            ImGui.SetWindowSize(new Vector2(400, 600));

            //init game
            init.Invoke();
            MainImGui.Init();
        }
        
        static bool lockMouse = false;
        static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
        {
            if(!lockMouse && key == Key.Escape)
            {
                Mouse.mouse.Cursor.CursorMode = CursorMode.Raw;
                lockMouse = true;
            }
            else if(lockMouse && key == Key.Escape)
            {
                Mouse.mouse.Cursor.CursorMode = CursorMode.Normal;
                lockMouse = false;
            }
            
            if(!showImguiDebug && key == Key.F1)
            {
                showImguiDebug = true;
            }
            else if(showImguiDebug && key == Key.F1)
            {
                showImguiDebug = false;
            }
        }
        
        static float totalTimeUpdate = 0.0f;

        static void OnUpdate(double dt)
        {
            //adjust for dpi scaling because x11 and wayland are amazing
            size = (Vector2)snWindow.FramebufferSize;
            windowScale = size / (Vector2)snWindow.Size;
            scaleFromBase = size / new Vector2(SCR_WIDTH, SCR_HEIGHT);
            
            totalTime += (float)dt;
            
            Input.Clear();
            Input.Update();
            Mouse.Update();
            UiRenderer.Update();
            ParticleManager.Update((float)dt);
            
            if(lockMouse)
            {
                Vector2 mousePosMoved = Mouse.position - Mouse.lastPosition;
                player.Look((int)mousePosMoved.X, (int)mousePosMoved.Y, false, false);
            }
            
            currentScene.Update();
            update.Invoke((float)dt);

            totalTimeUpdate += (float)dt * 1000;
            while (totalTimeUpdate >= fixedUpdateTime)
            {
                totalTimeUpdate -= fixedUpdateTime;
                FixedUpdate(fixedUpdateTime / 1000f);
            }
        }

        static void FixedUpdate(float dt)
        {
            fixedUpdate.Invoke(dt);

            if (NetworkManager.didInit)
            {
                if(NetworkManager.serverStarted)
                {
                    SpatialServer.Update(dt);
                }
                if(NetworkManager.clientStarted)
                {
                    SpatialClient.Update(dt);
                }
            }
            
            physics.UpdatePhysics(ref currentScene, dt);
        }

        static unsafe void OnRender(double dt)
        {
            if(showImguiDebug)
            {
                controller.Update((float)dt);
                ImGuiMenu((float)dt);
            }

            player.camera.SetViewMat();
            player.camera.SetProjMat(size.X, size.Y);
            player.camera.SetProjMatClose(size.X, size.Y);
            

            gl.ClearColor(Color.FromArgb(102, 178, 204));
            gl.Viewport(0,0, (uint)size.X, (uint)size.Y);

            gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            gl.DepthFunc(GLEnum.Lequal);
            gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);
            if(showWireFrame)
                gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line);

            if (!Settings.RendererSettings.EnableRayTracing)
            {
                gl.UseProgram(Renderer.defaultShader.shader);
                Renderer.defaultShader.setVec3("lightPos", player.position);
                Renderer.Draw(currentScene, ref Renderer.defaultShader, player.camera.viewMat, player.camera.projMat, player.camera.position);
            }
            else
            {
                gl.UseProgram(RayTracer.shader.shader);
                RayTracer.shader.setVec3("lightPos",  player.position);
                RayTracer.Draw(currentScene, player.camera.viewMat, player.camera.projMat, player.camera.position);
            }
            ParticleManager.Render(player.camera.viewMat, player.camera.projMat);
            UiRenderer.Draw();

            SetNeededDebug(player.camera.projMat, player.camera.viewMat);
            DrawDebugItems();

            if (showImguiDebug)
            {
                controller.Render();
            }
        }

        static void Clean()
        {
            ParticleManager.Clean();
            
            physics.CleanPhysics(ref currentScene);
            NetworkManager.Cleanup();
            currentScene.Clear();
        }
    }
}