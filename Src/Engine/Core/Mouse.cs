
using System.Numerics;
using Silk.NET.Input;

namespace SpatialEngine
{
    public static class Mouse
    {
        public static IMouse mouse;
        public static Vector2 position;
        public static Vector2 lastPosition;
        public static bool uiWantMouse;

        public static void Init()
        {
            mouse = Input.input.Mice[0];
        }
        
        public static void Update()
        {
            lastPosition = position;
            position = mouse.Position;
        }
    }
}