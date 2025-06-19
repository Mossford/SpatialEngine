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
    public static class ObjectViewer
    {
        static int counter = 0;
        static Vector3 selposition = new Vector3();
        static Vector3 selrotation = new Vector3();
        static Vector3 selvelocity = new Vector3();
        static Vector3 selvelocityrot = new Vector3();
        static int icoSphereSub = 0;
        static float spikerSphereSize = 0;
        static string textInput = "";
        static bool staticObject = false;
        
        public static void Draw()
        {
            ImGui.SetNextWindowSize(new Vector2(600, 420), ImGuiCond.FirstUseEver);
            ImGui.Begin("Object");

            //select the object you want gives properties 
            //or create a object and that selects by default
            ImGui.Text("Select Object Properties");
            ImGui.Spacing();
            if (ImGui.ArrowButton("##left", ImGuiDir.Left))
            {
                if (counter > (int)MeshType.First)
                    counter--;
            }
            ImGui.SameLine(0.0f, 1.0f);
            if (ImGui.ArrowButton("##right", ImGuiDir.Right))
            {
                if (counter < (int)MeshType.Last)
                    counter++;
            }
            ImGui.SameLine();
            ImGui.Text(((MeshType)counter).ToString());
            if (counter == (int)MeshType.IcoSphereMesh)
                ImGui.SliderInt("Subdivison level", ref icoSphereSub, 0, 10);
            if (counter == (int)MeshType.SpikerMesh)
            {
                ImGui.SliderInt("Subdivison level", ref icoSphereSub, 0, 10);
                ImGui.SliderFloat("Size", ref spikerSphereSize, 0.01f, 10);
            }
            if (counter == (int)MeshType.FileMesh)
            {
                if (ImGui.TreeNode("Models"))
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
                textInput = ((MeshType)counter).ToString();
            }
            ImGui.Checkbox("Static", ref staticObject);
            ImGui.InputFloat3("Object Position", ref selposition);
            ImGui.InputFloat3("Object Rotation", ref selrotation);
            ImGui.InputFloat3("Object Velocity", ref selvelocity);
            ImGui.InputFloat3("Object Rotation Velocity", ref selvelocityrot);
            ImGui.Text("Current Model: " + textInput);

            if (ImGui.Button("Add Object"))
            {
                Mesh selmesh;
                int id = scene.SpatialObjects.Count;
                switch (counter)
                {
                    case (int)MeshType.CubeMesh:
                        selmesh = CreateCubeMesh(selposition, new Quaternion(selrotation, 1.0f));
                        vertCount += (uint)selmesh.vertexes.Length;
                        indCount += (uint)selmesh.indices.Length;
                        if (staticObject)
                            scene.AddSpatialObject(selmesh, new Vector3(1.0f), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
                        else
                            scene.AddSpatialObject(selmesh, new Vector3(1.0f), MotionType.Dynamic, Layers.MOVING, Activation.Activate);
                        break;
                    case (int)MeshType.IcoSphereMesh:
                        selmesh = CreateSphereMesh(selposition, new Quaternion(selrotation * MathF.PI / 180.0f, 1.0f), (uint)icoSphereSub);
                        vertCount += (uint)selmesh.vertexes.Length;
                        indCount += (uint)selmesh.indices.Length;
                        if (staticObject)
                            scene.AddSpatialObject(selmesh, 1.0f, MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
                        else
                            scene.AddSpatialObject(selmesh, 1.0f, MotionType.Dynamic, Layers.MOVING, Activation.Activate);
                        break;
                    case (int)MeshType.SpikerMesh:
                        selmesh = CreateSpikerMesh(selposition, new Quaternion(selrotation * MathF.PI / 180.0f, 1.0f), spikerSphereSize, icoSphereSub);
                        vertCount += (uint)selmesh.vertexes.Length;
                        indCount += (uint)selmesh.indices.Length;
                        if (staticObject)
                            scene.AddSpatialObject(selmesh, MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
                        else
                            scene.AddSpatialObject(selmesh, MotionType.Dynamic, Layers.MOVING, Activation.Activate);
                        break;
                    case (int)MeshType.TriangleMesh:
                        selmesh = Create2DTriangle(selposition, new Quaternion(selrotation * MathF.PI / 180.0f, 1.0f));
                        vertCount += (uint)selmesh.vertexes.Length;
                        indCount += (uint)selmesh.indices.Length;
                        if (staticObject)
                            scene.AddSpatialObject(selmesh, MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
                        else
                            scene.AddSpatialObject(selmesh, MotionType.Dynamic, Layers.MOVING, Activation.Activate);
                        break;
                    case (int)MeshType.FileMesh:
                        if (!File.Exists(ModelPath + textInput))
                        {
                            ImGui.OpenPopup("Error");
                        }
                        else
                        {
                            scene.AddSpatialObject(LoadModel(selposition, new Quaternion(selrotation * MathF.PI / 180.0f, 1.0f), textInput));
                            vertCount += (uint)scene.SpatialObjects[id].SO_mesh.vertexes.Length;
                            indCount += (uint)scene.SpatialObjects[id].SO_mesh.indices.Length;
                            scene.SpatialObjects[id].SO_rigidbody.SetVelocity(selvelocity);
                            scene.SpatialObjects[id].SO_rigidbody.SetAngularVelocity(selvelocityrot);
                        }
                        break;
                }
            }
            if (ImGui.BeginPopup("Error"))
            {
                string text = "Model Not Found " + textInput;
                ImGui.Text(text);
                ImGui.EndPopup();
            }

            ImGui.End();
        }
    }
}