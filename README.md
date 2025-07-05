
![logo](logo.png)

> ⚠️ This project is archived and intended for reference only. ⚠️ 

Biophage is my submission to the [Microsoft Dream.Build.Play 2009](https://en.wikipedia.org/wiki/Dream_Build_Play) competition. It is a 3D Real Time Strategy multiplayer game developed for the Xbox 360 console, developed in C# on the [XNA](https://en.wikipedia.org/wiki/Microsoft_XNA) framework.

I had considered porting the game to the open-source successor to the XNA framework, [MonoGame](https://monogame.net), but pivoted to a 2D remake in the [LÖVE](https://love2d.org) framework.

# Dev Highlight

- C#
- .NET Compact
- XNA
- XNA Networking / [Lidgren.Network](https://github.com/lidgren/lidgren-network-gen3)
- Xbox 360 / Windows Games
- [JigLibX](http://jiglibx.wikidot.com) physics engine

# Game Synopsis

[High Concept PDF](high_concept.pdf)

[![DBP 2009 Entry](https://img.youtube.com/vi/AKw0u0r2t50/0.jpg)](https://www.youtube.com/watch?v=AKw0u0r2t50)

# Development

## Tooling

It would be difficult to build the project with the original [XNA](https://en.wikipedia.org/wiki/Microsoft_XNA) and other supporting libraries, as these are no longer available. But it might be possible with some code tweaking to use MonoGame and the [Lidgren.Network](https://github.com/lidgren/lidgren-network-gen3).

The project is split into two distinct parts:

## 1. LNA Game Engine

`Project\ Virus/Trunk/Biophage/LNAGameEngine/`

LNA is a Visual Studio C# library that extends XNA with a Game Scene/Object manger, Resource manager, shaders, UI, Networking manager, and various other common features to help support development of XNA games.

It builds a set of `dll` CLR/IL libraries.

## 2. Project Virus

`Project\ Virus/Trunk/Biophage/Biophage/`

Visual Studio C# executable containing all code, content, and documentation pertaining to the Biophage game itself.
