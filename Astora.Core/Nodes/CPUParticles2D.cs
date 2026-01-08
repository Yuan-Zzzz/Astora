using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Nodes
{
    public class CPUParticles2D : Node2D
    {
        /// <summary>
        /// The particle data structure
        /// </summary>
        private class Particle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public Color Color;
            public float Scale;
            public float Rotation;
            public float TimeAlive;
            public float LifeTime;
            public bool IsActive;
            public Vector2 SpawnPosition; 
        }
        
        /// <summary>
        /// The shape from which particles are emitted
        /// </summary>
        public enum ParticleEmissionShape
        {
            Point,
            Box,
            Sphere
        }
        
        public ParticleEmissionShape EmissionShape { get; set; } = ParticleEmissionShape.Point;
        /// <summary>
        /// Extents of the emission box shape
        /// </summary>
        public Vector2 EmissionBoxExtents { get; set; } = new Vector2(10f, 10f);
        /// <summary>
        /// Emission sphere radius
        /// </summary>
        public float EmissionSphereRadius { get; set; } = 10f;
        
        /// <summary>
        /// Radial acceleration applied to particles
        /// </summary>
        public float TangentialAccel { get; set; } = 0f;
        
        /// <summary>
        /// Texture used for particles
        /// </summary>
        public Texture2D Texture { get; set; }
        /// <summary>
        /// Whether the particle system is currently emitting particles
        /// </summary>
        public bool Emitting { get; set; } = true;
        /// <summary>
        /// Particle pool size
        /// </summary>
        public int Amount { get; private set; } = 100;
        /// <summary>
        /// Particle system will emit all particles once and stop
        /// </summary>
        public bool OneShot { get; set; } = false;
        /// <summary>
        /// Lifetime of each particle in seconds
        /// </summary>
        public float Lifetime { get; set; } = 1.0f;
        /// <summary>
        /// Whether particles use local coordinates (true) or global coordinates (false)
        /// </summary>
        public bool LocalCoords { get; set; } = false;
        
        /// <summary>
        /// Simulation space gravity applied to particles
        /// </summary>
        public Vector2 Gravity { get; set; } = new Vector2(0, 980f);
        /// <summary>
        /// Initial emission direction of particles
        /// </summary>
        public Vector2 Direction { get; set; } = new Vector2(1, 0); 
        /// <summary>
        /// Spread angle in degrees around the Direction vector
        /// </summary>
        public float Spread { get; set; } = 45f;
        /// <summary>
        /// Minimum initial velocity of particles
        /// </summary>
        public float InitialVelocityMin { get; set; } = 100f;
        /// <summary>
        /// Maximum initial velocity of particles
        /// </summary>
        public float InitialVelocityMax { get; set; } = 200f;
        /// <summary>
        /// Angular velocity applied to particles in radians per second
        /// </summary>
        public float AngularVelocity { get; set; } = 0f; 
        
        /// <summary>
        /// Starting color of particles
        /// </summary>
        public Color ColorStart { get; set; } = Color.White;
        /// <summary>
        /// Ending color of particles
        /// </summary>
        public Color ColorEnd { get; set; } = Color.Transparent;
        /// <summary>
        /// Starting scale of particles
        /// </summary>
        public float ScaleStart { get; set; } = 1.0f;
        /// <summary>
        /// Ending scale of particles
        /// </summary>
        public float ScaleEnd { get; set; } = 1.0f;

        /// <summary>
        /// Blend state used for rendering particles
        /// </summary>
        public BlendState BlendState { get; set; } = BlendState.AlphaBlend;

        private Particle[] _particles;
        private Random _random;
        private float _timeSinceLastEmission;
        private Texture2D _defaultTexture;

        public CPUParticles2D(string name, int amount = 100) : base(name)
        {
            _random = new Random();
            ResizePool(amount);
        }

        /// <summary>
        /// Resize the particle pool to a new amount
        /// </summary>
        public void ResizePool(int newAmount)
        {
            Amount = newAmount;
            _particles = new Particle[Amount];
            for (int i = 0; i < Amount; i++)
            {
                _particles[i] = new Particle { IsActive = false };
            }
        }

        /// <summary>
        /// Update the particle system
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void Update(float deltaTime)
        {
            float dt = deltaTime;
            
            if (Emitting)
            {
                float emissionRate = Lifetime > 0 ? Amount / Lifetime : 0;
                float interval = emissionRate > 0 ? 1.0f / emissionRate : 0;

                if (interval > 0)
                {
                    _timeSinceLastEmission += dt;
                    while (_timeSinceLastEmission >= interval)
                    {
                        SpawnParticle();
                        _timeSinceLastEmission -= interval;
                        
                        if (OneShot && ActiveParticleCount() >= Amount)
                        {
                            Emitting = false;
                            break;
                        }
                    }
                }
            }
            
            for (int i = 0; i < Amount; i++)
            {
                var p = _particles[i];
                if (!p.IsActive) continue;

                p.TimeAlive += dt;
                if (p.TimeAlive >= p.LifeTime)
                {
                    p.IsActive = false;
                    continue;
                }
                
                float t = p.TimeAlive / p.LifeTime;
                
                p.Velocity += Gravity * dt;
                // Deal with tangential acceleration
                if (TangentialAccel != 0)
                {
                    Vector2 center = LocalCoords ? Vector2.Zero : p.SpawnPosition;
                    Vector2 radial = p.Position - center;
                    
                    if (radial != Vector2.Zero)
                    {
                        radial.Normalize();
                    }
                    else
                    {
                        radial = Vector2.UnitX; 
                    }
                    
                    Vector2 tangent = new Vector2(-radial.Y, radial.X);
                    
                    p.Velocity += tangent * TangentialAccel * dt;
                }
                
                p.Position += p.Velocity * dt;
                p.Rotation += AngularVelocity * dt;
                
                p.Color = Color.Lerp(ColorStart, ColorEnd, t);
                p.Scale = MathHelper.Lerp(ScaleStart, ScaleEnd, t);
            }
        }

        /// <summary>
        /// Spawn a new particle from the pool
        /// </summary>
        private void SpawnParticle()
        {
            Particle p = null;
            for (int i = 0; i < Amount; i++)
            {
                if (!_particles[i].IsActive)
                {
                    p = _particles[i];
                    break;
                }
            }

            if (p == null) return;
            
            p.IsActive = true;
            p.TimeAlive = 0f;
            p.LifeTime = Lifetime; 
            Vector2 offset = Vector2.Zero;

            switch (EmissionShape)
            {
                case ParticleEmissionShape.Point:
                    offset = Vector2.Zero;
                    break;
                case ParticleEmissionShape.Box:
                    float x = (NextFloat() - 0.5f) * EmissionBoxExtents.X;
                    float y = (NextFloat() - 0.5f) * EmissionBoxExtents.Y;
                    offset = new Vector2(x, y);
                    break;
                case ParticleEmissionShape.Sphere:
                    float angle = NextFloat() * MathHelper.TwoPi;
                    float radius = (float)Math.Sqrt(NextFloat()) * EmissionSphereRadius;
                    offset = new Vector2((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius);
                    break;
            }
            
            if (LocalCoords)
            {
                p.Position = offset;
                p.SpawnPosition = Vector2.Zero;
            }
            else
            {
                p.Position = GlobalPosition + offset;
                p.SpawnPosition = GlobalPosition;
            }
            
            float angleRad = (float)Math.Atan2(Direction.Y, Direction.X);
            float spreadRad = MathHelper.ToRadians(Spread);
            float randomAngle = angleRad + (NextFloat() - 0.5f) * spreadRad;
            Vector2 dirVec = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));
            p.Velocity = dirVec * MathHelper.Lerp(InitialVelocityMin, InitialVelocityMax, NextFloat());

            p.Color = ColorStart;
            p.Scale = ScaleStart; 
            p.Rotation = 0f;
        }

        /// <summary>
        /// Draw the particles
        /// </summary>
        /// <param name="spriteBatch"></param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            Engine.SetRenderState(BlendState); 
            var tex = Texture ?? GetDefaultTexture(spriteBatch.GraphicsDevice);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
            

            for (int i = 0; i < Amount; i++)
            {
                var p = _particles[i];
                if (!p.IsActive) continue;

                Vector2 drawPos = p.Position;

             
                if (LocalCoords)
                {
                    Vector2 globalPos, globalScale;
                    float globalRot;
                    DecomposeGlobalTransform(out globalPos, out globalRot, out globalScale);
                    
                    drawPos += globalPos; 
                }

                spriteBatch.Draw(
                    tex,
                    drawPos,
                    null,
                    p.Color,
                    p.Rotation,
                    origin,
                    p.Scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }
        
        private int ActiveParticleCount()
        {
            int count = 0;
            for (int i = 0; i < Amount; i++) if (_particles[i].IsActive) count++;
            return count;
        }
        
        private float NextFloat()
        {
            return (float)_random.NextDouble();
        }

        private Texture2D GetDefaultTexture(GraphicsDevice device)
        {
            if (_defaultTexture == null)
            {
                _defaultTexture = new Texture2D(device, 4, 4);
                Color[] data = new Color[16];
                for(int i=0; i<16; i++) data[i] = Color.White;
                _defaultTexture.SetData(data);
            }
            return _defaultTexture;
        }
        
        private void DecomposeGlobalTransform(out Vector2 pos, out float rot, out Vector2 scale)
        {
            GlobalTransform.Decompose(out Vector3 s, out Quaternion r, out Vector3 t);
            pos = new Vector2(t.X, t.Y);
            scale = new Vector2(s.X, s.Y);
            Vector3 direction = Vector3.Transform(Vector3.Right, r);
            rot = (float)Math.Atan2(direction.Y, direction.X);
        }
    }
}