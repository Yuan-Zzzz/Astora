using System;
using Astora.Core.Resources;
using Microsoft.Xna.Framework;

namespace Astora.Core.Nodes
{
    public class AnimatedSprite : Sprite
    {
        public SpriteFrames Frames { get; set; }

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

        public float SpeedScale { get; set; } = 1.0f;
        public bool Playing { get; private set; } = false;
        
        public event Action<string> OnAnimationFinished;
        public event Action<string> OnFrameChanged;
        
        private string _currentAnimationName;
        private int _frameIndex;
        private double _timer;
        private SpriteFrames.Animation _currentAnimData;

        public AnimatedSprite(string name, SpriteFrames frames = null) : base(name, null)
        {
            Frames = frames;
            if (Frames != null)
            {
                Texture = Frames.Texture;
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
                Console.WriteLine($"Animation not found: {name}");
                return;
            }

            _currentAnimationName = name;
            _currentAnimData = Frames.GetAnimation(name);
            SpeedScale = customSpeed;
            Playing = true;
            
            if (Texture != Frames.Texture) Texture = Frames.Texture;

            if (fromEnd)
            {
                _frameIndex = _currentAnimData.Frames.Count - 1;
            }
            else if (_currentAnimationName != name || _frameIndex >= _currentAnimData.Frames.Count)
            {
                // 如果切换了动画，重置索引
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
}