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
using static SpatialEngine.MeshUtils;
using static SpatialEngine.Globals;
using SpatialEngine.Networking;
using System.Threading;


namespace SpatialEngine
{

    public static class Globals
    {
        public static GL gl;
        public static GraphicsAPI glApi = GraphicsAPI.Default;
        public static IWindow window;
        public static IInputContext input;
        public static IKeyboard keyboard;
        public static string EngVer = "0.5.0";
        public static string OpenGlVersion = "";
        public static string Gpu = "";
        
        public static Scene scene;
        public static Renderer renderer;
        public static Physics physics;
        public static PhysicsSystem physicsSystem;
        public static BodyInterface bodyInterface;

        public static readonly string appPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public static readonly string resourcePath = appPath + @"/res/";
        public static readonly string ShaderPath = resourcePath + @"Shaders/";
        public static readonly string ImagePath = resourcePath + @"Images/";
        public static readonly string ModelPath = resourcePath + @"Models/";

        public static float drawCallAvg = 0.0f;
        public static uint DrawCallCount = 0;

        public static float GetTime()
        {
            return (float)window.Time;
        }
    }

    public class Game
    {
        static bool showWireFrame = false;
        static bool vsync = false;
        static uint vertCount;
        static uint indCount;
        static float totalTime = 0.0f;
        public const int SCR_WIDTH = 1920;
        public const int SCR_HEIGHT = 1080;
        static ImGuiController controller;
        static Vector2 LastMousePosition;
        static Shader shader;
        //static Host host = new Host(1920, "192.168.1.177");
        static Client client = new Client(1920, "127.0.0.1");
        static Thread hostThread;
        static Player player;


        public static void Main(string[] args)
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

            physics.CleanPhysics(ref scene);
            //host.Close();
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
            renderer = new Renderer();
            physics = new Physics();
            physics.InitPhysics();
            
            scene.AddSpatialObject(LoadModel(new Vector3(0,0,0), Quaternion.Identity, ModelPath + "Floor.obj"), new Vector3(50,1,50), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(50,30,0), Quaternion.Identity, ModelPath + "FloorWall1.obj"), new Vector3(1,30,50), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(0,10,50), Quaternion.Identity, ModelPath + "FloorWall2.obj"), new Vector3(50,10,1), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(25,5,0), Quaternion.Identity, ModelPath + "FloorWall3.obj"), new Vector3(1,5,20), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(37,4,21), Quaternion.Identity, ModelPath + "FloorWall4.obj"), new Vector3(13,4,1), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(37,5,-21), Quaternion.Identity, ModelPath + "FloorWall5.obj"), new Vector3(13,4,1), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(-50,2,0), Quaternion.Identity, ModelPath + "FloorWall6.obj"), new Vector3(1,2,50), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(-30,3,-50), Quaternion.Identity, ModelPath + "FloorWall7.obj"), new Vector3(20,3,1), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(0,5,0), Quaternion.Identity, ModelPath + "Bunnysmooth.obj"), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            //scene.AddSpatialObject(CreateSphereMesh(new Vector3(0,10,0), new Quaternion(0.1f, 0.1f, 0.1f, 1), 3), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);



            for (int i = 0; i < scene.SpatialObjects.Count; i++)
            {
                vertCount += (uint)scene.SpatialObjects[i].SO_mesh.vertexes.Length;
                indCount += (uint)scene.SpatialObjects[i].SO_mesh.indices.Length;
            }

            renderer.Init(scene);

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

            //start host after everything has init
            //host.Start();
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
                SpatialObjectPacket packet = new SpatialObjectPacket(i, scene.SpatialObjects[i].SO_mesh.position, scene.SpatialObjects[i].SO_mesh.rotation);
                client.Send(packet.ConvertToByte());
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

            int counter = 0;
            totalTimeUpdate += (float)dt;
            while (totalTimeUpdate >= 0.016f)
            {
                totalTimeUpdate -= 0.016f;
                FixedUpdate(0.016f);
                //counter++;

            }
            keysPressed.Clear();

        }

        static void FixedUpdate(float dt)
        {
            if (keyboard.IsKeyPressed(Key.V))
            {
                player.LaunchSphere(ref scene, ModelPath + "Torus.obj");
                SpawnSpatialObjectPacket packet = new SpawnSpatialObjectPacket(scene.SpatialObjects.Count - 1, scene.SpatialObjects[^1].SO_mesh.position, scene.SpatialObjects[^1].SO_mesh.rotation, scene.SpatialObjects[^1].SO_mesh.modelLocation, scene.SpatialObjects[^1].SO_rigidbody.settings.MotionType, bodyInterface.GetObjectLayer(scene.SpatialObjects[^1].SO_rigidbody.rbID), (Activation)Convert.ToInt32((bodyInterface.IsActive(scene.SpatialObjects[^1].SO_rigidbody.rbID))));
                client.Send(packet.ConvertToByte());
                vertCount += (uint)scene.SpatialObjects[scene.SpatialObjects.Count - 1].SO_mesh.vertexes.Length;
                indCount += (uint)scene.SpatialObjects[scene.SpatialObjects.Count - 1].SO_mesh.indices.Length;
            }
            scene.SpatialObjects[8].SO_mesh.position += new Vector3(0, 0.01f, 0);
            player.Movement(0.016f, keysPressed.ToArray());
            player.UpdatePlayer(0.016f);
            physics.UpdatePhysics(ref scene, dt);
        }

        static unsafe void OnRender(double dt)
        {   
            controller.Update((float)dt);

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
            renderer.Draw(scene, ref shader, player.camera.GetViewMat(), player.camera.GetProjMat(window.Size.X, window.Size.Y), player.camera.position);

            controller.Render();
        }

        class ScrollingBuffer 
        {
            int MaxSize;
            int Offset;
            List<Vector2> Data;
            public ScrollingBuffer(int max_size = 2000) 
            {
                MaxSize = max_size;
                Offset  = 0;
                Data = new List<Vector2>(MaxSize);
            }
            public void AddPoint(float x, float y) 
            {
                if (Data.Count< MaxSize)
                    Data.Add(new Vector2(x,y));
                else 
                {
                    Data[Offset] = new Vector2(x,y);
                    Offset =  (Offset + 1) % MaxSize;
                }
            }
            public void Erase() 
            {
                if (Data.Count() > 0) 
                {
                    Data.Clear();
                    Offset  = 0;
                }
            }
        }

        private static ScrollingBuffer frameTimes = new ScrollingBuffer(20000);
        private static float HighestFT = 0.0f;
        private static bool ShowSceneViewerMenu, ShowObjectViewerMenu, ShowConsoleViewerMenu;
        private static int IMM_counter = 0;
        private static Vector3 IMM_selposition = new Vector3();
        private static Vector3 IMM_selrotation = new Vector3();
        private static Vector3 IMM_selvelocity = new Vector3();
        private static Vector3 IMM_selvelocityrot = new Vector3();
        private static int IMM_IcoSphereSub = 0;
        private static float IMM_SpikerSphereSize = 0;
        private static string IMM_input = "";
        private static bool IMM_static = false;
        static void ImGuiMenu(float deltaTime)
        {
            if(deltaTime > HighestFT)
                HighestFT = (float)deltaTime;
            frameTimes.AddPoint(GetTime(), deltaTime);

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
            ImGui.Text(String.Format("{0} verts, {1} indices ({2} tris)", vertCount, indCount, indCount / 3));
            ImGui.Text(String.Format("RenderSets: {0}", renderer.renderSets.Count));
            ImGui.Text(String.Format("Amount of Spatials: ({0})", scene.SpatialObjects.Count()));
            ImGui.Text(String.Format("DrawCall Avg: ({0:N1}) DC/frame, DrawCall Total ({1})", drawCallAvg, DrawCallCount));
            ImGui.Text(String.Format("Time Open %.1f minutes", (GetTime() / 60.0f)));
            //ImGui.Text(String.Format("Time taken for Update run %.2fms ", MathF.Abs(updateTime)));
            //ImGui.Text(String.Format("Time taken for Fixed Update run %.2fms ", MathF.Abs(updateFixedTime)));

            //float frameTimeHistory = 2.75f;
            /*ImGui.SliderFloat("FrameTimeHistory", ref frameTimeHistory, 0.1f, 10.0f);
            if (ImPlot.BeginPlot("##Scrolling", ImVec2(ImGui::GetContentRegionAvail().x,100))) 
            {
                ImPlot.SetupAxes(nullptr, nullptr, ImPlotAxisFlags_NoTickLabels, ImPlotAxisFlags_AutoFit);
                ImPlot.SetupAxisLimits(ImAxis_X1,GetTime() - frameTimeHistory, GetTime(), ImGuiCond_Always);
                ImPlot.SetupAxisLimits(ImAxis_Y1,0,HighestFT + (HighestFT * 0.25f), ImGuiCond_Always);
                ImPlot.SetNextFillStyle(ImVec4(0,0.5,0.5,1),1.0f);
                ImPlot.PlotShaded("FrameTime", &frameTimes.Data[0].x, &frameTimes.Data[0].y, frameTimes.Data.size(), -INFINITY, 0, frameTimes.Offset, 2 * sizeof(float));
                ImPlot.EndPlot();
            }*/
            ImGui.Checkbox("Wire Frame", ref showWireFrame);
            if(ImGui.Checkbox("Vsync", ref vsync))
            {
                window.VSync = vsync;
            }

            ImGui.Spacing();
            //ImGui.DragFloat("Physics Speed", &PhysicsSpeed, 0.01f, -10.0f, 10.0f);
            ImGui.DragFloat3("Player Position", ref player.position, 1.0f, -50.0f, 50.0f);
            ImGui.DragFloat3("Player Rotation", ref player.rotation, 1.0f, -360.0f, 360.0f);
            ImGui.SliderFloat("Cam Fov", ref player.camera.zoom, 179.9f, 0.01f);
            Vector3 chunkpos = player.position / 10;
            ImGui.Text(String.Format("Player in ChunkPos: {0} {1} {2}", (int)chunkpos.X, (int)chunkpos.Y, (int)chunkpos.Z));

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Menus"))
                {
                    ImGui.MenuItem("Scene Viewer", null, ref ShowSceneViewerMenu);
                    ImGui.MenuItem("Object Viewer", null, ref ShowObjectViewerMenu);
                    ImGui.MenuItem("Console Viewer", null, ref ShowConsoleViewerMenu);
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            if(ShowSceneViewerMenu)
            {
                ImGui.SetNextWindowSize(new Vector2(600,420), ImGuiCond.FirstUseEver);
                ImGui.Begin("Scene Viewer");

                if (ImGui.TreeNode("Objects"))
                {
                    for (int i = 0; i < scene.SpatialObjects.Count(); i++)
                    {
                        if (ImGui.TreeNode(string.Format("Object {0}", scene.SpatialObjects[i].SO_id)))
                        {
                            ImGui.DragFloat3("Object Position", ref scene.SpatialObjects[i].SO_mesh.position, 0.05f, -100000.0f, 100000.0f);
                            ImGui.TreePop();
                        }
                    }
                    ImGui.TreePop();
                }
                /*if(ImGui.TreeNode("Scenes"))
                {
                    std.vector<std::string> files = GetFiles(sceneLoc);
                    for (unsigned int i = 0; i < files.size(); i++)
                    {
                        if (ImGui::TreeNode((void*)(intptr_t)i, "Scene: %s", files[i].c_str()))
                        {
                            ImGui::SameLine();
                            if(ImGui::Button("Load"))
                            {
                                LoadScene(sceneLoc, files[i], mainScene);
                            }
                            ImGui::TreePop();
                        }
                    }
                    ImGui::TreePop();
                }*/

                ImGui.End();
            }

            if(ShowObjectViewerMenu)
            {
                ImGui.SetNextWindowSize(new Vector2(600,420), ImGuiCond.FirstUseEver);
                ImGui.Begin("Object Viewer");

                //select the object you want gives properties 
                //or create a object and that selects by default
                ImGui.Text("Select Object Properties");
                ImGui.Spacing();
                if (ImGui.ArrowButton("##left", ImGuiDir.Left)) 
                { 
                    if(IMM_counter > (int)MeshType.First)
                        IMM_counter--;
                }
                ImGui.SameLine(0.0f, 1.0f);
                if (ImGui.ArrowButton("##right", ImGuiDir.Right)) 
                { 
                    if(IMM_counter < (int)MeshType.Last)
                        IMM_counter++; 
                }
                ImGui.SameLine();
                ImGui.Text(((MeshType)IMM_counter).ToString());
                if(IMM_counter == (int)MeshType.IcoSphereMesh)
                    ImGui.SliderInt("Subdivison level", ref IMM_IcoSphereSub, 0, 10);
                if(IMM_counter == (int)MeshType.SpikerMesh)
                {
                    ImGui.SliderInt("Subdivison level", ref IMM_IcoSphereSub, 0, 10);
                    ImGui.SliderFloat("Size", ref IMM_SpikerSphereSize, 0.01f, 10);
                }
                if(IMM_counter == (int)MeshType.FileMesh)
                {
                    if(ImGui.TreeNode("Models"))
                    {
                        /*static std::vector<std::string> files = GetFiles(modelLoc);
                        for (unsigned int i = 0; i < files.size(); i++)
                        {
                            if (i == 0)
                                ImGui::SetNextItemOpen(true, ImGuiCond_Once);

                            if (ImGui::TreeNode((void*)(intptr_t)i, "Model: %s", files[i].c_str()))
                            {
                                ImGui::SameLine();
                                if(ImGui::Button("Set"))
                                {
                                    input = files[i];
                                }
                                ImGui::TreePop();
                            }
                        }*/
                        ImGui.TreePop();
                    }
                }
                else
                {
                    IMM_input = ((MeshType)IMM_counter).ToString(); 
                }
                ImGui.Checkbox("Static", ref IMM_static);
                ImGui.InputFloat3("Object Position", ref IMM_selposition);
                ImGui.InputFloat3("Object Rotation", ref IMM_selrotation);
                ImGui.InputFloat3("Object Velocity", ref IMM_selvelocity);
                ImGui.InputFloat3("Object Rotation Velocity", ref IMM_selvelocityrot);

                ImGui.Text("Current Model: " + IMM_input);

                if(ImGui.Button("Add Object"))
                {
                    Mesh selmesh;
                    int id = scene.SpatialObjects.Count;
                    switch (IMM_counter)
                    {
                    case (int)MeshType.CubeMesh:
                        selmesh = CreateCubeMesh(IMM_selposition, new Quaternion(IMM_selrotation, 1.0f));
                        vertCount += (uint)selmesh.vertexes.Length;
                        indCount += (uint)selmesh.indices.Length;
                        if(IMM_static)
                            scene.AddSpatialObject(selmesh, new Vector3(1.0f), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
                        else
                            scene.AddSpatialObject(selmesh, new Vector3(1.0f), MotionType.Dynamic, Layers.MOVING, Activation.Activate);
                        break;
                    case (int)MeshType.IcoSphereMesh:
                        selmesh = CreateSphereMesh(IMM_selposition, new Quaternion(IMM_selrotation * MathF.PI / 180.0f, 1.0f), (uint)IMM_IcoSphereSub);
                        vertCount += (uint)selmesh.vertexes.Length;
                        indCount += (uint)selmesh.indices.Length;
                        if(IMM_static)
                            scene.AddSpatialObject(selmesh, 1.0f, MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
                        else
                            scene.AddSpatialObject(selmesh, 1.0f, MotionType.Dynamic, Layers.MOVING, Activation.Activate);
                        break;
                    case (int)MeshType.SpikerMesh:
                        selmesh = CreateSpikerMesh(IMM_selposition, new Quaternion(IMM_selrotation * MathF.PI / 180.0f, 1.0f), IMM_SpikerSphereSize, IMM_IcoSphereSub);
                        vertCount += (uint)selmesh.vertexes.Length;
                        indCount += (uint)selmesh.indices.Length;
                        if(IMM_static)
                            scene.AddSpatialObject(selmesh, MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
                        else
                            scene.AddSpatialObject(selmesh, MotionType.Dynamic, Layers.MOVING, Activation.Activate);
                        break;
                    case (int)MeshType.TriangleMesh:
                        selmesh = Create2DTriangle(IMM_selposition, new Quaternion(IMM_selrotation * MathF.PI / 180.0f, 1.0f));
                        vertCount += (uint)selmesh.vertexes.Length;
                        indCount += (uint)selmesh.indices.Length;
                        if(IMM_static)
                            scene.AddSpatialObject(selmesh, MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
                        else
                            scene.AddSpatialObject(selmesh, MotionType.Dynamic, Layers.MOVING, Activation.Activate);
                        break;
                    case (int)MeshType.FileMesh:
                        if(!File.Exists(IMM_input))
                        {
                            ImGui.OpenPopup("Error");
                        }
                        else
                        {
                            scene.AddSpatialObject(LoadModel(IMM_selposition, new Quaternion(IMM_selrotation * MathF.PI / 180.0f, 1.0f), ModelPath + input));
                            vertCount += (uint)scene.SpatialObjects[id].SO_mesh.vertexes.Length;
                            indCount += (uint)scene.SpatialObjects[id].SO_mesh.indices.Length;
                            scene.SpatialObjects[id].SO_rigidbody.SetVelocity(IMM_selvelocity);
                            scene.SpatialObjects[id].SO_rigidbody.SetAngularVelocity(IMM_selvelocityrot);
                        }
                        break;
                    }
                }
                if(ImGui.BeginPopup("Error"))
                {
                    string text = "Model Not Found " + IMM_input;
                    ImGui.Text(text);
                    ImGui.EndPopup();
                }

                ImGui.End();
            }

            if(ShowConsoleViewerMenu)
            {
                ImGui.SetNextWindowSize(new Vector2(600,420), ImGuiCond.FirstUseEver);
                ImGui.Begin("Console Viewer");
                ImGui.End();
            }
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
