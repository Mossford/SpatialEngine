using System;
using Silk.NET.Windowing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Diagnostics;
using System.Collections.Generic;
using ImGuiNET;
using Silk.NET.Core;


//Custom Engine things
using static SpatialEngine.MeshUtils;


namespace SpatialEngine
{

    public static class Globals
    {
        public static GL gl;
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
        static IInputContext input;
        private static Vector2 LastMousePosition;
        static IWindow window;
        static Shader shader;
        static Scene scene = new Scene();
        static Camera camera;
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
        }

        static unsafe void OnLoad() 
        {
            controller = new ImGuiController(Globals.gl = window.CreateOpenGL(), window, input = window.CreateInput());
            Globals.gl = GL.GetApi(window);
            Globals.gl.Enable(GLEnum.DepthTest);
            Globals.gl.Enable(GLEnum.Texture2D);
            Globals.gl.Enable(GLEnum.CullFace);
            Globals.gl.Enable(GLEnum.DebugOutput);
            Globals.gl.DebugMessageCallback(DebugProc, null);
            Globals.gl.DebugMessageControl(GLEnum.DontCare, GLEnum.DontCare, GLEnum.DebugSeverityNotification, 0, null, false);
            
            scene.AddSpatialObject(LoadModel(new Vector3(0,0,0), new Vector3(0,0,0), ModelPath + "Floor.obj"));
            scene.AddSpatialObject(LoadModel(new Vector3(5,2,0), new Vector3(0,0,0), ModelPath + "Bunny.obj"));
            scene.AddSpatialObject(LoadModel(new Vector3(-5,4,0), new Vector3(0,0,0), ModelPath + "Teapot.obj"));
            
            for (int i = 0; i < scene.SpatialObjects.Count; i++)
            {
                vertCount += (uint)scene.SpatialObjects[i].SO_mesh.vertexes.Length;
                indCount += (uint)scene.SpatialObjects[i].SO_mesh.indices.Length;
            }
            camera = new Camera(new Vector3(-33,12,-20), new Quaternion(300, 15, 0, 0), Vector3.Zero, 60f);
            shader = new Shader(Globals.gl, ShaderPath + "Default.vert", ShaderPath + "Default.frag");

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

        static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
        {
            
        }

        static unsafe void OnMouseMove(IMouse mouse, Vector2 position)
        {
            if (LastMousePosition == default) { LastMousePosition = position; }
            else
            {
                
            }
        }

        static void OnUpdate(double dt) 
        {
            totalTime += (float)dt;
            for (int i = 0; i < scene.SpatialObjects.Count; i++)
            {
                //scene.SpatialObjects[i].SO_mesh.rotation = new Vector3(MathF.Sin(totalTime), MathF.Sin(totalTime), 0.0f);
                scene.SpatialObjects[i].SO_mesh.SetModelMatrix();
            }
        }

        static unsafe void OnRender(double dt)
        {   
            controller.Update((float)dt);

            ImGuiMenu(dt);

            Globals.gl.ClearColor(Color.FromArgb(102, 178, 204));
            Globals.gl.Viewport(0,0, (uint)window.Size.X, (uint)window.Size.Y);

            Globals.gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Globals.gl.DepthFunc(GLEnum.Less);
            Globals.gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);
            if(showWireFrame)
                Globals.gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line);

            Globals.gl.UseProgram(shader.shader);
            shader.SetUniform("lightPos", new Vector3(0,10,-10));
            scene.DrawSingle(ref shader, camera.GetViewMat(), camera.GetProjMat(window.Size.X, window.Size.Y), camera.position);

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
            ImGui.SliderFloat3("Camera Position", ref camera.position, -10, 10);
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