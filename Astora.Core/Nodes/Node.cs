using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core
{
    public class Node
    {
        /// <summary>
        /// Name of the node for identification
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Parent node in the hierarchy
        /// </summary>
        public Node Parent { get; private set; }
        
        /// <summary>
        /// Node children in the hierarchy
        /// </summary>
        public List<Node> Children { get; private set; } = new List<Node>();
        
        // Mark if this node is queued for deletion
        public bool IsQueuedForDeletion { get; private set; } = false;

        public Node(string name = "Node")
        {
            Name = name;
        }

        #region Child Management
        /// <summary>
        /// Adds a child node to this node.
        /// </summary>
        /// <param name="child"></param>
        public void AddChild(Node child)
        {
            if (child.Parent != null)
                child.Parent.RemoveChild(child);

            child.Parent = this;
            Children.Add(child);
            child.Ready(); 
        }

        /// <summary>
        /// Removes a child node from this node.
        /// </summary>
        /// <param name="child"></param>
        public void RemoveChild(Node child)
        {
            if (Children.Contains(child))
            {
                child.Parent = null;
                Children.Remove(child);
            }
        }

        /// <summary>
        /// Marks this node and its children for deletion.
        /// </summary>
        public void QueueFree()
        {
            IsQueuedForDeletion = true;
            foreach (var child in Children)
            {
                child.QueueFree();
            }
        }
        #endregion

        #region Lifecycle Methods
        public virtual void Ready() { }
        public virtual void Update(float delta) { }
        public virtual void Draw(SpriteBatch spriteBatch) { }
        #endregion

        #region Internal Methods
        public void InternalUpdate(GameTime gameTime)
        {
            if (IsQueuedForDeletion) return;
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Update(delta);
            
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                Children[i].InternalUpdate(gameTime);
            }
            
            Children.RemoveAll(c => c.IsQueuedForDeletion);
        }
        public void InternalDraw(SpriteBatch spriteBatch)
        {
            if (IsQueuedForDeletion) return;
            
            Draw(spriteBatch);
            
            foreach (var child in Children)
            {
                child.InternalDraw(spriteBatch);
            }
        }
        #endregion
    }
}