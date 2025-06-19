using System;
using Silk.NET.OpenGL;
using System.Numerics;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text.Unicode;

//engine stuff
using static SpatialEngine.Globals;
using Silk.NET.Maths;
using Silk.NET.Input;
using System.Net.Security;
using Silk.NET.Vulkan;
using SpatialEngine.SpatialMath;
using System.Diagnostics.CodeAnalysis;

namespace SpatialEngine.Rendering
{
    public struct Vertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;

        public Vertex(Vector3 pos, Vector3 nor, Vector2 tex)
        {
            position = pos;
            normal = nor;
            uv = tex;
        }
    }

    public enum MeshType
    {
        CubeMesh,
        IcoSphereMesh,
        SpikerMesh,
        TriangleMesh,
        FileMesh,
        First = CubeMesh,
        Last = FileMesh
    };

    public class Mesh : IDisposable
    {
        public Vertex[] vertexes;
        public uint[] indices;
        /// <summary>
        /// only holds the file name of the mesh but not the mesh path
        /// </summary>
        public string modelLocation;
        public Vector3 position = Vector3.Zero; 
        public float scale = 1f;
        public Quaternion rotation = Quaternion.Identity;
        public Matrix4x4 modelMat;

        public Mesh(Vertex[] vertexes, uint[] indices, Vector3 position, Quaternion rotation, float scale = 1.0f)
        {
            this.vertexes = vertexes;
            this.indices = indices;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public Mesh(string modelLocation, Vertex[] vertexes, uint[] indices, Vector3 position, Quaternion rotation, float scale = 1.0f)
        {
            this.modelLocation = modelLocation;
            this.vertexes = vertexes;
            this.indices = indices;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public Mesh(string modelLocation, Vector3 position, Quaternion rotation, float scale = 1.0f)
        {
            this.modelLocation = modelLocation;
            LoadModel(position, rotation, modelLocation);
            this.scale = scale;
        }

        /// <summary>
        /// TESTING may cause issues
        /// </summary>
        public Mesh ()
        {
            vertexes = new Vertex[0];
            indices = new uint[0];
        }

        public void SetModelMatrix()
        {
            modelMat = Matrix4x4.Identity * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateScale(scale) * Matrix4x4.CreateTranslation(position);
        }

        /// <summary>
        /// Slow way of drawing meant for use outside of the scene system and of low volume of meshes
        /// </summary>
        public unsafe void DrawMesh(ref Shader shader, in Matrix4x4 view, in Matrix4x4 proj, in Vector3 camPos)
        {
            uint vao;
            uint vbo;
            uint ebo;

            vao = gl.GenVertexArray();
            gl.BindVertexArray(vao);
            vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            ebo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

            fixed (Vertex* buf = vertexes)
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertexes.Length * sizeof(Vertex)), buf, BufferUsageARB.StreamDraw);
            fixed (uint* buf = indices)
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StreamDraw);

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)0);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)(3 * sizeof(float)));
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)(6 * sizeof(float)));
            
            gl.UseProgram(shader.shader);
            gl.BindVertexArray(vao);
            shader.setMat4("view", view);
            shader.setMat4("projection", proj);
            shader.setVec3("viewPos", camPos);
            shader.setMat4("model", modelMat);
            shader.setBool("meshDraw", true);
            gl.DrawElements(GLEnum.Triangles, (uint)indices.Length, GLEnum.UnsignedInt, (void*)0);
            gl.BindVertexArray(0);

            gl.DeleteBuffer(vbo);
            gl.DeleteBuffer(ebo);
            gl.DeleteVertexArray(vao);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void SubdivideTriangle()
        {
            List<uint> newIndices = new List<uint>();
            List<Vertex> newVerts = new List<Vertex>();
            for (int g = 0; g < indices.Length; g += 3)
            {
                //Get the required vertexes
                uint ia = indices[g]; 
                uint ib = indices[g + 1];
                uint ic = indices[g + 2]; 
                Vertex aTri = vertexes[ia];
                Vertex bTri = vertexes[ib];
                Vertex cTri = vertexes[ic];

                //Create New Points
                Vector3 ab = (aTri.position + bTri.position) * 0.5f;
                Vector3 bc = (bTri.position + cTri.position) * 0.5f;
                Vector3 ca = (cTri.position + aTri.position) * 0.5f;

                //Create Normals
                Vector3 u = bc - ab;
                Vector3 v = ca - ab;
                Vector3 normal = Vector3.Normalize(Vector3.Cross(u,v));

                //Add the new vertexes
                ia = (uint)newVerts.Count;
                newVerts.Add(aTri);
                ib = (uint)newVerts.Count;
                newVerts.Add(bTri);
                ic = (uint)newVerts.Count;
                newVerts.Add(cTri);
                uint iab = (uint)newVerts.Count;
                newVerts.Add(new Vertex(ab, normal, Vector2.Zero));
                uint ibc = (uint)newVerts.Count; 
                newVerts.Add(new Vertex(bc, normal, Vector2.Zero));
                uint ica = (uint)newVerts.Count; 
                newVerts.Add(new Vertex(ca, normal, Vector2.Zero));
                newIndices.Add(ia); newIndices.Add(iab); newIndices.Add(ica);
                newIndices.Add(ib); newIndices.Add(ibc); newIndices.Add(iab);
                newIndices.Add(ic); newIndices.Add(ica); newIndices.Add(ibc);
                newIndices.Add(iab); newIndices.Add(ibc); newIndices.Add(ica);
            }
            indices = newIndices.ToArray();
            vertexes = newVerts.ToArray();
        }

        /// <summary>
        /// creates smooth normals
        /// </summary>
        public void CalculateNormalsSmooth()
        {
            for (int g = 0; g < vertexes.Length; g++)
            {
                Vector3 normal = Vector3.One;
                for (int i = 0; i < indices.Length; i += 3)
                {
                    uint a, b, c;
                    a = indices[i];
                    b = indices[i + 1];
                    c = indices[i + 2];
                    if (vertexes[g].position == vertexes[a].position)
                    {
                        Vector3 u = vertexes[b].position - vertexes[a].position;
                        Vector3 v = vertexes[c].position - vertexes[a].position;
                        Vector3 tmpnormal = Vector3.Normalize(Vector3.Cross(u, v));
                        float aA = MathS.Vector3Angle(vertexes[b].position - vertexes[a].position, vertexes[c].position - vertexes[a].position);
                        normal += tmpnormal * aA;
                    }
                    if (vertexes[g].position == vertexes[b].position)
                    {
                        Vector3 u = vertexes[c].position - vertexes[b].position;
                        Vector3 v = vertexes[a].position - vertexes[b].position;
                        Vector3 tmpnormal = Vector3.Normalize(Vector3.Cross(u, v));
                        float aA = MathS.Vector3Angle(vertexes[c].position - vertexes[b].position, vertexes[a].position - vertexes[b].position);
                        normal += tmpnormal * aA;
                    }
                    if (vertexes[g].position == vertexes[c].position)
                    {
                        Vector3 u = vertexes[a].position - vertexes[c].position;
                        Vector3 v = vertexes[b].position - vertexes[c].position;
                        Vector3 tmpnormal = Vector3.Normalize(Vector3.Cross(u, v));
                        float aA = MathS.Vector3Angle(vertexes[a].position - vertexes[c].position, vertexes[b].position - vertexes[c].position);
                        normal += tmpnormal * aA;
                    }
                }
                vertexes[g].normal = Vector3.Normalize(normal);
            }
        }

        public void Balloon(float delta = 0.0f, float speed = 0.0f, float percentage = 0.0f)
        {
            for (int i = 0; i < vertexes.Count(); i++)
            {
                Vector3 finPos = Vector3.Normalize(vertexes[i].position);
                if (percentage == 0.0f)
                {
                    finPos *= GetTime() * delta * speed;
                    vertexes[i].position = (vertexes[i].position * (1.0f - GetTime() * delta * speed)) + finPos;
                }
                else
                    vertexes[i].position = vertexes[i].position * (1.0f - percentage) + finPos * percentage;
            }
        }

        /// <summary>
        /// Internal Mesh gen
        /// </summary>
        public void Create2DTriangle(Vector3 position, Quaternion rotation)
        {
            vertexes = new Vertex[3];
            vertexes[0] = new Vertex(new Vector3(-1.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(0, 0));
            vertexes[1] = new Vertex(new Vector3(1.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(1, 0));
            vertexes[2] = new Vertex(new Vector3(-1.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(0, 1));

            indices = new uint[3];
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;

            modelLocation = ((int)MeshType.TriangleMesh).ToString();
            this.position = position;
            this.rotation = rotation;
            scale = 1;
        }

        /// <summary>
        /// Internal Mesh gen
        /// </summary>
        public void CreateCubeMesh(Vector3 position, Quaternion rotation)
        {
            vertexes =
            [
                new Vertex(new Vector3(-1.0f, -1.0f, 1.0f),new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(-1.0f, 1.0f, 1.0f),new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(-1.0f, -1.0f, -1.0f),new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(-1.0f, 1.0f, -1.0f),new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3( 1.0f,-1.0f, 1.0f),new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(1.0f,1.0f, 1.0f),new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(1.0f,-1.0f, -1.0f),new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(1.0f,1.0f, -1.0f),new Vector3(0), new Vector2(0))
            ];
            indices =
            [
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
            ];

            modelLocation = ((int)MeshType.CubeMesh).ToString();
            this.position = position;
            this.rotation = rotation;
            scale = 1;
        }

        /// <summary>
        /// Internal Mesh gen
        /// </summary>
        public void CreateSphereMesh(Vector3 position, Quaternion rotation, uint subdivideNum)
        {

            float t = 0.52573111f;
            float b = 0.850650808f;

            vertexes =
            [
                new Vertex(new Vector3(-t,  b,  0), new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(t,  b,  0),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(-t, -b,  0), new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(t, -b,  0),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(0, -t,  b),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(0,  t,  b),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(0, -t, -b),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(0,  t, -b),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(b,  0, -t),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(b,  0,  t),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(-b,  0, -t), new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(-b,  0,  t), new Vector3(0), new Vector2(0))
            ];

            indices =
            [
                0, 11, 5,
                0, 5, 1,
                0, 1, 7,
                0, 7, 10,
                0, 10, 11,

                1, 5, 9,
                5, 11, 4,
                11, 10, 2,
                10, 7, 6,
                7, 1, 8,

                3, 9, 4,
                3, 4, 2,
                3, 2, 6,
                3, 6, 8,
                3, 8, 9,

                4, 9, 5,
                2, 4, 11,
                6, 2, 10,
                8, 6, 7,
                9, 8, 1
            ];

            for (int i = 0; i < subdivideNum; i++)
            {
                List<uint> newIndices = new List<uint>();
                List<Vertex> newVerts = new List<Vertex>();
                for (int g = 0; g < indices.Length; g += 3)
                {
                    //Get the required vertexes
                    uint ia = indices[g];
                    uint ib = indices[g + 1];
                    uint ic = indices[g + 2];
                    Vertex aTri = vertexes[ia];
                    Vertex bTri = vertexes[ib];
                    Vertex cTri = vertexes[ic];

                    //Create New Points
                    Vector3 ab = Vector3.Normalize((aTri.position + bTri.position) * 0.5f);
                    Vector3 bc = Vector3.Normalize((bTri.position + cTri.position) * 0.5f);
                    Vector3 ca = Vector3.Normalize((cTri.position + aTri.position) * 0.5f);

                    //Add the new vertexes
                    ia = (uint)newVerts.Count;
                    newVerts.Add(aTri);
                    ib = (uint)newVerts.Count;
                    newVerts.Add(bTri);
                    ic = (uint)newVerts.Count;
                    newVerts.Add(cTri);
                    uint iab = (uint)newVerts.Count;
                    newVerts.Add(new Vertex(ab, Vector3.One, Vector2.Zero));
                    uint ibc = (uint)newVerts.Count;
                    newVerts.Add(new Vertex(bc, Vector3.One, Vector2.Zero));
                    uint ica = (uint)newVerts.Count;
                    newVerts.Add(new Vertex(ca, Vector3.One, Vector2.Zero));
                    newIndices.Add(ia); newIndices.Add(iab); newIndices.Add(ica);
                    newIndices.Add(ib); newIndices.Add(ibc); newIndices.Add(iab);
                    newIndices.Add(ic); newIndices.Add(ica); newIndices.Add(ibc);
                    newIndices.Add(iab); newIndices.Add(ibc); newIndices.Add(ica);
                }
                indices = newIndices.ToArray();
                vertexes = newVerts.ToArray();
            }

            CalculateNormalsSmooth();

            modelLocation = ((int)MeshType.IcoSphereMesh).ToString();
            this.position = position;
            this.rotation = rotation;
            scale = 1;
        }

        /// <summary>
        /// Internal Mesh gen
        /// </summary>
        public void CreateSpikerMesh(Vector3 position, Quaternion rotation, float size, int sphereSubDivide = 2)
        {

            vertexes =
            [
                new Vertex(new Vector3(-1.0f,  1.0f,  0), new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(1.0f,  1.0f,  0),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(-1.0f, -1.0f,  0), new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(1.0f, -1.0f,  0),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(0, -1.0f,  1.0f),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(0,  1.0f,  1.0f),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(0, -1.0f, -1.0f),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(0,  1.0f, -1.0f),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(1.0f,  0, -1.0f),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(1.0f,  0,  1.0f),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(-1.0f,  0, -1.0f), new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(-1.0f,  0,  1.0f), new Vector3(0), new Vector2(0))
            ];

            indices =
            [
                0, 11, 5,
                0, 5, 1,
                0, 1, 7,
                0, 7, 10,
                0, 10, 11,

                1, 5, 9,
                5, 11, 4,
                11, 10, 2,
                10, 7, 6,
                7, 1, 8,

                3, 9, 4,
                3, 4, 2,
                3, 2, 6,
                3, 6, 8,
                3, 8, 9,

                4, 9, 5,
                2, 4, 11,
                6, 2, 10,
                8, 6, 7,
                9, 8, 1
            ];

            for (int i = 0; i < sphereSubDivide; i++)
            {
                List<uint> newIndices = new List<uint>();
                List<Vertex> newVerts = new List<Vertex>();
                for (int g = 0; g < indices.Length; g += 3)
                {
                    //Get the required vertexes
                    uint ia = indices[g];
                    uint ib = indices[g + 1];
                    uint ic = indices[g + 2];
                    Vertex aTri = vertexes[ia];
                    Vertex bTri = vertexes[ib];
                    Vertex cTri = vertexes[ic];

                    //Create New Points
                    Vector3 ab = Vector3.Normalize((aTri.position + bTri.position) * 0.5f) * size;
                    Vector3 bc = Vector3.Normalize((bTri.position + cTri.position) * 0.5f) * size;
                    Vector3 ca = Vector3.Normalize((cTri.position + aTri.position) * 0.5f) * size;

                    //Create Normals
                    Vector3 u = bc - ab;
                    Vector3 v = ca - ab;
                    Vector3 normal = Vector3.Normalize(Vector3.Cross(u, v));

                    //Add the new vertexes
                    ia = (uint)newVerts.Count;
                    newVerts.Add(aTri);
                    ib = (uint)newVerts.Count;
                    newVerts.Add(bTri);
                    ic = (uint)newVerts.Count;
                    newVerts.Add(cTri);
                    uint iab = (uint)newVerts.Count;
                    newVerts.Add(new Vertex(ab, normal, Vector2.Zero));
                    uint ibc = (uint)newVerts.Count;
                    newVerts.Add(new Vertex(bc, normal, Vector2.Zero));
                    uint ica = (uint)newVerts.Count;
                    newVerts.Add(new Vertex(ca, normal, Vector2.Zero));
                    newIndices.Add(ia); newIndices.Add(iab); newIndices.Add(ica);
                    newIndices.Add(ib); newIndices.Add(ibc); newIndices.Add(iab);
                    newIndices.Add(ic); newIndices.Add(ica); newIndices.Add(ibc);
                    newIndices.Add(iab); newIndices.Add(ibc); newIndices.Add(ica);
                }
                indices = newIndices.ToArray();
                vertexes = newVerts.ToArray();
            }

            modelLocation = ((int)MeshType.IcoSphereMesh).ToString();
            this.position = position;
            this.rotation = rotation;
            scale = 1;
        }

        /// <summary>
        /// Internal Mesh gen
        /// </summary>
        public void LoadModel(Vector3 position, Quaternion rotation, string name)
        {
            string loc = Resources.ModelPath + name;
            if (loc == "")
                return;
            if (!File.Exists(loc))
            {
                if (int.TryParse(loc, out int modelType))
                {
                    switch (modelType)
                    {
                        case (int)MeshType.CubeMesh:
                            CreateCubeMesh(position, rotation);
                            break;
                        case (int)MeshType.IcoSphereMesh:
                            CreateSphereMesh(position, rotation, 3);
                            break;
                        case (int)MeshType.TriangleMesh:
                            Create2DTriangle(position, rotation);
                            break;
                        case (int)MeshType.SpikerMesh:
                            CreateSpikerMesh(position, rotation, 0.3f);
                            break;
                    }
                }
                else
                {
                    //could not find the model even though an input file location was provided
                    Console.WriteLine("cannot find model at " + loc);
                    return;
                }
            }

            StreamReader reader = new StreamReader(loc);
            List<Vector2> tmpUV = new List<Vector2>();
            List<Vector3> tmpNormal = new List<Vector3>();
            List<Vector3> tmpVertice = new List<Vector3>();
            List<uint> tmpInd = new List<uint>(), tmpUVInd = new List<uint>(), tmpNormalInd = new List<uint>();
            Vertex vertex = new Vertex(Vector3.Zero, Vector3.Zero, Vector2.Zero);
            string line;
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();

                if (line[0] == 'v' && line[1] != 't' && line[1] != 'n')
                {
                    line = line.Remove(0, 2);
                    string[] values = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (values.Length >= 3 && float.TryParse(values[0], out float x) && float.TryParse(values[1], out float y) && float.TryParse(values[2], out float z))
                    {
                        tmpVertice.Add(new Vector3(x, y, z));
                    }
                }
                else if (line[0] == 'v' && line[1] == 't' && line[1] != 'n')
                {
                    line = line.Remove(0, 3);
                    string[] values = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (values.Length >= 2 && float.TryParse(values[0], out float x) && float.TryParse(values[1], out float y))
                    {
                        tmpUV.Add(new Vector2(x, y));
                    }
                }
                else if (line[0] == 'v' && line[1] != 't' && line[1] == 'n')
                {
                    line = line.Remove(0, 3);
                    string[] values = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (values.Length >= 3 && float.TryParse(values[0], out float x) && float.TryParse(values[1], out float y) && float.TryParse(values[2], out float z))
                    {
                        tmpNormal.Add(new Vector3(x, y, z));
                    }
                }
                else if (line[0] == 'f')
                {
                    line = line.Remove(0, 2);
                    line = line.Replace("/", " ");
                    string[] values = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    if (values.Length >= 9 &&
                        uint.TryParse(values[0], out uint ind1) &&
                        uint.TryParse(values[1], out uint uvind1) &&
                        uint.TryParse(values[2], out uint norind1) &&
                        uint.TryParse(values[3], out uint ind2) &&
                        uint.TryParse(values[4], out uint uvind2) &&
                        uint.TryParse(values[5], out uint norind2) &&
                        uint.TryParse(values[6], out uint ind3) &&
                        uint.TryParse(values[7], out uint uvind3) &&
                        uint.TryParse(values[8], out uint norind3))
                    {
                        tmpInd.Add(ind1); tmpUVInd.Add(uvind1); tmpNormalInd.Add(norind1);
                        tmpInd.Add(ind2); tmpUVInd.Add(uvind2); tmpNormalInd.Add(norind2);
                        tmpInd.Add(ind3); tmpUVInd.Add(uvind3); tmpNormalInd.Add(norind3);
                    }
                }
            }
            this.vertexes = new Vertex[tmpInd.Count];
            this.indices = new uint[tmpInd.Count];
            for (int i = 0; i < tmpInd.Count; i++)
            {
                uint indUv = tmpUVInd[i];
                uint indNor = tmpNormalInd[i];
                uint indVert = tmpInd[i];
                vertex.uv = tmpUV[(int)indUv - 1];
                vertex.normal = tmpNormal[(int)indNor - 1];
                vertex.position = tmpVertice[(int)indVert - 1];
                this.indices[i] = (uint)i;
                this.vertexes[i] = vertex;
                
            }

            modelLocation = name;
            this.position = position;
            this.rotation = rotation;
            scale = 1f;
        }

    }

    public static class MeshUtils
    {
        public static Mesh Create2DTriangle(Vector3 position, Quaternion rotation)
        {
            Vertex[] vertxes = new Vertex[3];
            vertxes[0] = new Vertex(new Vector3(-1.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(0, 0));
            vertxes[1] = new Vertex(new Vector3(1.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(1, 0));
            vertxes[2] = new Vertex(new Vector3(-1.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(0, 1));
            
            uint[] indices =
            {
                0, 1, 2
            };
            return new Mesh(((int)MeshType.TriangleMesh).ToString(), vertxes, indices, position, rotation, 1);
        }

        public static Mesh CreateCubeMesh(Vector3 position, Quaternion rotation)
        {
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
            return new Mesh(((int)MeshType.CubeMesh).ToString(), vertexes, indices, position, rotation, 1.0f);;
        }

        public static Mesh CreateSphereMesh(Vector3 position, Quaternion rotation, uint subdivideNum)
        {

            float t = 0.52573111f;
            float y = 0.850650808f;

            Vertex[] vertexes = 
            {
                new Vertex(new Vector3(-t,  y,  0), new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(t,  y,  0),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(-t, -y,  0), new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(t, -y,  0),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(0, -t,  y),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(0,  t,  y),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(0, -t, -y),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(0,  t, -y),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(y,  0, -t),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(y,  0,  t),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(-y,  0, -t), new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(-y,  0,  t), new Vector3(0), new Vector2(0))
            };

            uint[] indices = 
            {
                0, 11, 5, 
                0, 5, 1,
                0, 1, 7,
                0, 7, 10,
                0, 10, 11,
                
                1, 5, 9,
                5, 11, 4,
                11, 10, 2,
                10, 7, 6,
                7, 1, 8,
                
                3, 9, 4,
                3, 4, 2,
                3, 2, 6,
                3, 6, 8,
                3, 8, 9,
                
                4, 9, 5,
                2, 4, 11,
                6, 2, 10,
                8, 6, 7,
                9, 8, 1
            };

            for (int i = 0; i < subdivideNum; i++)
            {
                List<uint> newIndices = new List<uint>();
                List<Vertex> newVerts = new List<Vertex>();
                for (int g = 0; g < indices.Length; g += 3)
                {
                    //Get the required vertexes
                    uint ia = indices[g]; 
                    uint ib = indices[g + 1];
                    uint ic = indices[g + 2]; 
                    Vertex aTri = vertexes[ia];
                    Vertex bTri = vertexes[ib];
                    Vertex cTri = vertexes[ic];

                    //Create New Points
                    Vector3 ab = Vector3.Normalize((aTri.position + bTri.position) * 0.5f);
                    Vector3 bc = Vector3.Normalize((bTri.position + cTri.position) * 0.5f);
                    Vector3 ca = Vector3.Normalize((cTri.position + aTri.position) * 0.5f);

                    //Create Normals
                    Vector3 u = bc - ab;
                    Vector3 v = ca - ab;
                    Vector3 normal = Vector3.Normalize(Vector3.Cross(u,v));

                    //Add the new vertexes
                    ia = (uint)newVerts.Count;
                    newVerts.Add(aTri);
                    ib = (uint)newVerts.Count;
                    newVerts.Add(bTri);
                    ic = (uint)newVerts.Count;
                    newVerts.Add(cTri);
                    uint iab = (uint)newVerts.Count;
                    newVerts.Add(new Vertex(ab, normal, Vector2.Zero));
                    uint ibc = (uint)newVerts.Count; 
                    newVerts.Add(new Vertex(bc, normal, Vector2.Zero));
                    uint ica = (uint)newVerts.Count; 
                    newVerts.Add(new Vertex(ca, normal, Vector2.Zero));
                    newIndices.Add(ia); newIndices.Add(iab); newIndices.Add(ica);
                    newIndices.Add(ib); newIndices.Add(ibc); newIndices.Add(iab);
                    newIndices.Add(ic); newIndices.Add(ica); newIndices.Add(ibc);
                    newIndices.Add(iab); newIndices.Add(ibc); newIndices.Add(ica);
                }
                indices = newIndices.ToArray();
                vertexes = newVerts.ToArray();
            }

            for (int g = 0; g < vertexes.Length; g++)
            {
                Vector3 normal = Vector3.One;
                for (int i = 0; i < indices.Length; i += 3)
                {
                    uint a, b, c;
                    a = indices[i];
                    b = indices[i + 1];
                    c = indices[i + 2];
                    if (vertexes[g].position == vertexes[a].position)
                    {
                        Vector3 u = vertexes[b].position - vertexes[a].position;
                        Vector3 v = vertexes[c].position - vertexes[a].position;
                        Vector3 tmpnormal = Vector3.Normalize(Vector3.Cross(u, v));
                        normal += tmpnormal;
                    }
                    if (vertexes[g].position == vertexes[b].position)
                    {
                        Vector3 u = vertexes[c].position - vertexes[b].position;
                        Vector3 v = vertexes[a].position - vertexes[b].position;
                        Vector3 tmpnormal = Vector3.Normalize(Vector3.Cross(u, v));
                        normal += tmpnormal;
                    }
                    if (vertexes[g].position == vertexes[c].position)
                    {
                        Vector3 u = vertexes[a].position - vertexes[c].position;
                        Vector3 v = vertexes[b].position - vertexes[c].position;
                        Vector3 tmpnormal = Vector3.Normalize(Vector3.Cross(u, v));
                        normal += tmpnormal;
                    }
                }
                vertexes[g].normal = Vector3.Normalize(normal);
            }

            return new Mesh(((int)MeshType.IcoSphereMesh).ToString(), vertexes, indices, position, rotation, 1.0f);
        }

        public static Mesh CreateSpikerMesh(Vector3 position, Quaternion rotation, float size, int sphereSubDivide = 2)
        {

            Vertex[] vertexes = 
            {
                new Vertex(new Vector3(-1.0f,  1.0f,  0), new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(1.0f,  1.0f,  0),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(-1.0f, -1.0f,  0), new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(1.0f, -1.0f,  0),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(0, -1.0f,  1.0f),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(0,  1.0f,  1.0f),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(0, -1.0f, -1.0f),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(0,  1.0f, -1.0f),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(1.0f,  0, -1.0f),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(1.0f,  0,  1.0f),  new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(-1.0f,  0, -1.0f), new Vector3(0), new Vector2(0)),
                new Vertex(new Vector3(-1.0f,  0,  1.0f), new Vector3(0), new Vector2(0))
            };

            uint[] indices = 
            {
                0, 11, 5, 
                0, 5, 1,
                0, 1, 7,
                0, 7, 10,
                0, 10, 11,
                
                1, 5, 9,
                5, 11, 4,
                11, 10, 2,
                10, 7, 6,
                7, 1, 8,
                
                3, 9, 4,
                3, 4, 2,
                3, 2, 6,
                3, 6, 8,
                3, 8, 9,
                
                4, 9, 5,
                2, 4, 11,
                6, 2, 10,
                8, 6, 7,
                9, 8, 1
            };

            for (int i = 0; i < sphereSubDivide; i++)
            {
                List<uint> newIndices = new List<uint>();
                List<Vertex> newVerts = new List<Vertex>();
                for (int g = 0; g < indices.Length; g += 3)
                {
                    //Get the required vertexes
                    uint ia = indices[g]; 
                    uint ib = indices[g + 1];
                    uint ic = indices[g + 2]; 
                    Vertex aTri = vertexes[ia];
                    Vertex bTri = vertexes[ib];
                    Vertex cTri = vertexes[ic];

                    //Create New Points
                    Vector3 ab = Vector3.Normalize((aTri.position + bTri.position) * 0.5f) * size;
                    Vector3 bc = Vector3.Normalize((bTri.position + cTri.position) * 0.5f) * size;
                    Vector3 ca = Vector3.Normalize((cTri.position + aTri.position) * 0.5f) * size;

                    //Create Normals
                    Vector3 u = bc - ab;
                    Vector3 v = ca - ab;
                    Vector3 normal = Vector3.Normalize(Vector3.Cross(u,v));

                    //Add the new vertexes
                    ia = (uint)newVerts.Count;
                    newVerts.Add(aTri);
                    ib = (uint)newVerts.Count;
                    newVerts.Add(bTri);
                    ic = (uint)newVerts.Count;
                    newVerts.Add(cTri);
                    uint iab = (uint)newVerts.Count;
                    newVerts.Add(new Vertex(ab, normal, Vector2.Zero));
                    uint ibc = (uint)newVerts.Count; 
                    newVerts.Add(new Vertex(bc, normal, Vector2.Zero));
                    uint ica = (uint)newVerts.Count; 
                    newVerts.Add(new Vertex(ca, normal, Vector2.Zero));
                    newIndices.Add(ia); newIndices.Add(iab); newIndices.Add(ica);
                    newIndices.Add(ib); newIndices.Add(ibc); newIndices.Add(iab);
                    newIndices.Add(ic); newIndices.Add(ica); newIndices.Add(ibc);
                    newIndices.Add(iab); newIndices.Add(ibc); newIndices.Add(ica);
                }
                indices = newIndices.ToArray();
                vertexes = newVerts.ToArray();
            }
            return new Mesh(((int)MeshType.IcoSphereMesh).ToString(), vertexes, indices, position, rotation, 1.0f);
        }

        public static Mesh LoadModel(Vector3 position, Quaternion rotation, string name)
        {
            string loc = Resources.ModelPath + name;
            if (loc == "")
                return null;
            if (!File.Exists(loc))
            {
                if(int.TryParse(loc, out int modelType))
                {
                    switch (modelType)
                    {
                        case (int)MeshType.CubeMesh:
                            return CreateCubeMesh(position, rotation);
                        case (int)MeshType.IcoSphereMesh:
                            return CreateSphereMesh(position, rotation, 3);
                        case (int)MeshType.TriangleMesh:
                            return Create2DTriangle(position, rotation);
                        case (int)MeshType.SpikerMesh:
                            return CreateSpikerMesh(position, rotation, 0.3f);
                    }
                }
                else
                {
                    //could not find the model even though an input file location was provided
                    Console.WriteLine("cannot find model at " + loc);
                    return null;
                }
            }

            StreamReader reader = new StreamReader(loc);
            List<Vertex> vertexes = new List<Vertex>();
            List<uint> indices = new List<uint>();
            List<Vector2> tmpUV = new List<Vector2>();
            List<Vector3> tmpNormal = new List<Vector3>();
            List<Vector3> tmpVertice = new List<Vector3>();
            List<uint> tmpInd = new List<uint>(), tmpUVInd = new List<uint>(), tmpNormalInd = new List<uint>();
            Vertex vertex = new Vertex(Vector3.Zero, Vector3.Zero, Vector2.Zero);
            string line;
            while (!reader.EndOfStream)
            {
                 line = reader.ReadLine();

                if (line[0] == 'v' && line[1] != 't' && line[1] != 'n')
                {
                    line = line.Remove(0,2);
                    string[] values = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (values.Length >= 3 && float.TryParse(values[0], out float x) && float.TryParse(values[1], out float y) && float.TryParse(values[2], out float z))
                    {
                        tmpVertice.Add(new Vector3(x, y, z));
                    }
                }
                else if(line[0] == 'v' && line[1] == 't' && line[1] != 'n')
                {
                    line = line.Remove(0, 3);
                    string[] values = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (values.Length >= 2 && float.TryParse(values[0], out float x) && float.TryParse(values[1], out float y))
                    {
                        tmpUV.Add(new Vector2(x, y));
                    }
                }
                else if(line[0] == 'v' && line[1] != 't' && line[1] == 'n')
                {
                    line = line.Remove(0, 3);
                    string[] values = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (values.Length >= 3 && float.TryParse(values[0], out float x) && float.TryParse(values[1], out float y) && float.TryParse(values[2], out float z))
                    {
                        tmpNormal.Add(new Vector3(x, y, z));
                    }
                }
                else if(line[0] == 'f')
                {
                    line = line.Remove(0, 2);
                    line = line.Replace("/", " ");
                    string[] values = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    if (values.Length >= 9 &&
                        uint.TryParse(values[0], out uint ind1) &&
                        uint.TryParse(values[1], out uint uvind1) &&
                        uint.TryParse(values[2], out uint norind1) &&
                        uint.TryParse(values[3], out uint ind2) &&
                        uint.TryParse(values[4], out uint uvind2) &&
                        uint.TryParse(values[5], out uint norind2) &&
                        uint.TryParse(values[6], out uint ind3) &&
                        uint.TryParse(values[7], out uint uvind3) &&
                        uint.TryParse(values[8], out uint norind3))
                    {
                        tmpInd.Add(ind1); tmpUVInd.Add(uvind1); tmpNormalInd.Add(norind1);
                        tmpInd.Add(ind2); tmpUVInd.Add(uvind2); tmpNormalInd.Add(norind2);
                        tmpInd.Add(ind3); tmpUVInd.Add(uvind3); tmpNormalInd.Add(norind3);
                    }
                }
            }
            for (int i = 0; i < tmpInd.Count; i++)
            {
                uint indUv = tmpUVInd[i];
                uint indNor = tmpNormalInd[i];
                uint indVert = tmpInd[i];
                vertex.uv = tmpUV[(int)indUv - 1];
                vertex.normal = tmpNormal[(int)indNor - 1];
                vertex.position = tmpVertice[(int)indVert - 1];
                indices.Add((uint)i);
                vertexes.Add(vertex);
            }
            return new Mesh(name, vertexes.ToArray(), indices.ToArray(), position, rotation, 1.0f);
        }
    }
}