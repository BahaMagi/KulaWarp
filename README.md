# Kula Warp
Kula Warp is a tribute to [Kula World](https://en.wikipedia.org/wiki/Kula_World) developed by Game Design Sweden AB in 1998. The game is a 3D puzzle platformer that challenges your spatial thinking. 

The goal of the game is to collect keys (will turn into energy as soon as I have the assets for that) to activate the warp to the next level. Additional pickups provide points which at the same time provide lives. The arcade like game punishes you for falling short on time or off the level by inflicting a penalty to your total points based on the points accumulated at the time of death.

In order to get to these keys there are multiple ways to change the direction of gravity to allow you to reach even far away points in the level. The means of movement in the game were slightly modified compared to the original Kula World. Rather than jumping, the player can warp forwards or upwards to teleport the sphere through space. The sphere will manifest at the target location and reattach to any block in the vicinity. This provides an intriguing new way to change gravity and circumvent obstacles. 
## About the Project 
The project was started end of June/beginning of July 2019 to build a portfolio to demonstrate technical skills and familiarity with some of the tools needed to develop games. All code, content and assets were (mostly) developed by one person. A list of resources for some of the images and textures is given below. 

Kula World was chosen as convenient target due to its comparatively small scope. The limited level geometry makes it less reliant on 3D and CG assets than other games and, thus, makes it possible for one person to create a playable demo in a relatively short time. 

The project is ongoing and very far from being finished. However, it demonstrates design concepts and skills used to develop the game thus far. 
## Getting Started
The game was developed with [Unity 2019.1.7f1 Personal](https://unity3d.com/de/get-unity/download/archive). All files necessary to build the game yourself should be included in the repository. In order to display all objects properly, [Blender] (https://www.blender.org/download/) might be required.	

If you would like to test a pre-built version of the game, here are some binaries compiled on August, 14th 2019 from the master branch:
* [Windows](https://www.dropbox.com/s/elskrk1usktkcg8/KulaWarp_WindowsBuild.rar?dl=0)
* [Mac OS ] (https://www.dropbox.com/s/lyerriojb8spn26/KulaWarp_Mac_Build.app.zip?dl=0)
* [Linux] (https://www.dropbox.com/sh/ijsg46bt71zpuaj/AADvI8QkZlIXeijh3UoiOdG2a?dl=0)

If you are interested how the project developed since then, feel free to roam the dev branch. 
## Controls 
The use of an Xbox 360° controller is recommended on Windows. However, due to different key mappings on Mac and Linux players will have to stick to keyboard for now. The controller mappings should be rather intuitive. But as there are no control options that contain the keyboard controls yet, this section explains the basic mappings. 

The game is surprisingly simple. You can do but 3 things: **move**, **look around** and **warp**. However, they can be used in a myriad different ways. The game levels introduce the different concepts one after the other to allow for an intuitive understanding and experimenting with the different mechanics. 
### Move
* *Arrow Keys* / *WASD* on keyboard
* *Joy Stick* / *D-Pad* on controller

Moves you one block forwards or rotates the camera around the player.
### Look Around
* *Q* + *E* on keyboard 
* *LT* + *RT* on controller 

Tilts the camera up or down to allow viewing the level from a different angle.
### Warp
* *Space* on keyboard
* *A* on controller

Warps the player sphere two blocks forwards or upwards. If there is a block directly next to the destination (not diagonally) the direction of gravity will change to allow the sphere to continue on that block. 
## Future Work
 As mentioned above, the game is still under heavy development. The [Issues] () section on this GitHub provides a small overview of known bugs, desired features and dreaded tasks which are yet on the TODO list for this project.
##  Assets
Assets in this project were mostly created by the developer for this project with tools like Photoshop and Blender. In addition to that, inspiration and samples were taken from 
* The Unity [Asset store](https://assetstore.unity.com/), i.e. the skyboxes,
* [Poliigon]( https://www.poliigon.com/), i.e. textures which are in the files but not yet in the game
* [FlatIcon](https://www.flaticon.com/), i.e. images which were either directly used or incorporated into the UI. 
All third party assets are temporary. They will be replaced with self-made assets once more development time can be invested into asset creating or once I stumble upon a friendly graphics designer who is willing to support my cause. 