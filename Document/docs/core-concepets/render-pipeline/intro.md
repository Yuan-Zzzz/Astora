## Render Pipeline
The Render Pipeline is a core module of the Astora Engine, what is is the manager of all rendering operations. It is deal rendering automatically, and it is highly customizable through the use of Render Passes and other settings.

## Base Structure
```mermaid
graph TD
    A[**Engine**<br/>Core of Astora, Manage any part of the engine] --> B[**RenderPipeline**<br/>Manager of rendering operations] 
    B --> C[**RenderBatcher**<br/>Manager of render batches, based on SpriteBatch]
```
