using System;
using Astora.Core.Attributes;
using Astora.Core.Rendering.RenderPipeline;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Nodes;

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
    
    [SerializeField]
    private ParticleEmissionShape _emissionShape = ParticleEmissionShape.Point;
    
    [SerializeField]
    private Vector2 _emissionBoxExtents = new Vector2(10f, 10f);
    
    [SerializeField]
    private float _emissionSphereRadius = 10f;
    
    [SerializeField]
    private float _tangentialAccel = 0f;
    
    [SerializeField]
    private bool _emitting = true;
    
    [SerializeField]
    private int _amount = 100;
    
    [SerializeField]
    private bool _oneShot = false;
    
    [SerializeField]
    private float _lifetime = 1.0f;
    
    [SerializeField]
    private bool _localCoords = false;
    
    [SerializeField]
    private Vector2 _gravity = new Vector2(0, 980f);
    
    [SerializeField]
    private Vector2 _direction = new Vector2(1, 0);
    
    [SerializeField]
    private float _spread = 45f;
    
    [SerializeField]
    private float _initialVelocityMin = 100f;
    
    [SerializeField]
    private float _initialVelocityMax = 200f;
    
    [SerializeField]
    private float _angularVelocity = 0f;
    
    [SerializeField]
    private Color _colorStart = Color.White;
    
    [SerializeField]
    private Color _colorEnd = Color.Transparent;
    
    [SerializeField]
    private float _scaleStart = 1.0f;
    
    [SerializeField]
    private float _scaleEnd = 1.0f;
    
    // Non-serialized runtime fields
    private Texture2D _texture;
    private BlendState _blendState = BlendState.AlphaBlend;
    
    public ParticleEmissionShape EmissionShape 
    { 
        get => _emissionShape; 
        set => _emissionShape = value; 
    }
    
    /// <summary>
    /// Extents of the emission box shape
    /// </summary>
    public Vector2 EmissionBoxExtents 
    { 
        get => _emissionBoxExtents; 
        set => _emissionBoxExtents = value; 
    }
    
    /// <summary>
    /// Emission sphere radius
    /// </summary>
    public float EmissionSphereRadius 
    { 
        get => _emissionSphereRadius; 
        set => _emissionSphereRadius = value; 
    }
    
    /// <summary>
    /// Radial acceleration applied to particles
    /// </summary>
    public float TangentialAccel 
    { 
        get => _tangentialAccel; 
        set => _tangentialAccel = value; 
    }
    
    /// <summary>
    /// Texture used for particles
    /// </summary>
    public Texture2D Texture 
    { 
        get => _texture; 
        set => _texture = value; 
    }
    
    /// <summary>
    /// Whether the particle system is currently emitting particles
    /// </summary>
    public bool Emitting 
    { 
        get => _emitting; 
        set => _emitting = value; 
    }
    
    /// <summary>
    /// Particle pool size
    /// </summary>
    public int Amount 
    { 
        get => _amount; 
        set
        {
            if (value != _amount && value > 0)
            {
                ResizePool(value);
            }
        }
    }
    
    /// <summary>
    /// Particle system will emit all particles once and stop
    /// </summary>
    public bool OneShot 
    { 
        get => _oneShot; 
        set => _oneShot = value; 
    }
    
    /// <summary>
    /// Lifetime of each particle in seconds
    /// </summary>
    public float Lifetime 
    { 
        get => _lifetime; 
        set => _lifetime = value; 
    }
    
    /// <summary>
    /// Whether particles use local coordinates (true) or global coordinates (false)
    /// </summary>
    public bool LocalCoords 
    { 
        get => _localCoords; 
        set => _localCoords = value; 
    }
    
    /// <summary>
    /// Simulation space gravity applied to particles
    /// </summary>
    public Vector2 Gravity 
    { 
        get => _gravity; 
        set => _gravity = value; 
    }
    
    /// <summary>
    /// Initial emission direction of particles
    /// </summary>
    public Vector2 Direction 
    { 
        get => _direction; 
        set => _direction = value; 
    }
    
    /// <summary>
    /// Spread angle in degrees around the Direction vector
    /// </summary>
    public float Spread 
    { 
        get => _spread; 
        set => _spread = value; 
    }
    
    /// <summary>
    /// Minimum initial velocity of particles
    /// </summary>
    public float InitialVelocityMin 
    { 
        get => _initialVelocityMin; 
        set => _initialVelocityMin = value; 
    }
    
    /// <summary>
    /// Maximum initial velocity of particles
    /// </summary>
    public float InitialVelocityMax 
    { 
        get => _initialVelocityMax; 
        set => _initialVelocityMax = value; 
    }
    
    /// <summary>
    /// Angular velocity applied to particles in radians per second
    /// </summary>
    public float AngularVelocity 
    { 
        get => _angularVelocity; 
        set => _angularVelocity = value; 
    }
    
    /// <summary>
    /// Starting color of particles
    /// </summary>
    public Color ColorStart 
    { 
        get => _colorStart; 
        set => _colorStart = value; 
    }
    
    /// <summary>
    /// Ending color of particles
    /// </summary>
    public Color ColorEnd 
    { 
        get => _colorEnd; 
        set => _colorEnd = value; 
    }
    
    /// <summary>
    /// Starting scale of particles
    /// </summary>
    public float ScaleStart 
    { 
        get => _scaleStart; 
        set => _scaleStart = value; 
    }
    
    /// <summary>
    /// Ending scale of particles
    /// </summary>
    public float ScaleEnd 
    { 
        get => _scaleEnd; 
        set => _scaleEnd = value; 
    }

    /// <summary>
    /// Blend state used for rendering particles
    /// </summary>
    public BlendState BlendState 
    { 
        get => _blendState; 
        set => _blendState = value; 
    }

    private Particle[] _particles;
    private Random _random;
    private float _timeSinceLastEmission;
    private Texture2D _defaultTexture;

    public CPUParticles2D() : this("CPUParticles2D") { }

    public CPUParticles2D(string name, int amount = 100) : base(name)
    {
        _random = new Random();
        _amount = amount;
        ResizePool(amount);
    }

    /// <summary>
    /// Resize the particle pool to a new amount
    /// </summary>
    public void ResizePool(int newAmount)
    {
        _amount = newAmount;
        _particles = new Particle[_amount];
        for (int i = 0; i < _amount; i++)
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
    public override void Draw(IRenderBatcher renderBatcher)
    {
        var tex = Texture ?? GetDefaultTexture(Engine.GDM.GraphicsDevice);
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

            renderBatcher.Draw(
                tex,
                drawPos,
                null,
                p.Color,
                p.Rotation,
                origin,
                new Vector2(p.Scale),
                SpriteEffects.None,
                0f,
                this.BlendState
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
