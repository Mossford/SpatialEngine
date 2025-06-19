## Core

### The core consists of the entry point, resource handling, Scene Management, Math, Debugging, and Controls. 
#### [The Main File](Program.cs)
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

#### [The Scene File](Scene.cs)
> This file contains the most important part of the engine. It holds the structure for the main object, a *SpatialObject*. This container holds important things like the Mesh for that object and the RigidBody for that object.
> <br>
>
> The main Scene class is also stored in here which holds a list of all the SpatialObjects loaded in the scene. It provides functions for adding SpatialObjects with different properties and to save and soon load a scene.

#### The rest of the files
> The rest are for functions to help with the engine such as math functions for quaterion to euler angles. The ResUtil file holds things like paths for the engine to use like a Model path for meshes and a Image path for textures.