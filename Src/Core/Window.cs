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

        public static Shader defaultShader;
        public static Texture defaultTexture;

        public static void Init()
        {
            glApi.Version = new APIVersion(4, 6);
            WindowOptions options = WindowOptions.Default with
            {
                Size = new Vector2D<int>(SCR_WIDTH, SCR_HEIGHT),
                Title = "SpatialEngine",
                VSync = vsync,
                PreferredDepthBufferBits = 24,
                API = glApi,
            };
            snWindow = SilkNetWindow.Create(options);
            snWindow.Load += OnLoad;
            snWindow.Update += OnUpdate;
            snWindow.Render += OnRender;
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
            gl.Enable(GLEnum.DepthTest);
            gl.Enable(GLEnum.CullFace);
            gl.Enable(GLEnum.DebugOutput);
            gl.DebugMessageCallback(DebugProc, null);
            gl.DebugMessageControl(GLEnum.DontCare, GLEnum.DontCare, GLEnum.DebugSeverityNotification, 0, null, false);

            //init systems
            scene = new Scene();
            physics = new Physics();
            physics.InitPhysics();

            Renderer.Init(scene);
            UiRenderer.Init();
            UiTextHandler.Init();
            defaultShader = new Shader(gl, "Default.vert", "Default.frag");
            defaultTexture = new Texture();
            defaultTexture.LoadTexture("RedDebug.png");

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
            GameManager.InitGame();
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
            
            totalTime += (float)dt;
            
            Input.Clear();
            Input.Update();
            Mouse.Update();
            
            if(lockMouse)
            {
                Vector2 mousePosMoved = Mouse.position - Mouse.lastPosition;
                player.Look((int)mousePosMoved.X, (int)mousePosMoved.Y, false, false);
            }
            
            for (int i = 0; i < scene.SpatialObjects.Count; i++)
            {
                scene.SpatialObjects[i].SO_mesh.SetModelMatrix();
            }
            
            GameManager.UpdateGame((float)dt);

            totalTimeUpdate += (float)dt * 1000;
            while (totalTimeUpdate >= fixedUpdateTime)
            {
                totalTimeUpdate -= fixedUpdateTime;
                FixedUpdate(fixedUpdateTime / 1000f);
            }
        }

        static void FixedUpdate(float dt)
        {
            if (Input.IsKeyDown(Key.V))
            {
                player.LaunchCube(ref scene);
                if (NetworkManager.didInit)
                {
                    SpawnSpatialObjectPacket packet = new SpawnSpatialObjectPacket(scene.SpatialObjects.Count - 1, scene.SpatialObjects[^1].SO_mesh.position, scene.SpatialObjects[^1].SO_mesh.rotation, scene.SpatialObjects[^1].SO_mesh.modelLocation, scene.SpatialObjects[^1].SO_rigidbody.settings.MotionType, bodyInterface.GetObjectLayer(scene.SpatialObjects[^1].SO_rigidbody.rbID), Activation.Activate);
                    NetworkManager.client.SendRelib(packet);
                }
            }
            player.Movement(dt);
            player.UpdatePlayer(dt);

            GameManager.FixedUpdateGame(dt);

            if (NetworkManager.didInit)
            {
                if(NetworkManager.serverStarted)
                {
                    NetworkManager.server.Update(dt);
                }
                if(NetworkManager.clientStarted)
                {
                    if(!NetworkManager.client.IsConnected())
                    {
                        physics.UpdatePhysics(ref scene, dt);
                    }
                    NetworkManager.client.Update(dt);
                }
            }
            else
            {
                physics.UpdatePhysics(ref scene, dt);
            }
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
            
            gl.UseProgram(defaultShader.shader);
            defaultShader.setVec3("lightPos", new Vector3(0,20,-10));
            defaultTexture.Bind();
            Renderer.Draw(scene, ref defaultShader, player.camera.viewMat, player.camera.projMat, player.camera.position);
            UiRenderer.Draw();

            //render players
            if(NetworkManager.didInit)
            {
                for (int i = 0; i < NetworkManager.client.playerMeshes.Count; i++)
                {
                    NetworkManager.client.playerMeshes[i].SetModelMatrix();
                    NetworkManager.client.playerMeshes[i].DrawMesh(ref defaultShader, player.camera.viewMat, player.camera.projMat, player.camera.position);
                }
            }

            SetNeededDebug(player.camera.projMat, player.camera.viewMat);
            DrawDebugItems();

            if (showImguiDebug)
            {
                controller.Render();
            }
        }
    }
}