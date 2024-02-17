# **The Spatial Engine** 
## (A c# version of the c++ Spatial Engine by me)

## Current Features
* #### Runs on SilkNet using a custom built renderer on Opengl that uses batching of meshes for fast rendering of huge amounts of meshes. Uses custom loading of obj meshes and support for textures and soon a 2d custom renderer for ui.
* #### Uses Jolt physics for its physics and a possbility to make my own physics engine in the future once I get the physics on the c++ version of this engine working
* #### Currently uses Riptide Networking for connecting clients to servers and sending packets. Will be replaced by Valves networking solution. Builds on top of this with a custom packet system for the server and client.
* #### Custom way of representing objects in the game with a *SpatialObject* and scene loading *(soon)* and saving.





# How it works

## Core

### The core consists of the entry point, resource handling, Scene Management, Math, Debugging, and Controls. 
#### [The Main File](Src/Core/Program.cs)
> The entry point starts with initilizing resources then checks if the engine has been started in server mode. If it has not it will continue on.
> If it continues it will initalize OpenGl and start the core functions of the engine, such as the main loop and the render loop.
> <br>
>
> The **Load function** will start Initalizing the rest of core Opengl parts such as depth testing. It will then move onto initalizing the scene and physics and load the main testing scene. This will also start the main Renderer, Networking Manager, and Game Manager
> Input functions will also be handled and start including if a key was pressed once and where the mouse currently is.
> <br>
>
> The **Update function** handles key presses that are held for longer and calculating the Fixed Update function and running that. The important part of this function is that it loops through all the objects in the scene and creates the Model Matrix for it
> The **most important** function in the engine, the **Fixed Update** loop handles sending packets to the Server, Player Movement, Use to handle Physics Updates, and handles Client Updates. This function will run at a fixed rate of 60 times per second for 16.6 ms.
> <br>
>
> The last function will be the **Render loop** which handles running the Renderer and setting up opengl settings *(These settings should be moved into the Renderer)*. Debug rendering will also run in this function.
> <br>
>
> There is also Global variables stored in here so that any file can have access to main parts of the engine such as the Main Scene, Renderer, and Physics.

#### [The Scene File](Src/Core/Scene.cs)
> This file contains the most important part of the engine. It holds the structure for the main object, a *SpatialObject*. This container holds important things like the Mesh for that object and the RigidBody for that object.
> <br>
>
> The main Scene class is also stored in here which holds a list of all the SpatialObjects loaded in the scene. It provides functions for adding SpatialObjects with different properties and to save and soon load a scene.

#### The rest of the files
> The rest are for functions to help with the engine such as math functions for quaterion to euler angles. The ResUtil file holds things like paths for the engine to use like a Model path for meshes and a Image path for textures.

## Rendering

### The rendering section consists of rendering specific things like the Main Drawer, Mesh handling, and other rendering specific items
#### [The Renderer File](Src/Rendering/Renderer.cs)
> The renderer operates on one specific structure. A **RenderSet**. The Renderer uses a list of these Rendersets to draw sets of SpatialObjects. In normal terms this is called a Batch Renderer or a Batching System.
> <br>
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
> From here it will construct the needed Opengl buffers and tell Opengl how the vertex is stored.
>
> This in code is
```c#
public unsafe void CreateDrawSet(in List<SpatialObject> objs, int countBE, int countTO)
    {
        int vertexSize = 0;
        int indiceSize = 0;
        for (int i = countBE; i < countTO; i++)
        {
            vertexSize += objs[i].SO_mesh.vertexes.Length;
            indiceSize += objs[i].SO_mesh.indices.Length;
        }
    
        Vertex[] verts = new Vertex[vertexSize];
        uint[] inds = new uint[indiceSize];
        //models = stackalloc Matrix4x4[countTO - countBE];
        int countV = 0;
        int countI = 0;
        //int count = 0;
        for (int i = countBE; i < countTO; i++)
        {
            //models[count] = objs[i].SO_mesh.modelMat;
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
            //count++;
        }
    
        //modelMatrixes = new BufferObject<Matrix4x4>(models, 3, BufferTargetARB.ShaderStorageBuffer, BufferUsageARB.StreamDraw);
    
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
    }
```
> <br>
>
> The UpdateDrawSet function operates the same as this but only gets run when an update to that drawset is needed and will add or remove a mesh when needed.
> <br>
>
> Now we get to the most important part of the Renderset. The DrawSet function.
> <br>
> This function takes in a CountBE and CountTO just like the functions before but uses these to draw the meshes in between those two indexes. It also takes in other paramaters required for rendering like the view and projection matrix.
> <br>
> It starts wtith required Opengl functions of binding the vertex array and setting the matrices into the shader. It then leads to the loop which will go over every object from the CountBE to the CountTO. It will then check if a *MeshOffset* has been created for the current object. If it has not it will then run the function *GetOffsetIndex()*. 
> <br>
> This function is very important to the speed of this renderer. This function is required for the use of the Opengl Draw function I use which is *DrawElementsBaseVertex()*. This function needs to take in a index into the buffer in which it will start drawing from that index. This helper function precalculates and stores every SpatialObject mesh offset so that it can draw into that array without needing to start from the 0 index of the vertex buffer.
> <br>
>
> Now Opengls documentation for this function has caused some problems for their naming of paramaters and their uses. As shown here.
```c#
//Because of opengls stupid documentation this draw call is suppose to take in the offset in indices by bytes then take in the offset in vertices instead of the offset in indices
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
    //Console.WriteLine(beCount + " to " + objCount + " " + i);
    renderSets[i].DrawSet(scene.SpatialObjects, beCount, objCount, ref shader, view, proj, camPos);
    count -= MaxRenders;
    beCount = objCount;
}
```


> An abstract way to represent this renderer is that it takes in all the meshes in the scene. Splits them up into sections by a set value. Then combines all these meshes vertexes into one mesh. Send that to the gpu and render that one mesh using a offset so that it can be multiple meshes but only using one mesh.

#### [The Mesh File](Src/Rendering/Mesh.cs)
>The Mesh file contains all the important functions and data to represent a mesh that Opengl can take.


## Networking

### The networking section contains important things like packet handling, connections between the server and clients and how to run this all in a correct fashion.







## TODO

* ### Server handles creating a object for each player to represent a model but only send it to other clients except the one its rendering?
* ### Documentation
* ### ui rendering
* ### Cascading Shadows
* ### refactoring of systems?