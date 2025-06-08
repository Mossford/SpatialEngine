using ImGuiNET;
using System.Linq;
using System.Numerics;

//engine stuff
using static SpatialEngine.Globals;

namespace SpatialEngine.Rendering.ImGUI
{
    public static class SceneViewer
    {
        public static void Draw()
        {
            ImGui.SetNextWindowSize(new Vector2(600, 420), ImGuiCond.FirstUseEver);
            ImGui.Begin("Scene");

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
    }
}