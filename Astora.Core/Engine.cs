using Astora.Core.Scene;
using Astora.Core.Utils;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core;

public static class Engine
{
    public static ContentManager Content { get; set; }
    public static GraphicsDevice GraphicsDevice { get; set; }
    public static SpriteBatch SpriteBatch { get; set; }
    public static SceneTree CurretScene { get; set; }
    public static ISceneSerializer Serializer { get; set; } = new YamlSceneSerializer();
        
    public static T Load<T>(string path)
    {
        return Content.Load<T>(path);
    }
}