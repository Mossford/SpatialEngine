using System;
using Silk.NET.Windowing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using ImGuiNET;
using Silk.NET.OpenGL.Extensions.ImGui;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using JoltPhysicsSharp;
using System.Runtime.InteropServices;


//Custom Engine things
using static SpatialEngine.Rendering.MeshUtils;
using static SpatialEngine.Rendering.MainImGui;
using static SpatialEngine.Resources;
using static SpatialEngine.Globals;
using SpatialEngine.Networking;
using SpatialEngine.Rendering;
//silk net has its own shader for some reason
using Shader = SpatialEngine.Rendering.Shader;
using Texture = SpatialEngine.Rendering.Texture;
using static SpatialEngine.Debugging;

using SpatialGame;

namespace SpatialEngine
{

    public static class Globals
    {
        public static GL gl;
        public static GraphicsAPI glApi = GraphicsAPI.Default;
        public static IWindow window;
        public const int SCR_WIDTH = 1920;
        public const int SCR_HEIGHT = 1080;
        public static IInputContext input;
        public static IKeyboard keyboard;
        public static string EngVer = "0.6.7";
        public static string OpenGlVersion = "";
        public static string Gpu = "";
        
        public static Scene scene;
        public static Physics physics;
        public static PhysicsSystem physicsSystem;
        public static BodyInterface bodyInterface;

        public static bool showWireFrame = false;
        //going to be true because my gpu squeals if vsync is off
        public static bool vsync = true;
        public static uint vertCount;
        public static uint indCount;

        public static Player player;

        public static uint DrawCallCount = 0;
        public static float totalTime = 0.0f;

        /// <summary>
        /// In Seconds
        /// </summary>
        /// <returns></returns>
        public static float GetTime() => totalTime;
    }

    public class Game
    {
        static ImGuiController controller;
        static Vector2 LastMousePosition;
        static Shader shader;
        static Texture texture;


        public static void Main(string[] args)
        {
            //init resources like resources paths
            InitResources();

            if (args.Length == 0)
            {
                glApi.Version = new APIVersion(4, 6);
                WindowOptions options = WindowOptions.Default with
                {
                    Size = new Vector2D<int>(SCR_WIDTH, SCR_HEIGHT),
                    Title = "GameTesting",
                    VSync = vsync,
                    PreferredDepthBufferBits = 24,
                    API = glApi,
                };
                window = Window.Create(options);
                window.Load += OnLoad;
                window.Update += OnUpdate;
                window.Render += OnRender;
                window.Run();
            }
            else if (args[0] == "server")
            {
                //handles all the initilization of scene and physics and netowrking for server
                //and starts running the update loop
                HeadlessServer.Init();
            }

            scene.SaveScene(ScenePath, "Main.txt");
            physics.CleanPhysics(ref scene);
            NetworkManager.Cleanup();
        }

        static unsafe void OnLoad() 
        {
            //hostThread = new Thread(host.Recive);
            //hostThread.Start();
            controller = new ImGuiController(gl = window.CreateOpenGL(), window, input = window.CreateInput());
            keyboard = input.Keyboards.FirstOrDefault();
            gl = GL.GetApi(window);
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
            gl.Enable(GLEnum.Texture2D);
            gl.Enable(GLEnum.CullFace);
            gl.Enable(GLEnum.DebugOutput);
            gl.DebugMessageCallback(DebugProc, null);
            gl.DebugMessageControl(GLEnum.DontCare, GLEnum.DontCare, GLEnum.DebugSeverityNotification, 0, null, false);

            scene = new Scene();
            physics = new Physics();
            physics.InitPhysics();
            
            scene.AddSpatialObject(LoadModel(new Vector3(0,0,0), Quaternion.Identity, ModelPath, "Floor.obj"), new Vector3(50,1,50), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(50,30,0), Quaternion.Identity, ModelPath, "FloorWall1.obj"), new Vector3(1,30,50), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(0,10,50), Quaternion.Identity, ModelPath, "FloorWall2.obj"), new Vector3(50,10,1), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(25,5,0), Quaternion.Identity, ModelPath, "FloorWall3.obj"), new Vector3(1,5,20), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(37,4,21), Quaternion.Identity, ModelPath, "FloorWall4.obj"), new Vector3(13,4,1), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(37,5,-21), Quaternion.Identity, ModelPath, "FloorWall5.obj"), new Vector3(13,4,1), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(-50,2,0), Quaternion.Identity, ModelPath, "FloorWall6.obj"), new Vector3(1,2,50), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(-30,3,-50), Quaternion.Identity, ModelPath, "FloorWall7.obj"), new Vector3(20,3,1), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);

            for (int i = 0; i < scene.SpatialObjects.Count; i++)
            {
                vertCount += (uint)scene.SpatialObjects[i].SO_mesh.vertexes.Length;
                indCount += (uint)scene.SpatialObjects[i].SO_mesh.indices.Length;
            }

            Renderer.Init(scene);

            player = new Player(15.0f, new Vector3(-33,12,-20), new Vector3(300, 15, 0));
            shader = new Shader(gl, "Default.vert", "Default.frag");
            texture = new Texture();
            texture.LoadTexture("RedDebug.png");

            ImGui.SetWindowSize(new Vector2(400, 600));

            //input stuffs
            for (int i = 0; i < input.Keyboards.Count; i++)
                input.Keyboards[i].KeyDown += KeyDown;
            for (int i = 0; i < input.Mice.Count; i++)
            {
                input.Mice[i].Cursor.CursorMode = CursorMode.Normal;
                input.Mice[i].MouseMove += OnMouseMove;
            }

            //start host after everything has init
            //host.Start();
            NetworkManager.Init();


            //init game
            GameManager.InitGame();
        }

        static bool lockMouse = false;
        static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
        {
            if(!lockMouse && key == Key.Escape)
            {
                input.Mice.FirstOrDefault().Cursor.CursorMode = CursorMode.Raw;
                lockMouse = true;
            }
            else if(lockMouse && key == Key.Escape)
            {
                input.Mice.FirstOrDefault().Cursor.CursorMode = CursorMode.Normal;
                lockMouse = false;
            }
        }

        static unsafe void OnMouseMove(IMouse mouse, Vector2 position)
        {
            if(lockMouse)
            {
                Vector2 mousePosMoved = position - LastMousePosition;
                LastMousePosition = position;
                player.Look((int)mousePosMoved.X, (int)mousePosMoved.Y, false, false);
                LastMousePosition = position;
            }
        }

        static List<int> keysPressed = new List<int>();
        static float totalTimeUpdate = 0.0f;
        static void OnUpdate(double dt)
        {
            totalTime += (float)dt;
            for (int i = 0; i < scene.SpatialObjects.Count; i++)
            {
                scene.SpatialObjects[i].SO_mesh.SetModelMatrix();
            }
            if (keyboard.IsKeyPressed(Key.W))
            {
                keysPressed.Add((int)Key.W);
            }
            if (keyboard.IsKeyPressed(Key.S))
            {
                keysPressed.Add((int)Key.S);
            }
            if (keyboard.IsKeyPressed(Key.A))
            {
                keysPressed.Add((int)Key.A);
            }
            if (keyboard.IsKeyPressed(Key.D))
            {
                keysPressed.Add((int)Key.D);
            }
            if (keyboard.IsKeyPressed(Key.Space))
            {
                keysPressed.Add((int)Key.Space);
            }
            if (keyboard.IsKeyPressed(Key.ShiftLeft))
            {
                keysPressed.Add((int)Key.ShiftLeft);
            }

            totalTimeUpdate += (float)dt;
            while (totalTimeUpdate >= 0.0166f)
            {
                totalTimeUpdate -= 0.0166f;
                FixedUpdate(0.0166f);
            }
            keysPressed.Clear();

            //GameManager.UpdateGame((float)dt);
        }

        static void FixedUpdate(float dt)
        {
            if (keyboard.IsKeyPressed(Key.V))
            {
                player.LaunchCube(ref scene, ModelPath);
                if (!NetworkManager.isServer && NetworkManager.didInit)
                {
                    SpawnSpatialObjectPacket packet = new SpawnSpatialObjectPacket(scene.SpatialObjects.Count - 1, scene.SpatialObjects[^1].SO_mesh.position, scene.SpatialObjects[^1].SO_mesh.rotation, scene.SpatialObjects[^1].SO_mesh.modelLocation, scene.SpatialObjects[^1].SO_rigidbody.settings.MotionType, bodyInterface.GetObjectLayer(scene.SpatialObjects[^1].SO_rigidbody.rbID), (Activation)Convert.ToInt32((bodyInterface.IsActive(scene.SpatialObjects[^1].SO_rigidbody.rbID))));
                    NetworkManager.client.SendRelib(packet);
                }
                vertCount += (uint)scene.SpatialObjects[scene.SpatialObjects.Count - 1].SO_mesh.vertexes.Length;
                indCount += (uint)scene.SpatialObjects[scene.SpatialObjects.Count - 1].SO_mesh.indices.Length;
            }
            player.Movement(dt, keysPressed.ToArray());
            player.UpdatePlayer(dt);
            //physics.UpdatePhysics(ref scene, dt);

            if (NetworkManager.didInit)
            {
                if(NetworkManager.isServer)
                {
                    NetworkManager.server.Update(dt);
                }
                else
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
            controller.Update((float)dt);

            player.camera.SetViewMat();
            player.camera.SetProjMat(window.Size.X, window.Size.Y);

            ImGuiMenu((float)dt);

            gl.ClearColor(Color.FromArgb(102, 178, 204));
            gl.Viewport(0,0, (uint)window.Size.X, (uint)window.Size.Y);

            gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            gl.DepthFunc(GLEnum.Less);
            gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);
            if(showWireFrame)
                gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line);

            gl.UseProgram(shader.shader);
            shader.setVec3("lightPos", new Vector3(0,10,-10));
            gl.ActiveTexture(GLEnum.Texture0);
            texture.Bind();
            Renderer.Draw(scene, ref shader, player.camera.viewMat, player.camera.projMat, player.camera.position);

            //render players
            if(NetworkManager.didInit && !NetworkManager.isServer)
            {
                for (int i = 0; i < NetworkManager.client.playerMeshes.Count; i++)
                {
                    NetworkManager.client.playerMeshes[i].SetModelMatrix();
                    NetworkManager.client.playerMeshes[i].DrawMesh(ref shader, player.camera.viewMat, player.camera.projMat, player.camera.position);
                }
            }

            SetNeededDebug(player.camera.projMat, player.camera.viewMat);
            DrawDebugItems();

            controller.Render();
        }

        

        static unsafe void DebugProc(GLEnum source, GLEnum type, int id, GLEnum severity, int length, nint msg, nint userParam)
        {
            string _source;
            string _type;
            string _severity;

            switch (source) 
            {
                case GLEnum.DebugSourceApi:
                _source = "API";
                break;

                case GLEnum.DebugSourceWindowSystem:
                _source = "WINDOW SYSTEM";
                break;

                case GLEnum.DebugSourceShaderCompiler:
                _source = "SHADER COMPILER";
                break;

                case GLEnum.DebugSourceThirdParty:
                _source = "THIRD PARTY";
                break;

                case GLEnum.DebugSourceApplication:
                _source = "APPLICATION";
                break;

                case GLEnum.DebugSourceOther:
                _source = "UNKNOWN";
                break;

                default:
                _source = "UNKNOWN";
                break;
            }

            switch (type) {
                case GLEnum.DebugTypeError:
                _type = "ERROR";
                break;

                case GLEnum.DebugTypeDeprecatedBehavior:
                _type = "DEPRECATED BEHAVIOR";
                break;

                case GLEnum.DebugTypeUndefinedBehavior:
                _type = "UDEFINED BEHAVIOR";
                break;

                case GLEnum.DebugTypePortability:
                _type = "PORTABILITY";
                break;

                case GLEnum.DebugTypePerformance:
                _type = "PERFORMANCE";
                break;

                case GLEnum.DebugTypeOther:
                _type = "OTHER";
                break;

                case GLEnum.DebugTypeMarker:
                _type = "MARKER";
                break;

                default:
                _type = "UNKNOWN";
                break;
            }

            switch (severity) {
                case GLEnum.DebugSeverityHigh:
                _severity = "HIGH";
                break;

                case GLEnum.DebugSeverityMedium:
                _severity = "MEDIUM";
                break;

                case GLEnum.DebugSeverityLow:
                _severity = "LOW";
                break;

                case GLEnum.DebugSeverityNotification:
                _severity = "NOTIFICATION";
                break;

                default:
                _severity = "UNKNOWN";
                break;
            }

            Console.WriteLine(string.Format("{0}: {1} of {2} severity, raised from {3}: {4}", id, _type, _severity, _source, msg));
        }
    }
}
