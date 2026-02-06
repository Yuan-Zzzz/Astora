using System.Collections.Generic;
using Astora.Core.Nodes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Astora.Core.Inputs;

/// <summary>
/// Input system for handling keyboard and mouse input.
/// 
/// Mouse coordinate systems:
/// - RawMousePosition: Window coordinates (physical pixels)
/// - MouseScreenPosition: Design resolution coordinates (after ScaleMatrix transform)
/// - MouseWorldPosition: World coordinates (after Camera ViewMatrix transform)
/// </summary>
public static class Input
{
    // Keyboard State
    private static KeyboardState _currentKey;
    private static KeyboardState _previousKey;
    
    // Mouse State
    private static MouseState _currentMouse;
    private static MouseState _previousMouse;

    /// <summary>
    /// Tick Update Input States
    /// </summary>
    public static void Update()
    {
        _previousKey = _currentKey;
        _currentKey = Keyboard.GetState();
        
        _previousMouse = _currentMouse;
        _currentMouse = Mouse.GetState();
    }
    
    #region Keyboard methods
    
    /// <summary>
    /// Check if a key is currently down
    /// </summary>
    public static bool IsKeyDown(Keys key)
    {
        return _currentKey.IsKeyDown(key);
    }
    
    /// <summary>
    /// Check if a key was just pressed this frame
    /// </summary>
    public static bool IsKeyPressed(Keys key)
    {
        return _currentKey.IsKeyDown(key) && !_previousKey.IsKeyDown(key);
    }

    /// <summary>
    /// Check if a key was just released this frame
    /// </summary>
    public static bool IsKeyReleased(Keys key)
    {
        return !_currentKey.IsKeyDown(key) && _previousKey.IsKeyDown(key);
    }

    /// <summary>
    /// Enumerate keys that were pressed this frame (for UI key routing).
    /// </summary>
    public static IEnumerable<Keys> GetKeysPressedThisFrame()
    {
        foreach (Keys key in System.Enum.GetValues(typeof(Keys)))
        {
            if (key == Keys.None) continue;
            if (_currentKey.IsKeyDown(key) && !_previousKey.IsKeyDown(key))
                yield return key;
        }
    }

    /// <summary>
    /// Enumerate keys that were released this frame (for UI key routing).
    /// </summary>
    public static IEnumerable<Keys> GetKeysReleasedThisFrame()
    {
        foreach (Keys key in System.Enum.GetValues(typeof(Keys)))
        {
            if (key == Keys.None) continue;
            if (!_currentKey.IsKeyDown(key) && _previousKey.IsKeyDown(key))
                yield return key;
        }
    }

    /// <summary>
    /// Get axis value based on negative and positive keys
    /// </summary>
    public static int GetAxis(Keys negative, Keys positive)
    {
        int val = 0;
        if (IsKeyDown(positive)) val += 1;
        if (IsKeyDown(negative)) val -= 1;
        return val;
    }
    
    #endregion
    
    #region Mouse properties and methods
    
    /// <summary>
    /// Raw mouse position in window coordinates (physical pixels).
    /// This is the unprocessed position from the OS.
    /// </summary>
    public static Vector2 RawMousePosition => new Vector2(_currentMouse.X, _currentMouse.Y);
    
    /// <summary>
    /// Mouse position in design resolution coordinates.
    /// Use this for UI interactions and screen-space calculations.
    /// Applies inverse ScaleMatrix to convert from window to design resolution.
    /// </summary>
    public static Vector2 MouseScreenPosition
    {
        get
        {
            var scaleMatrix = Engine.GetScaleMatrix();
            var invScaleMatrix = Matrix.Invert(scaleMatrix);
            return Vector2.Transform(RawMousePosition, invScaleMatrix);
        }
    }
    
    /// <summary>
    /// Get mouse position in world coordinates using the specified camera.
    /// Use this for game world interactions (e.g., clicking on game objects).
    /// </summary>
    public static Vector2 GetMouseWorldPosition(Camera2D camera)
    {
        if (camera == null)
            return MouseScreenPosition;
        
        return camera.ScreenToWorld(MouseScreenPosition);
    }
    
    /// <summary>
    /// Mouse position in world coordinates using the current active camera.
    /// Returns screen position if no active camera is available.
    /// Use this for game world interactions (e.g., clicking on game objects).
    /// </summary>
    public static Vector2 MouseWorldPosition
    {
        get
        {
            var activeCamera = Engine.CurrentScene?.ActiveCamera;
            return GetMouseWorldPosition(activeCamera);
        }
    }
    
    /// <summary>
    /// [Deprecated] Use MouseScreenPosition instead.
    /// Mouse position in design resolution coordinates.
    /// </summary>
    public static Vector2 MousePosition => MouseScreenPosition;
    
    public static bool IsMouseButtonDown(ButtonState buttonState)
    {
        return buttonState == ButtonState.Pressed;
    }
    
    public static bool IsLeftMouseButtonDown()
    {
        return _currentMouse.LeftButton == ButtonState.Pressed;
    }
    
    public static bool IsRightMouseButtonDown()
    {
        return _currentMouse.RightButton == ButtonState.Pressed;
    }
    
    public static bool IsMiddleMouseButtonDown()
    {
        return _currentMouse.MiddleButton == ButtonState.Pressed;
    }
    
    public static bool IsLeftMouseButtonPressed()
    {
        return _currentMouse.LeftButton == ButtonState.Pressed && 
               _previousMouse.LeftButton == ButtonState.Released;
    }
    
    public static bool IsRightMouseButtonPressed()
    {
        return _currentMouse.RightButton == ButtonState.Pressed && 
               _previousMouse.RightButton == ButtonState.Released;
    }
    
    public static bool IsMiddleMouseButtonPressed()
    {
        return _currentMouse.MiddleButton == ButtonState.Pressed && 
               _previousMouse.MiddleButton == ButtonState.Released;
    }

    /// <summary>
    /// True when left button was released this frame (was pressed, now released).
    /// </summary>
    public static bool IsLeftMouseButtonReleased()
    {
        return _currentMouse.LeftButton == ButtonState.Released &&
               _previousMouse.LeftButton == ButtonState.Pressed;
    }

    /// <summary>
    /// True when right button was released this frame.
    /// </summary>
    public static bool IsRightMouseButtonReleased()
    {
        return _currentMouse.RightButton == ButtonState.Released &&
               _previousMouse.RightButton == ButtonState.Pressed;
    }
    
    public static int MouseScrollDelta => _currentMouse.ScrollWheelValue - _previousMouse.ScrollWheelValue;
    #endregion
}
