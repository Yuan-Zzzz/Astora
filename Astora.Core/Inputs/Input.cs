using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Astora.Core.Inputs
{
    public static class Input
    {
        // 键盘状态
        private static KeyboardState _currentKey;
        private static KeyboardState _previousKey;
        
        // 鼠标状态
        private static MouseState _currentMouse;
        private static MouseState _previousMouse;

        public static void Update()
        {
            _previousKey = _currentKey;
            _currentKey = Keyboard.GetState();
            
            _previousMouse = _currentMouse;
            _currentMouse = Mouse.GetState();
        }
        
        // 键盘方法
        public static bool IsKeyDown(Keys key)
        {
            return _currentKey.IsKeyDown(key);
        }
        
        public static bool IsKeyPressed(Keys key)
        {
            return _currentKey.IsKeyDown(key) && !_previousKey.IsKeyDown(key);
        }

        public static int GetAxis(Keys negative, Keys positive)
        {
            int val = 0;
            if (IsKeyDown(positive)) val += 1;
            if (IsKeyDown(negative)) val -= 1;
            return val;
        }
        
        // 鼠标方法
        public static Vector2 MousePosition => new Vector2(_currentMouse.X, _currentMouse.Y);
        
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
        
        public static int MouseScrollDelta => _currentMouse.ScrollWheelValue - _previousMouse.ScrollWheelValue;
    }
}