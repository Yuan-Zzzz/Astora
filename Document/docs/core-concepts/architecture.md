# Architecture

!!! warning
    This document is a work in progress and may be updated frequently.
## Overview
Astora Engine is node-based, meaning that everything in the engine is represented as a node in a tree structure. This allows for a highly modular and flexible design, where different components can be easily added, removed, or modified without affecting the overall system. This is inspired by Godot Engine's architecture.

## Engine
The Engine class is the main entry point of Astora Engine. It acts as a static facade: after `Engine.Initialize(Content, GDM, nodeFactory?)`, all getters and methods delegate to an internal `IEngineContext` (default implementation: `EngineContext`). This allows tests or multi-viewport setups to inject a custom context without changing call sites.

Features of the Engine class include:

- Managing the main game loop, including updating and rendering.
- Providing a centralized input handling system.
- Design resolution and display settings (scale matrix, viewport).

### Extension points
- **Resources**: Register custom asset types with `ResourceLoader.RegisterImporter<T>(IResourceImporter)`.
- **Nodes**: Implement `INodeFactory` (e.g. use `NodeTypeRegistry`) and pass it to `Engine.Initialize` so scene load/save and the editor use the same node types.
- **Rendering**: Nodes draw via `IRenderBatcher`; the pipeline can be extended with custom `IRenderPass` implementations.

## Node System
Node System is the core of Astora Engine. It provides a way to create and manage nodes, which can represent various elements such as scenes, objects, scripts, and more. Nodes can have parent-child relationships, allowing for hierarchical organization and inheritance of properties and behaviors. In Astora Engine, everything is a node, including scenes, objects, scripts, and even the engine itself. This allows for a consistent and unified approach to managing different components of the engine.