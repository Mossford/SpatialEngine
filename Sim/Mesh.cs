using System;
using Silk.NET.OpenGL;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Collections.Generic;

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
        TriangleMesh,
        FileMesh,
        First = CubeMesh,
        Last = FileMesh
    };

    public class Mesh : IDisposable
    {
        public Vertex[] vertexes;
        public uint[] indices;
        GL gl;
        public uint vao;
        public uint vbo;
        public uint ebo;
        public string modelLocation;

        public Vector3 position = Vector3.Zero; 
        public float scale = 1f;
        public Vector3 rotation = Vector3.Zero;
        public Matrix4x4 modelMat;

        public Mesh(GL gl, Vertex[] vertexes, uint[] indices, Vector3 position, Vector3 rotation, float scale)
        {
            this.gl = gl;
            this.vertexes = vertexes;
            this.indices = indices;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public Mesh(GL gl, string modelLocation, Vertex[] vertexes, uint[] indices, Vector3 position, Vector3 rotation, float scale)
        {
            this.gl = gl;
            this.modelLocation = modelLocation;
            this.vertexes = vertexes;
            this.indices = indices;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        ~Mesh()
        {
            gl.DeleteVertexArray(vao);
            gl.DeleteBuffer(vbo);
            gl.DeleteBuffer(ebo);
        }

        public void SetModelMatrix()
        {
            modelMat = Matrix4x4.Identity;
            modelMat = Matrix4x4.CreateTranslation(position) * (Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, rotation.X) * Matrix4x4.CreateFromAxisAngle(Vector3.UnitY, rotation.Y) * Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ, rotation.Z)) * Matrix4x4.CreateScale(scale);
        }

        public unsafe void BufferGens()
        {
            vao = gl.GenVertexArray();
            gl.BindVertexArray(vao);
            vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            ebo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

            fixed (Vertex* buf = vertexes)
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (vertexes.Length * sizeof(Vertex)), buf, BufferUsageARB.StaticDraw);
            fixed (uint* buf = indices)
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*) 0);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*) (3 * sizeof(float)));
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*) (6 * sizeof(float)));
            gl.BindVertexArray(0);
        }

        public unsafe void ReGenBuffer()
        {
            Dispose();
            vao = gl.GenVertexArray();
            gl.BindVertexArray(vao);
            vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            ebo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

            fixed (Vertex* buf = vertexes)
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (vertexes.Length * sizeof(Vertex)), buf, BufferUsageARB.StaticDraw);
            fixed (uint* buf = indices)
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*) 0);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*) (3 * sizeof(float)));
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*) (6 * sizeof(float)));
            gl.BindVertexArray(0);
        }

        public unsafe void DrawMesh()
        {
            gl.BindVertexArray(vao);
            gl.DrawElements(GLEnum.Triangles, (uint)indices.Length, GLEnum.UnsignedInt, (void*) 0);
        }

        public void Dispose()
        {
            gl.DeleteVertexArray(vao);
            gl.DeleteBuffer(vbo);
            gl.DeleteBuffer(ebo);
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
        public static Mesh Create2DTriangle(GL gl, Vector3 position, Vector3 rotation)
        {
            Vertex[] vertxes = new Vertex[3];
            vertxes[0] = new Vertex(new Vector3(-1.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(0.0f, 0.0f));
            vertxes[1] = new Vertex(new Vector3(1.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(0.0f, 0.0f));
            vertxes[2] = new Vertex(new Vector3(-1.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(0.0f, 0.0f));
            
            uint[] indices =
            {
                0, 1, 2
            };
            return new Mesh(gl, ((int)MeshType.TriangleMesh).ToString(), vertxes, indices, position, rotation, 1);
        }

        public static Mesh CreateCubeMesh(GL gl, Vector3 position, Vector3 rotation)
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
            return new Mesh(gl, ((int)MeshType.CubeMesh).ToString(), vertexes, indices, position, rotation, 1.0f);;
        }

        public static Mesh CreateSphereMesh(GL gl, Vector3 position, Vector3 rotation, uint subdivideNum)
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
            return new Mesh(gl, ((int)MeshType.IcoSphereMesh).ToString(), vertexes, indices, position, rotation, 1.0f);
        }

        public static Mesh CreateSpikerMesh(GL gl, Vector3 position, Vector3 rotation, float size)
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

            for (int i = 0; i < 2; i++)
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
            return new Mesh(gl, ((int)MeshType.IcoSphereMesh).ToString(), vertexes, indices, position, rotation, 1.0f);
        }
    }
}