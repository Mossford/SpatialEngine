## Rendering

### The rendering section consists of rendering specific things like the Main Drawer, Mesh handling, and other rendering specific items
#### [The Renderer File](Renderer.cs)
> The renderer operates on one specific structure. A **RenderSet**. The Renderer uses a list of these Rendersets to draw sets of SpatialObjects. In normal terms this is called a Batch Renderer or a Batching System.
> <br>
>
> An abstract way to represent this renderer is that it takes in all the meshes in the scene. Splits them up into sections by a set value. Then combines all these meshes vertexes into one mesh. Send that to the gpu and render that one mesh using a offset so that it can be multiple draw calls but only using one mesh.
>
> The **RenderSet** contains the needed things for fully rendering a set of objects. Opengl uses a **Vao** *(Vertex array object)* This holds a index to where the vertexes of a mesh are stored. Opengl also uses a **Vbo** *(Vertex buffer object)* and a **Ebo** *(Element buffer object)*, in which the Ebo being the more important one holding all the indices of the mesh.
> <br>
> The render set also stores a list of a object called a **MeshOffset** which will be used for a method of drawing this renderer was built for. 
> <br>
>
> The main functions of the Renderset are the **UpdateDrawSet()** and the **DrawSet()** functions. These are the most important to the operation of one.
> <br>
> The Renderset starts with the function to Create a *DrawSet*. A DrawSet is a group of meshes selected to be combined into one *object*. This function takes in *CountBE* and a *CountTO*, these both point the function where to start taking in meshes to combine and where to end taking in meshes.
> From here is simple the function will then get every mesh from that starting index of CountBE to CountTO and put all their vertexes and indices into 2 arrays respectively.
> <br>
>
> This part of the code is shown as
```c#
int vertexSize = 0;
int indiceSize = 0;
for (int i = countBE; i < countTO; i++)
{
    vertexSize += objs[i].SO_mesh.vertexes.Length;
    indiceSize += objs[i].SO_mesh.indices.Length;
}

Vertex[] verts = new Vertex[vertexSize];
uint[] inds = new uint[indiceSize];
int countV = 0;
int countI = 0;
for (int i = countBE; i < countTO; i++)
{
    for (int j = 0; j < objs[i].SO_mesh.vertexes.Length; j++)
    {
        verts[countV] = objs[i].SO_mesh.vertexes[j];
        countV++;
    }
    for (int j = 0; j < objs[i].SO_mesh.indices.Length; j++)
    {
        inds[countI] = objs[i].SO_mesh.indices[j];
        countI++;
    }
}
```
> From here it will construct the needed Opengl buffers and tell Opengl how the vertex is stored.
>
> This in code is
```c#
vao = gl.GenVertexArray();
gl.BindVertexArray(vao);
vbo = gl.GenBuffer();
gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
ebo = gl.GenBuffer();
gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

fixed (Vertex* buf = verts)
    gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertexSize * sizeof(Vertex)), buf, BufferUsageARB.StreamDraw);
fixed (uint* buf = inds)
    gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indiceSize * sizeof(uint)), buf, BufferUsageARB.StreamDraw);

gl.EnableVertexAttribArray(0);
gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)0);
gl.EnableVertexAttribArray(1);
gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)(3 * sizeof(float)));
gl.EnableVertexAttribArray(2);
gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)(6 * sizeof(float)));
gl.BindVertexArray(0);
```
> <br>
>
> The UpdateDrawSet function operates the same as this but only gets run when an update to that drawset is needed and will add or remove a mesh when needed.
> <br>
>
> The difference in opengl is shown here
```c#
gl.BindVertexArray(vao);
gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

fixed (Vertex* buf = verts)
    gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(verts.Length * sizeof(Vertex)), buf, BufferUsageARB.StreamDraw);
fixed (uint* buf = inds)
    gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(inds.Length * sizeof(uint)), buf, BufferUsageARB.StreamDraw);

gl.BindVertexArray(0);
```
> <br>
>
> Now we get to the most important part of the Renderset. The DrawSet function.
> <br>
> This function takes in a CountBE and CountTO just like the functions before but uses these to draw the meshes in between those two indexes. It also takes in other paramaters required for rendering like the view and projection matrix.
> <br>
> It starts wtith required Opengl functions of binding the vertex array and setting the matrices into the shader. It then leads to the loop which will go over every object from the CountBE to the CountTO. It will then check if a *MeshOffset* has been created for the current object. If it has not it will then run the function *GetOffsetIndex()*. 
> <br>
> This function is very important to the speed of this renderer. This function is required for the use of the Opengl Draw function I use which is *DrawElementsBaseVertex()*. This function needs to take in a index into the buffer in which it will start drawing from that index. This helper function precalculates and stores every SpatialObject mesh offset so that it can draw into that array without needing to start from the 0 index of the vertex buffer.
>
> This function is represented as
```c#
int offset = 0;
int offsetByte = 0;
for (int i = countBE; i < index; i++)
{
    offset += objs[i].SO_mesh.vertexes.Length;
    offsetByte += objs[i].SO_mesh.indices.Length;
}
meshOffsets.Add(new MeshOffset(offset, offsetByte * sizeof(uint)));
return meshOffsets.Count - 1;
```
> <br>
>
> Now Opengls documentation for this function has caused some problems for their naming of paramaters and their uses. As shown here.
* ### This has been swapped out for a multidraw which functions the same but takes in an array of those paramaters so it can run in one api call.
```c#
//Because of opengls stupid documentation this draw call is suppose to take in the offset in indices by bytes then take in the offset in vertices instead of the offset in indices
// and its not the indices that are stored it wants the offsets as the indcies are already in a buffer which is what draw elements is using
/*
    indices
        Specifies a pointer to the location where the indices are stored.
    basevertex
        Specifies a constant that should be added to each element of indices when chosing elements from the enabled vertex arrays. 
*/
//This naming is so fucking bad and has caused me multiple hours in trying to find what the hell the problem is
gl.DrawElementsBaseVertex(GLEnum.Triangles, (uint)objs[i].SO_mesh.indices.Length, GLEnum.UnsignedInt, (void*)meshOffsets[index].offsetByte, meshOffsets[index].offset);
```
> Now the main Renderer all opertes in the **Draw()** function.
> This function starts with getting the current amount of SpatialObjects in the scene. It then checks if the amount of objects multiplied with the maximum a renderset can render multiplied by the amount of current rendersets is less than the amount of objects. Yeilding the expression of *(ObjectAmount > MaximumRenderAmount * RendersetCount)*. This will check if we have more objects than the amount all the rendersets can hold.
> <br> 
> If this condition turns true we will then add a new renderset to the list of rendersets and create a draw set using a CountBE and CountTO. This is calculated through the loop. This code appears as
```c#
int countADD = scene.SpatialObjects.Count;
int beCountADD = 0;
int objCountADD = 0;
for (int i = 0; i < renderSets.Count; i++)
{
    beCountADD = objCountADD;
    objCountADD = (int)MathF.Min(MaxRenders, countADD) + (i * MaxRenders);
    countADD -= MaxRenders;
}
```
> This will calculate the index for the CountBE being *beCountADD* and the CountTO being *objCountADD*. It will then use this to fully run the CreateDrawSet() function.
> <br>
>
> It will then lead to checking if we need to update the current rendersets based on that if we have changed in the amount of Spatialobjects since the last time we ran the renderer. This part is fundemtently the same as creating the drawset in where we still calculate the CountBE and CountTO but we reupload with the new objects to the renderset. We could use the CreateDrawSet() function for this purpose but a speical one is needed as that contains opengl code that would slow down the renderer.
> <br>
>
> This leads to the loop to go over all the rendersets and call their draw function with calculating the CountBE and CountTO.
>
>This is shown in a simple way of
```c#
count = objTotalCount;
beCount = 0;
for (int i = 0; i < renderSets.Count; i++)
{
    int objCount = (int)MathF.Min(MaxRenders, count) + (i * MaxRenders);
    renderSets[i].DrawSet(scene.SpatialObjects, beCount, objCount, ref shader, view, proj, camPos);
    count -= MaxRenders;
    beCount = objCount;
}
```

#### [The Mesh File](Mesh.cs)
>The Mesh file contains all the important functions and data to represent a mesh that Opengl can take.
> 
> It contains the Vertex struct which holds the data required for the mesh. Currently it contains the Position of the vertex, Normal and UV coordinate of it as well. 
>
> It has im pretty sure the most refrences in the engine at 150 at the time of writing. It is represented in code as 

```c#
public struct Vertex
{
    public Vector3 position;
    public Vector3 normal;
    public Vector2 uv;
}
```

> This leads to the Mesh class which holds an array of these vertexes, and an array of the indexes to each of those vertexes.
> It also has the model location if the mesh was happened to be loaded from a file. It contains the position, rotation, and scale of that mesh.

```c#
public class Mesh : IDisposable
{
    public Vertex[] vertexes;
    public uint[] indices;
    public string modelLocation;
    public Vector3 position = Vector3.Zero; 
    public float scale = 1f;
    public Quaternion rotation = Quaternion.Identity;
    public Matrix4x4 modelMat;

    ...
}

```

> 
> There is a DrawMesh function which is intended to draw the mesh directly without going through the renderer. So if we create the mesh class without a SpatialObjct associated with it we can directly draw it if needed. It is noted that this is a slow way to draw the mesh and should not be done in big volumes.
>
> There are several Helper and "Helper" functions associated with the mesh class. These include the *SubdivideTriangle()*, *CalculateNormalsSmooth()*, and *Balloon()*.
>
> The purpose of Subdivide triangle is to subdivide a mesh and was built of as an extension of having to create a sphere in code.
>
> The purpose of the Calculate Normals Smooth is in its name. It is to calculate the normals of a mesh so that the mesh is not flat shaded.
>
> The last function is a joke one where it is built off the main way to create a sphere and will take any mesh and turn it into a sphere if possible.
>
> The next functions are in testing right now and are an internal way to create a Mesh, with the intended goal to reduce copying data that the GC has to clean up.

> The other things that are left in this file are an enum to hold the different types of meshes in the engine, Such as a sphere, cube, triangle, and a "Spiker".

```c#
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
```

>
> There is also the functions mentioned before for creaating Meshes but they are not internal and return a mesh to be copied. There is also the important function to load a mesh from file, which only supports obj files right now, as they are simple to do.
