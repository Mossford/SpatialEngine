using System;
using Silk.NET.OpenGL;
using System.Numerics;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text.Unicode;

namespace SpatialEngine
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
        public uint vao;
        public uint vbo;
        public uint ebo;
        public string modelLocation;
        public Vector3 position = Vector3.Zero; 
        public float scale = 1f;
        public Vector3 rotation = Vector3.Zero;
        public Matrix4x4 modelMat;

        public Mesh(Vertex[] vertexes, uint[] indices, Vector3 position, Vector3 rotation, float scale)
        {
            this.vertexes = vertexes;
            this.indices = indices;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public Mesh(string modelLocation, Vertex[] vertexes, uint[] indices, Vector3 position, Vector3 rotation, float scale)
        {
            this.modelLocation = modelLocation;
            this.vertexes = vertexes;
            this.indices = indices;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        ~Mesh()
        {
            Globals.gl.DeleteVertexArray(vao);
            Globals.gl.DeleteBuffer(vbo);
            Globals.gl.DeleteBuffer(ebo);
        }

        public void SetModelMatrix()
        {
            modelMat = Matrix4x4.Identity;
            modelMat *= Matrix4x4.CreateTranslation(position);
            modelMat *= Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, rotation.X);
            modelMat *= Matrix4x4.CreateFromAxisAngle(Vector3.UnitY, rotation.Y);
            modelMat *= Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ, rotation.Z);
            modelMat *= Matrix4x4.CreateScale(scale);
        }

        public unsafe void BufferGens()
        {
            vao = Globals.gl.GenVertexArray();
            Globals.gl.BindVertexArray(vao);
            vbo = Globals.gl.GenBuffer();
            Globals.gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            ebo = Globals.gl.GenBuffer();
            Globals.gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

            fixed (Vertex* buf = vertexes)
                Globals.gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (vertexes.Length * sizeof(Vertex)), buf, BufferUsageARB.StaticDraw);
            fixed (uint* buf = indices)
                Globals.gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

            Globals.gl.EnableVertexAttribArray(0);
            Globals.gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*) 0);
            Globals.gl.EnableVertexAttribArray(1);
            Globals.gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*) (3 * sizeof(float)));
            Globals.gl.EnableVertexAttribArray(2);
            Globals.gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*) (6 * sizeof(float)));
            Globals.gl.BindVertexArray(0);
        }

        public unsafe void ReGenBuffer()
        {
            Dispose();
            vao = Globals.gl.GenVertexArray();
            Globals.gl.BindVertexArray(vao);
            vbo = Globals.gl.GenBuffer();
            Globals.gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            ebo = Globals.gl.GenBuffer();
            Globals.gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

            fixed (Vertex* buf = vertexes)
                Globals.gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (vertexes.Length * sizeof(Vertex)), buf, BufferUsageARB.StaticDraw);
            fixed (uint* buf = indices)
                Globals.gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

            Globals.gl.EnableVertexAttribArray(0);
            Globals.gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*) 0);
            Globals.gl.EnableVertexAttribArray(1);
            Globals.gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*) (3 * sizeof(float)));
            Globals.gl.EnableVertexAttribArray(2);
            Globals.gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*) (6 * sizeof(float)));
            Globals.gl.BindVertexArray(0);
        }

        public unsafe void DrawMesh()
        {
            Globals.gl.BindVertexArray(vao);
            Globals.gl.DrawElements(GLEnum.Triangles, (uint)indices.Length, GLEnum.UnsignedInt, (void*) 0);
        }

        public void Dispose()
        {
            Globals.gl.DeleteVertexArray(vao);
            Globals.gl.DeleteBuffer(vbo);
            Globals.gl.DeleteBuffer(ebo);
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
    }

    public static class MeshUtils
    {
        public static Mesh Create2DTriangle(Vector3 position, Vector3 rotation)
        {
            Vertex[] vertxes = new Vertex[3];
            vertxes[0] = new Vertex(new Vector3(-1.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(0.0f, 0.0f));
            vertxes[1] = new Vertex(new Vector3(1.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(0.0f, 0.0f));
            vertxes[2] = new Vertex(new Vector3(-1.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(0.0f, 0.0f));
            
            uint[] indices =
            {
                0, 1, 2
            };
            return new Mesh(((int)MeshType.TriangleMesh).ToString(), vertxes, indices, position, rotation, 1);
        }

        public static Mesh CreateCubeMesh(Vector3 position, Vector3 rotation)
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

        public static Mesh CreateSphereMesh(Vector3 position, Vector3 rotation, uint subdivideNum)
        {

            float t = 0.52573111f;
            float b = 0.850650808f;

            Vertex[] vertexes = 
            {
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
            return new Mesh(((int)MeshType.IcoSphereMesh).ToString(), vertexes, indices, position, rotation, 1.0f);
        }

        public static Mesh CreateSpikerMesh(Vector3 position, Vector3 rotation, float size, int sphereSubDivide = 2)
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

        public static Mesh LoadModel(Vector3 position, Vector3 rotation, string modelLocation)
        {
            if(!File.Exists(modelLocation))
            {
                Console.WriteLine("cannot find model loc");
                if(modelLocation == "")
                    return null;
                switch (Int32.Parse(modelLocation))
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


            string[] lines = File.ReadAllLines(modelLocation);
            List<Vertex> vertexes = new List<Vertex>();
            List<uint> indices = new List<uint>();
            List<Vector2> tmpUV = new List<Vector2>();
            List<Vector3> tmpNormal = new List<Vector3>();
            List<Vector3> tmpVertice = new List<Vector3>();
            List<uint> tmpInd = new List<uint>(), tmpUVInd = new List<uint>(), tmpNormalInd = new List<uint>();
            Vertex vertex = new Vertex(Vector3.Zero, Vector3.Zero, Vector2.Zero);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if(line[0] == 'v' && line[1] != 't' && line[1] != 'n')
                {
                    line = line.Remove(0, 2);
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
            return new Mesh(modelLocation, vertexes.ToArray(), indices.ToArray(), position, rotation, 1.0f);
        }
    }
}