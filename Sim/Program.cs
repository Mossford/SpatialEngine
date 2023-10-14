using System;
using Silk.NET.Windowing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Collections.Generic;


namespace GameTesting
{
    public class Game
    {

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
        static string appPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static string resourcePath = appPath + @"\res";
        static string ShaderPath = resourcePath + @"/Shaders";

        static float time = 0;


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

            float[] vertices =
            {
                -1.0f, -1.0f,  1.0f,
                1.0f, -1.0f,  1.0f,
                1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                -1.0f, -1.0f, -1.0f,
                1.0f, -1.0f, -1.0f,
                1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f
            };

            uint[] indices =
            {
                0, 1, 2,
		        2, 3, 0,
                1, 5, 6,
                6, 2, 1,
                7, 6, 5,
                5, 4, 7,
                4, 0, 3,
                3, 7, 4,
                4, 5, 1,
                1, 0, 4,
                3, 2, 6,
                6, 7, 3
            };

            meshes.Add(new Mesh(gl, vertices, indices));
            meshes.Add(new Mesh(gl, vertices, indices));
            meshes.Add(new Mesh(gl, vertices, indices));
            meshes.Add(new Mesh(gl, vertices, indices));
            camera = new Camera(new Vector3(0,0,-2), Quaternion.Identity, Vector3.Zero, 45f);
            shader = new Shader(gl, ShaderPath + @"\Default.vert", ShaderPath + @"\Default.frag");

            ImGuiNET.ImGui.SetWindowSize(new Vector2(400,600));

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

        static Vector3 dir;
        static bool check;
        static unsafe void OnMouseMove(IMouse mouse, Vector2 position)
        {
            if (LastMousePosition == default) { LastMousePosition = position; }
            else
            {
                
            }
        }

        static void OnUpdate(double dt) 
        {
            time += (float)dt;
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
            
            ImGuiNET.ImGui.Begin("GameTesting");

            ImGuiNET.ImGui.SliderFloat3("Ray Direction", ref dir, -1, 1);
            ImGuiNET.ImGui.Checkbox("Inside", ref check);


            ImGuiNET.ImGui.Text("Camera");
            ImGuiNET.ImGui.SliderFloat3("Camera Position", ref camera.position, -10, 10);

            for (int i = 0; i < meshes.Count; i++)
            {
                ImGuiNET.ImGui.Text("Cube " + i);
                ImGuiNET.ImGui.ColorEdit4("Cube Color", ref color);
                ImGuiNET.ImGui.SliderFloat3("Cube Position", ref meshes[i].position, 5, -5);
                ImGuiNET.ImGui.SliderAngle("Cube Rotation X", ref meshes[i].rotation.X, -360, 360);
                ImGuiNET.ImGui.SliderAngle("Cube Rotation Y", ref meshes[i].rotation.Y, -360, 360);
                ImGuiNET.ImGui.SliderAngle("Cube Rotation Z", ref meshes[i].rotation.Z, -360, 360);
                ImGuiNET.ImGui.SliderFloat("Cube Scale", ref meshes[i].scale, -5, 5);
            }

            camera.SetViewMat();

            gl.Enable(EnableCap.DepthTest);
            gl.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

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
    }
}