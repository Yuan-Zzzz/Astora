using Microsoft.Xna.Framework.Input;

namespace Astora.Core.Inputs
{
    public static class Input
    {
        private static KeyboardState _currentKey;
        private static KeyboardState _previousKey;

        public static void Update()
        {
            _previousKey = _currentKey;
            _currentKey = Keyboard.GetState();
        }
        
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
    }
}