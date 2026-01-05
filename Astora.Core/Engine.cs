using Astora.Core.Scene;
using Astora.Core.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core;

public static class Engine
{
    public static ContentManager Content { get; private set; }
    public static GraphicsDevice GraphicsDevice { get; private set; }
    public static SpriteBatch SpriteBatch { get; private set; }
    public static SceneTree CurrentScene { get; set; }
    public static ISceneSerializer Serializer { get; set; } = new YamlSceneSerializer();
    
    /// <summary>
    /// 统一初始化引擎
    /// </summary>
    public static void Initialize(ContentManager content, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        Content = content;
        GraphicsDevice = graphicsDevice;
        SpriteBatch = spriteBatch;
        CurrentScene = new SceneTree();
    }
    
    /// <summary>
    /// 便捷的场景加载方法
    /// </summary>
    public static void LoadScene(string scenePath)
    {
        if (CurrentScene == null)
            throw new InvalidOperationException("Engine not initialized. Call Engine.Initialize() first.");
        
        if (Serializer == null)
            throw new InvalidOperationException("Scene serializer not set.");
        
        var scene = Serializer.Load(scenePath);
        CurrentScene.ChangeScene(scene);
    }
    
    /// <summary>
    /// 游戏循环更新方法
    /// </summary>
    public static void Update(GameTime gameTime)
    {
        CurrentScene?.Update(gameTime);
    }
    
    /// <summary>
    /// 游戏循环渲染方法
    /// </summary>
    public static void Render(Color? clearColor = null)
    {
        if (GraphicsDevice == null || SpriteBatch == null || CurrentScene == null)
            return;
        
        GraphicsDevice.Clear(clearColor ?? Color.Black);
        
        Matrix viewMatrix = Matrix.Identity;
        if (CurrentScene.ActiveCamera != null)
        {
            viewMatrix = CurrentScene.ActiveCamera.GetViewMatrix();
        }
        
        SpriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: viewMatrix
        );
        
        CurrentScene.Draw(SpriteBatch);
        
        SpriteBatch.End();
    }
    
    /// <summary>
    /// 便捷的资源加载方法
    /// </summary>
    public static T Load<T>(string path) where T : class
    {
        if (Content == null)
            throw new InvalidOperationException("Engine not initialized. Call Engine.Initialize() first.");
        return Content.Load<T>(path);
    }
}