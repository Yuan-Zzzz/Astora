using Astora.Core.Attributes;
using Microsoft.Xna.Framework;

namespace Astora.Core.Nodes
{
    /// <summary>
    /// 2D Node Base Class
    /// </summary>
    public class Node2D : Node
    {
        /// <summary>
        /// Node Position in 2D Space
        /// </summary>
        [SerializeField]
        private Vector2 _position = Vector2.Zero;
        
        public Vector2 Position 
        { 
            get => _position; 
            set => _position = value; 
        }
        
        /// <summary>
        /// Node Rotation in Radians
        /// </summary>
        [SerializeField]
        private float _rotation = 0f;
        
        public float Rotation 
        { 
            get => _rotation; 
            set => _rotation = value; 
        }
        
        /// <summary>
        /// Node Scale in 2D Space
        /// </summary>
        [SerializeField]
        private Vector2 _scale = Vector2.One;
        
        public Vector2 Scale 
        { 
            get => _scale; 
            set => _scale = value; 
        }

        public Node2D() : base() { }

        public Node2D(string name = "Node2D") : base(name) { }
        
        /// <summary>
        /// Gets the global transformation matrix of the Node2D, combining its local transform with its parent's global transform if applicable.
        /// </summary>
        public Matrix GlobalTransform
        {
            get
            {
                // Create local transformation matrix
                var localMat = Matrix.CreateScale(new Vector3(Scale, 1)) *
                               Matrix.CreateRotationZ(Rotation) *
                               Matrix.CreateTranslation(new Vector3(Position, 0));
                
                // If there is a parent Node2D, combine with its global transform
                if (Parent is Node2D parent2d)
                {
                    return localMat * parent2d.GlobalTransform;
                }

                return localMat;
            }
        }
        
        /// <summary>
        /// Gets or sets the global position of the Node2D in world space.
        /// </summary>
        public Vector2 GlobalPosition
        {
            get
            {
                var mat = GlobalTransform;
                return new Vector2(mat.Translation.X, mat.Translation.Y);
            }
            set
            {
                if (Parent is Node2D parent2d)
                {
                    // Inverse transform the global position to local space
                    var parentGlobalMat = parent2d.GlobalTransform;
                    Matrix.Invert(ref parentGlobalMat, out var invParentMat);
                    var localPos = Vector2.Transform(value, invParentMat);
                    Position = localPos;
                }
                else
                {
                    Position = value;
                }
            }
        }
    }
}