# Architecture

!!! warning
    This document is a work in progress and may be updated frequently.
## Overview
Astora Engine is node-based, meaning that everything in the engine is represented as a node in a tree structure. This allows for a highly modular and flexible design, where different components can be easily added, removed, or modified without affecting the overall system. This is inspired by Godot Engine's architecture.

## Engine
The Engine class is the main entry point of Astora Engine. It is responsible for initializing and managing the various subsystems of the engine, such as rendering, input handling, audio, and more. The Engine class also provides a way to access global settings and configurations for the engine.

Features of the Engine class include:

- Managing the main game loop, including updating and rendering.
- Providing a centralized input handling system.
- Designing resolution and display settings.

## Node System
Node System is the core of Astora Engine. It provides a way to create and manage nodes, which can represent various elements such as scenes, objects, scripts, and more. Nodes can have parent-child relationships, allowing for hierarchical organization and inheritance of properties and behaviors. In Astora Engine, everything is a node, including scenes, objects, scripts, and even the engine itself. This allows for a consistent and unified approach to managing different components of the engine.