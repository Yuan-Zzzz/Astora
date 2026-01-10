# RenderPipeline
The RenderPipeline is the manager of the rendering process. It is designed to solve these problems by organizing the rendering process into a modular, linear workflow.
## Why RenderPipeline?
MonoGame provides a powerful rendering method, We can clear the screen and draw sprites in a absolute positon. However, this approach has some limitations:

- **Lack of Modularity**: The rendering process is often tightly coupled with game logic, making it difficult to manage and extend.
- **Difficult to Customize**: Customizing the rendering process (e.g., adding post-processing effects, handling multiple cameras) can be cumbersome and error-prone.
- **State Management Issues**: Managing rendering states (like blend modes, shaders, etc.) can become complex, especially when different objects require different states.(Fortunately, we have [RenderBatcher](./render-batcher.md) to help us manage the states.)

## How RenderPipeline Works?
The RenderPipeline operates by processing a series of rendering commands in a specific order. Each stage of the pipeline is responsible for a particular task, such as culling, sorting, or applying post-processing effects. This separation of concerns makes it easier to manage the rendering process and implement new features.

At it's core, the RenderPipeline is Pass-based. It's a list of passes. We render them in sequence. Each Pass represents a distinct phase in the rendering process. For example, you might have passes for:

- Scene Rendering
- Post-Processing
- UI Rendering

You can customize the RenderPipeline by adding, removing, or modifying passes to suit your game's needs.


``` cs
Engine.RenderPipeline.AddPass(new SceneRenderPass());  //Render the Game Scene  
Engine.RenderPipeline.AddPass(new PostProcessPass());  //Apply Post-Processing Effects
Engine.RenderPipeline.AddPass(new UIRenderPass());     //Render the User Interface
Engine.RenderPipeline.AddPass(new FinalCompositionPass()); //Compose the Final Image
```
!!! warning
    This document is a work in progress and may be updated frequently.

### Default RenderPipeline
### Visual Resolution Handling
### Ping-Pong Buffering
