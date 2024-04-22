# **The Spatial Engine** 
## (A framework for building my own projects in c# and expanded off the ideas in [c++ Spatial Engine](https://github.com/Mosseelight/SpaceSimulationTesting) by me)

## Current Features
* #### Runs on SilkNet using a custom built renderer on Opengl that uses batching of meshes for fast rendering of huge amounts of meshes. Uses custom loading of obj meshes and support for textures and soon a 2d custom renderer for ui.
* #### Uses Jolt physics for its physics and a possbility to make my own physics engine in the future once I get the physics on the c++ version of this engine working
* #### Currently uses Riptide Networking for connecting clients to servers and sending packets. Will be replaced by Valves networking solution. Builds on top of this with a custom packet system for the server and client.
* #### Custom way of representing objects in the game with a *SpatialObject* and scene loading *(soon)* and saving.


## TODO

* ### Add to physics
> * Put physics in the Physics namespace
* ### Fix Networking bugs
> * Really werid behavior when disconnecting and reconnecting 
> * Meshes on clients after the first dont get the proper index
* ### Player needs a mesh
* ### Documentation
* ### Refactor shader system
* ### ui rendering
> * Add in AABB checks with possible quadtree for input and buttons
> * Text rendering (How the hell do this this)
* ### Cascading Shadows
* ### refactoring of systems?


# How it works

## [Core](Src/Core/Core.md)
## [Rendering](Src/Rendering/Rendering.md)
## [Networking](Src/Networking/Networking.md)
