 using Microsoft.Xna.Framework;
 using Microsoft.Xna.Framework.Graphics;

 namespace Astora.Core.Resources;

 public class SpriteFrames
 {
     public class Animation
     {
         public string Name { get; set; }
         public float Fps { get; set; } = 5f;
         public bool Loop { get; set; } = true;
         public List<Rectangle> Frames { get; set; } = new List<Rectangle>();
     }
     
     private Dictionary<string, Animation> _animations = new Dictionary<string, Animation>();
     
     public Texture2D Texture { get; private set; }

     public SpriteFrames(Texture2D texture)
     {
         Texture = texture;
     }

     public void AddAnimation(string name, float fps = 10f, bool loop = true)
     {
         if (!_animations.ContainsKey(name))
         {
             _animations[name] = new Animation 
             { 
                 Name = name, 
                 Fps = fps, 
                 Loop = loop 
             };
         }
     }

     public void AddFrame(string animationName, Rectangle region)
     {
         if (_animations.TryGetValue(animationName, out var anim))
         {
             anim.Frames.Add(region);
         }
     }
     
     public void AddFramesFromAtlas(string animationName, TextureAtlas2D atlas, string[] frameNames)
     {
         foreach (var name in frameNames)
         {
             var rect = atlas.GetFrame(name);
             if (rect.HasValue)
             {
                 AddFrame(animationName, rect.Value);
             }
         }
     }

     public Animation GetAnimation(string name)
     {
         if (_animations.TryGetValue(name, out var anim))
             return anim;
         return null;
     }
        
     public bool HasAnimation(string name) => _animations.ContainsKey(name);
 }