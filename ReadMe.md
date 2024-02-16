# **The Spatial Engine**

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
> This function starts with getting the current amount of SpatialObjects in the scene. It then checks if the amount of objects multiplied with the maximum a renderset can render multiplied by the amount of current rendersets is less than the amount of objects. Yeilding the expression of *(ObjectAmount > MaximumRenderAmount * RendersetCount)*.
