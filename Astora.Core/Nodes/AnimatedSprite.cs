using System;
using Astora.Core.Attributes;
using Astora.Core.Resources;
using Microsoft.Xna.Framework;

namespace Astora.Core.Nodes;

public class AnimatedSprite : Sprite
{
    // Non-serialized runtime field
    private SpriteFrames _frames;
    
    public SpriteFrames Frames 
    { 
        get => _frames; 
        set => _frames = value; 
    }

    public string Animation 
    { 
        get => _currentAnimationName;
        set 
        {
            if (_currentAnimationName != value)
                Play(value);
        }
    }

    public int Frame 
    { 
        get => _frameIndex;
        set 
        {
            _frameIndex = value;
            UpdateRegion();
        }
    }

    [SerializeField]
    private float _speedScale = 1.0f;
    
    public float SpeedScale 
    { 
        get => _speedScale; 
        set => _speedScale = value; 
    }
    
    public bool Playing { get; private set; } = false;
    
    public event Action<string> OnAnimationFinished;
    public event Action<string> OnFrameChanged;
    
    private string _currentAnimationName;
    private int _frameIndex;
    private double _timer;
    private SpriteFrames.Animation _currentAnimData;

    public AnimatedSprite() : this("AnimatedSprite") { }

    public AnimatedSprite(string name, SpriteFrames frames = null) : base(name, null)
    {
        _frames = frames;
        if (_frames != null)
        {
            Texture = _frames.Texture;
        }
    }
    
    public void Play(string name = null, float customSpeed = 1.0f, bool fromEnd = false)
    {
        if (Frames == null) return;
        
        if (string.IsNullOrEmpty(name))
        {
            name = _currentAnimationName;
            if (string.IsNullOrEmpty(name)) return;
        }

        if (!Frames.HasAnimation(name))
        {
            Logger.Warn($"Animation not found: {name}");
            return;
        }

        bool isNewAnimation = _currentAnimationName != name;
        _currentAnimationName = name;
        _currentAnimData = Frames.GetAnimation(name);
        SpeedScale = customSpeed;
        Playing = true;
        
        if (Texture != Frames.Texture) Texture = Frames.Texture;

        if (fromEnd)
        {
            _frameIndex = _currentAnimData.Frames.Count - 1;
        }
        else if (isNewAnimation || _frameIndex >= _currentAnimData.Frames.Count)
        {
            _frameIndex = 0;
        }
        
        _timer = 0;
        UpdateRegion();
    }

    public void Stop()
    {
        Playing = false;
        _frameIndex = 0;
        UpdateRegion();
    }

    public void Pause()
    {
        Playing = false;
    }
    
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        if (!Playing || _currentAnimData == null || _currentAnimData.Frames.Count == 0) 
            return;
        
        double timePerFrame = 1.0 / (_currentAnimData.Fps * Math.Abs(SpeedScale));

        _timer += deltaTime;

        if (_timer >= timePerFrame)
        {
            while (_timer >= timePerFrame)
            {
                _timer -= timePerFrame;
                int nextFrame = _frameIndex + (int)Math.Sign(SpeedScale);
                
                if (nextFrame >= _currentAnimData.Frames.Count)
                {
                    if (_currentAnimData.Loop)
                    {
                        nextFrame = 0;
                    }
                    else
                    {
                        nextFrame = _currentAnimData.Frames.Count - 1;
                        Playing = false;
                        OnAnimationFinished?.Invoke(_currentAnimationName);
                        break; 
                    }
                }
                else if (nextFrame < 0)
                {
                    if (_currentAnimData.Loop)
                    {
                        nextFrame = _currentAnimData.Frames.Count - 1;
                    }
                    else
                    {
                        nextFrame = 0;
                        Playing = false;
                        OnAnimationFinished?.Invoke(_currentAnimationName);
                        break;
                    }
                }
                if (_frameIndex != nextFrame)
                {
                    _frameIndex = nextFrame;
                    OnFrameChanged?.Invoke(_currentAnimationName);
                }
            }
            UpdateRegion();
        }
    }

    private void UpdateRegion()
    {
        if (_currentAnimData != null && _currentAnimData.Frames.Count > 0)
        {
            if (_frameIndex < 0) _frameIndex = 0;
            if (_frameIndex >= _currentAnimData.Frames.Count) _frameIndex = _currentAnimData.Frames.Count - 1;
            
            this.Region = _currentAnimData.Frames[_frameIndex];
        }
    }
}
