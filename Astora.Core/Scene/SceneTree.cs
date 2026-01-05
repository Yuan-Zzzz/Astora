using Astora.Core.Inputs;
using Astora.Core.Nodes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Scene
{
    public class SceneTree
    {
        public Node Root { get; private set; }
        public Camera2D ActiveCamera { get; set; }

        public void ChangeScene(Node newSceneRoot)
        {
            // Set new root
            Root = newSceneRoot;
            
            // Call Ready on the new root
            if (Root != null)
            {
                Root.Ready();
            }
        }

        public void Update(GameTime gameTime)
        {
            // Update Input
            Input.Update();

            // Update Nodes
            if (Root != null)
            {
                Root.InternalUpdate(gameTime);
            }
        }

        /// <summary>
        /// Draw Nodes
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (Root != null)
            {
                Root.InternalDraw(spriteBatch);
            }
        }
        
        /// <summary>
        /// Set the current active camera
        /// </summary>
        /// <param name="camera"></param>
        public void SetCurrentCamera(Camera2D camera)
        {
            ActiveCamera = camera;
        }
    }
}