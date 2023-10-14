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


namespace GameTesting
{
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
        static GL gl;
        static Shader shader;
        static List<Mesh> meshes = new List<Mesh>();
        static Camera camera;
        static readonly string appPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static readonly string resourcePath = appPath + @"\res";
        static readonly string ShaderPath = resourcePath + @"/Shaders";


        public static void Main(string[] args)
        {
            WindowOptions options = WindowOptions.Default with
            {
                Size = new Vector2D<int>(SCR_WIDTH, SCR_HEIGHT),
                Title = "GameTesting",
                VSync = false
            };
            window = Window.Create(options);
            window.Load += OnLoad;
            window.Update += OnUpdate;
            window.Render += OnRender;
            window.Run();
        }

        static unsafe void OnLoad() 
        {
            controller = new ImGuiController(gl = window.CreateOpenGL(), window, input = window.CreateInput());
            gl.ClearColor(Color.DarkCyan);

            Vertex[] vertexes =
            {
                new Vertex(new Vector3(-1.0f, -1.0f, 1.0f),new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(-1.0f, 1.0f, 1.0f),new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(-1.0f, -1.0f, -1.0f),new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(-1.0f, 1.0f, -1.0f),new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3( 1.0f,-1.0f, 1.0f),new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(1.0f,1.0f, 1.0f),new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(1.0f,-1.0f, -1.0f),new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(1.0f,1.0f, -1.0f),new Vector3(0), new Vector2(0))
            };

            uint[] indices =
            {
                1, 2, 0,
                3, 6, 2,
                7, 4, 6,
                5, 0, 4,
                6, 0, 2,
                3, 5, 7,
                1, 3, 2,
                3, 7, 6,
                7, 5, 4,
                5, 1, 0,
                6, 4, 0,
                3, 1, 5
            };

            meshes.Add(new Mesh(gl, vertexes, indices));
            vertCount += (uint)meshes[0].vertexes.Length;
            indCount += (uint)meshes[0].indices.Length;
            camera = new Camera(new Vector3(0,0,-2), Quaternion.Identity, Vector3.Zero, 45f);
            shader = new Shader(gl, ShaderPath + @"\Default.vert", ShaderPath + @"\Default.frag");

            ImGui.SetWindowSize(new Vector2(400,600));

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
            /*for (int i = 0; i < meshes.Count; i++)
            {
                meshes[i].rotation.X = MathF.Sin(time * i);
                meshes[i].rotation.Y = MathF.Sin(time * i);
                meshes[i].rotation.Z = MathF.Sin(time * i);
            }*/
            //Console.WriteLine(boundingBox.max + " max " + boundingBox.min + " min");
            
        }

        static Vector4 color = new Vector4();

        static unsafe void OnRender(double dt)
        {   
            controller.Update((float)dt);

            ImGuiMenu(dt);

            camera.SetViewMat();

            gl.Enable(EnableCap.DepthTest);
            gl.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);
            if(showWireFrame)
                gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line);

            gl.UseProgram(shader.shader);
            shader.SetUniform("uColor", color);
            shader.SetUniform("uView", camera.viewMat);
            shader.SetUniform("uProj", camera.GetProjMat());

            for (int i = 0; i < meshes.Count; i++)
            {
                meshes[i].CreateViewMat();
                shader.SetUniform("uModel", meshes[i].modelMat);
                meshes[i].DrawMesh();
            }

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
            ImGui.Text(string.Format("Amount of Spatials: ({0})", meshes.Count));
            //ImGui.Text(string.Format("Ram Usage: {0:N2}mb", process.PrivateMemorySize64 / 1024.0f / 1024.0f));
            ImGui.Text(string.Format("Time Open {0:N1} minutes", (totalTime / 60.0f)));

            ImGui.Spacing();
            ImGui.Checkbox("Wire Frame", ref showWireFrame);

            ImGui.Text("Camera");
            ImGui.SliderFloat3("Camera Position", ref camera.position, -10, 10);

            for (int i = 0; i < meshes.Count; i++)
            {
                ImGui.Text("Cube " + i);
                ImGui.ColorEdit4("Cube Color", ref color);
                ImGui.SliderFloat3("Cube Position", ref meshes[i].position, 5, -5);
                ImGui.SliderAngle("Cube Rotation X", ref meshes[i].rotation.X, -360, 360);
                ImGui.SliderAngle("Cube Rotation Y", ref meshes[i].rotation.Y, -360, 360);
                ImGui.SliderAngle("Cube Rotation Z", ref meshes[i].rotation.Z, -360, 360);
                ImGui.SliderFloat("Cube Scale", ref meshes[i].scale, -5, 5);
            }
        }
    }
}