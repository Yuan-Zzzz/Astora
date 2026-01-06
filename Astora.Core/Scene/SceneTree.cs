using Astora.Core.Inputs;
using Astora.Core.Nodes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Scene
{
    public class SceneTree
    {
        /// <summary>
        /// Root Node of the Scene
        /// </summary>
        public Node Root { get; private set; }
        /// <summary>
        /// Currently Active Camera
        /// </summary>
        public Camera2D ActiveCamera { get; set; }

        public void ChangeScene(Node newSceneRoot)
        {
            // Set new root
            Root = newSceneRoot;
            
            // Call Ready on the new root
            if (Root != null)
            {
                Root.Ready();
                
                // // 自动查找并设置Camera2D
                var camera = GetNode<Camera2D>();
                if (camera != null)
                {
                    ActiveCamera = camera;
                }
            }
            else
            {
                ActiveCamera = null;
            }
        }

        /// <summary>
        /// Load Scene from file
        /// </summary>
        public void LoadScene(string scenePath)
        {
            if (Engine.Serializer == null)
                throw new InvalidOperationException("Scene serializer not set.");
            
            var scene = Engine.Serializer.Load(scenePath);
            ChangeScene(scene);
        }

        /// <summary>
        /// Update Nodes
        /// </summary>
        /// <param name="gameTime"></param>
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
        
        /// <summary>
        /// 清理标记删除的节点（用于编辑器模式）
        /// </summary>
        public void CleanupQueuedNodes()
        {
            if (Root != null)
            {
                CleanupNode(Root);
            }
        }
        
        private void CleanupNode(Node node)
        {
            // 先清理子节点
            for (int i = node.Children.Count - 1; i >= 0; i--)
            {
                var child = node.Children[i];
                CleanupNode(child);
                
                if (child.IsQueuedForDeletion)
                {
                    node.Children.RemoveAt(i);
                }
            }
        }
        
        /// <summary>
        /// Get Node by Type
        /// </summary>
        public T GetNode<T>() where T : Node
        {
            return FindNode<T>(Root);
        }
        
        /// <summary>
        /// Find Node by Name
        /// </summary>
        public Node FindNode(string name)
        {
            return FindNodeByName(Root, name);
        }
        
        /// <summary>
        /// Find Node by Type (recursive)
        /// </summary>
        /// <param name="node">Start Node</param>
        /// <typeparam name="T">Node Type</typeparam>
        /// <returns></returns>
        private T FindNode<T>(Node node) where T : Node
        {
            if (node == null) return null;
            if (node is T result) return result;
            
            foreach (var child in node.Children)
            {
                var found = FindNode<T>(child);
                if (found != null) return found;
            }
            
            return null;
        }
        
        /// <summary>
        /// Find Node by Name (recursive)
        /// </summary>
        /// <param name="node">Start Node</param>
        /// <param name="name">Target Name</param>
        /// <returns></returns>
        private Node FindNodeByName(Node node, string name)
        {
            if (node == null) return null;
            if (node.Name == name) return node;
            
            foreach (var child in node.Children)
            {
                var found = FindNodeByName(child, name);
                if (found != null) return found;
            }
            
            return null;
        }
    }
}