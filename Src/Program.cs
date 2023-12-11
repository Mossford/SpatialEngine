using System;
using Silk.NET.Windowing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using ImGuiNET;
using Silk.NET.Core;
using System.Linq;


//Custom Engine things
using static SpatialEngine.MeshUtils;
using static SpatialEngine.Globals;


namespace SpatialEngine
{

    public static class Globals
    {
        public static GL gl;
        public static IWindow window;
        public static IInputContext input;
        public static IKeyboard keyboard;
    }

    public class Game
    {
        static bool showWireFrame = false;
        static uint vertCount;
        static uint indCount;
        static float totalTime = 0.0f;
        public const int SCR_WIDTH = 1920;
        public const int SCR_HEIGHT = 1080;
        static ImGuiController controller;
        static Vector2 LastMousePosition;
        static Shader shader;
        static Scene scene = new Scene();
        static Physics physics = new Physics();
        static Player player;
        static readonly string appPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static readonly string resourcePath = appPath + @"/res/";
        static readonly string ShaderPath = resourcePath + @"Shaders/";
        static readonly string ImagePath = resourcePath + @"Images/";
        static readonly string ModelPath = resourcePath + @"Models/";


        public static void Main(string[] args)
        {
            WindowOptions options = WindowOptions.Default with
            {
                Size = new Vector2D<int>(SCR_WIDTH, SCR_HEIGHT),
                Title = "GameTesting",
                VSync = false,
                PreferredDepthBufferBits = 24
            };
            window = Window.Create(options);
            window.Load += OnLoad;
            window.Update += OnUpdate;
            window.Render += OnRender;
            window.Run();

            physics.CleanPhysics();
        }

        static unsafe void OnLoad() 
        {
            controller = new ImGuiController(gl = window.CreateOpenGL(), window, input = window.CreateInput());
            keyboard = input.Keyboards.FirstOrDefault();
            gl = GL.GetApi(window);
            gl.Enable(GLEnum.DepthTest);
            gl.Enable(GLEnum.Texture2D);
            gl.Enable(GLEnum.CullFace);
            gl.Enable(GLEnum.DebugOutput);
            gl.DebugMessageCallback(DebugProc, null);
            gl.DebugMessageControl(GLEnum.DontCare, GLEnum.DontCare, GLEnum.DebugSeverityNotification, 0, null, false);

            physics.InitPhysics();
            
            scene.AddSpatialObject(LoadModel(new Vector3(0,0,0), new Vector3(0,0,0), ModelPath + "Floor.obj"));
            scene.AddSpatialObject(LoadModel(new Vector3(5,2,0), new Vector3(0,0,0), ModelPath + "Bunny.obj"));
            scene.AddSpatialObject(LoadModel(new Vector3(-5,4,0), new Vector3(0,0,0), ModelPath + "Teapot.obj"));
            
            for (int i = 0; i < scene.SpatialObjects.Count; i++)
            {
                vertCount += (uint)scene.SpatialObjects[i].SO_mesh.vertexes.Length;
                indCount += (uint)scene.SpatialObjects[i].SO_mesh.indices.Length;
            }
            player = new Player(15.0f, new Vector3(-33,12,-20), new Vector3(300, 15, 0));
            shader = new Shader(gl, ShaderPath + "Default.vert", ShaderPath + "Default.frag");

            ImGui.SetWindowSize(new Vector2(400, 600));

            //input stuffs
            for (int i = 0; i < input.Keyboards.Count; i++)
                input.Keyboards[i].KeyDown += KeyDown;
            for (int i = 0; i < input.Mice.Count; i++)
            {
                input.Mice[i].Cursor.CursorMode = CursorMode.Normal;
                input.Mice[i].MouseMove += OnMouseMove;
            }
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
            Vector2 mousePosMoved = position - LastMousePosition;
            LastMousePosition = position;
            player.Look((int)mousePosMoved.X, (int)mousePosMoved.Y, false, false);
            LastMousePosition = position;
        }

        static List<int> keysPressed = new List<int>();
        static float totalTimeUpdate = 0.0f;
        static void OnUpdate(double dt) 
        {
            totalTime += (float)dt;
            for (int i = 0; i < scene.SpatialObjects.Count; i++)
            {
                //scene.SpatialObjects[i].SO_mesh.rotation = new Vector3(MathF.Sin(totalTime), MathF.Sin(totalTime), 0.0f);
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

            physics.UpdatePhysics(ref scene, (float)dt);

            int counter = 0;
            totalTimeUpdate += (float)dt;
            while (totalTimeUpdate >= 0.016f)
            {
                FixedUpdate((float)dt);
                counter++;

                if(dt >= 0.016f)
                {
                    if(counter == 3)
                        break;
                    totalTimeUpdate -= 0.016f;
                }
                else
                {
                    totalTimeUpdate = 0;
                    break;
                }
            }
            keysPressed.Clear();
        }

        static void FixedUpdate(float dt)
        {
            player.Movement(0.016f, keysPressed.ToArray());
            player.UpdatePlayer(0.016f);
        }

        static unsafe void OnRender(double dt)
        {   
            controller.Update((float)dt);

            ImGuiMenu(dt);

            gl.ClearColor(Color.FromArgb(102, 178, 204));
            gl.Viewport(0,0, (uint)window.Size.X, (uint)window.Size.Y);

            gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            gl.DepthFunc(GLEnum.Less);
            gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);
            if(showWireFrame)
                gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line);

            gl.UseProgram(shader.shader);
            shader.SetUniform("lightPos", new Vector3(0,10,-10));
            scene.DrawSingle(ref shader, player.camera.GetViewMat(), player.camera.GetProjMat(window.Size.X, window.Size.Y), player.camera.position);

            controller.Render();
        }

        static void ImGuiMenu(double deltaTime)
        {
            ImGuiWindowFlags window_flags = 0;
            window_flags |= ImGuiWindowFlags.NoTitleBar;
            window_flags |= ImGuiWindowFlags.MenuBar;

            ImGui.Begin("SpatialEngine", window_flags);

            ImGui.Text(string.Format("App avg {0:N3} ms/frame ({1:N1} FPS)", deltaTime * 1000, Math.Round(1.0f / deltaTime)));
            ImGui.Text(string.Format("{0} verts, {1} indices ({2} tris)", vertCount, indCount, indCount / 3));
            ImGui.Text(string.Format("Amount of Spatials: ({0})", scene.SpatialObjects.Count));
            //ImGui.Text(string.Format("Ram Usage: {0:N2}mb", process.PrivateMemorySize64 / 1024.0f / 1024.0f));
            ImGui.Text(string.Format("Time Open {0:N1} minutes", (totalTime / 60.0f)));

            ImGui.Spacing();
            ImGui.Checkbox("Wire Frame", ref showWireFrame);

            ImGui.Text("Camera");
            ImGui.SliderFloat3("Camera Position", ref player.camera.position, -10, 10);
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

            Console.WriteLine("%d: %s of %s severity, raised from %s: %s\n", id, _type, _severity, _source, msg);
        }
    }
}