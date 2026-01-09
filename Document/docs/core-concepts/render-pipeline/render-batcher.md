# RenderBatcher
RenderBatcher is the encapsulation of MonoGame's SpriteBatch in Astora Engine. It provides a higher-level interface for managing render batches, making it easier to deal some rendering operations.

## About SpriteBatch
The SpriteBatch is a batch processing system of MonoGame that allows you to draw multiple sprites with a single draw call, improving rendering performance. It use Begin/End methods to define a batch of sprites which will be drawn together.Here is the basic usage of SpriteBatch:

```cs
spriteBatch.Begin(...Something Parameters...);
spriteBatch.Draw(...);
spriteBatch.Draw(...);
...
spriteBatch.End();
```

When you call `Begin`, The Sprite Batch will set up some necessary states for rendering, such as the transformation matrix, the blend state, and the sampler state. This allows you to draw your sprites with the correct settings without having to change these states manually for each draw call.And whe you call `End`,The Sprite Batch will flush all the sprites that have been drawn since the last `Begin` call to the graphics device in a single draw call.


## Talk about Transform Matrix
We mentioned the transformation matrix in the previous section. So what it is?

Imagine here is a canvas, and a rect is located at (50,100) on this canvas. If we move up the rect by 10 units, and then move right by 20 units. Eventually, the point will be located at (70,90). Same as scaling and rotation, we can use translate the rect attributes to achieve the same effect. However, here comes the problem,. There are too many ways to transform a rect, and if we want to combine multiple transformations, it will become very complicated. This is where the transformation matrix comes in. A transformation matrix is a mathematical tool that can represent multiple transformations in a single matrix.

By using matrix multiplication, we can combine multiple transformations into a single matrix. For example, if we want to first translate the rect by (20,-10), then scale it by 2 times, and finally rotate it by 45 degrees, we can create three separate matrices for each transformation and then multiply them together to get a single transformation matrix. This matrix can then be applied to the rect to achieve the desired transformation.

So you can see that the transformation matrix is a magic box that can transform positions, scales, and rotations all at once.

!!! warning
    I think the part should be in artical about Camera, but just remind here.

## Why RenderBatcher?
While SpriteBatch is Powerful, using it directly is facing a limitation on state management. 

When you call `SpriteBatch.Begin()`, the rendering states(BlendState, SamplerState, Effect, TransformMatrix, etc.) are set for the entire batch. 

Imagining a situation, you are drawing a scene with multiple sprites. They have the same rendering states, like all the blend state is AlphaBlend.Suddenly, you need to draw some particles with Additive blend state, or apply a special shader effect to a chraracter sprite.you can't switch the modes in the middle of a batch simply.

### The Problem: Manual State Management
So without a manager, everything what will be drawn needs to manage the rendring lifecycle itself. A simplest implementation may look like this:

```cs
//It's in ParticleSystem render loop
// 1. We have to interrupt the global batch because we need Additive blending
spriteBatch.End(); 
// 2. Start a new batch with our specific settings
spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, ..., globalTransform);
// 3. Draw our particles
foreach(var p in particles) spriteBatch.Draw(...);
// 4. End our batch
spriteBatch.End();
// 5. RESTORE the original batch for the next object? 
spriteBatch.Begin(..., BlendState.AlphaBlend, ...);
```

So here comes the problem:
- We have to manually manage the batch lifecycle, which is error-prone and tedious.
- If not managed carefully, you might accidentally break batches too often, increasing draw calls significantly.
- GameObject need to know too much about the global rendering context (like the Camera's View Matrix), but it's unnecessary for them to be aware of these details.

### The Solution: RenderBatcher
RenderBatcher is designed to solve these problems by managing multiple render batches automatically. It allows different parts of your rendering code to request specific rendering states without worrying about the underlying batch management.

We just need to call RenderBatcher.Draw with the desired settings, and the RenderBatcher will take care of starting and ending batches as needed. This way, you can focus on what to draw rather than how to manage the drawing process.

## What is RenderBatcher?
The answer is simple: It's a state machine for SpriteBatch.

At its core, RenderBatcher is the middleware between your rendering code and the SpriteBatch. You just need to tell your RenderBatcher what to draw and with which settings. No more the `Begin`/`End` calls, just draw directly.

RenderBatcher tracks the current rendering state internally. 
- _currentTransformMatrix: The current transformation matrix applied to all sprites.
- _currentBlendState: The current blend state for rendering.
- _currentSamplerState: The current sampler state for textures.
- _currentEffect: The current shader effect applied to sprites.

### How it Works
When you call `RenderBatcher.Draw`, it deal a fast check:

- Check requested and compare with current states.
- If they match, it simply draws the sprite using the existing batch.
- If they differ, it ends the current batch (if any), updates the states, and starts a new batch with the requested settings.