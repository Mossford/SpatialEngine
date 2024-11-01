using Silk.NET.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpatialEngine
{
    public static class Input
    {
        public static Dictionary<int, int> keysPressed;
        public static IInputContext input;
        public static IKeyboard keyboard;

        public static void Init()
        {
            input = Globals.window.CreateInput();
            keyboard = input.Keyboards.FirstOrDefault();
            keysPressed = new Dictionary<int, int>();
        }

        public static void Update()
        {
            for (int i = 0; i < keyboard.SupportedKeys.Count; i++)
            {
                if (keyboard.IsKeyPressed(keyboard.SupportedKeys[i]))
                {
                    if(!keysPressed.ContainsKey((int)keyboard.SupportedKeys[i]))
                    {
                        keysPressed.Add((int)keyboard.SupportedKeys[i], 1);
                    }
                    else
                    {
                        keysPressed[(int)keyboard.SupportedKeys[i]] = 1;
                    }
                }
                else
                {
                    if (!keysPressed.ContainsKey((int)keyboard.SupportedKeys[i]))
                    {
                        keysPressed.Add((int)keyboard.SupportedKeys[i], 0);
                    }
                    else
                    {
                        keysPressed[(int)keyboard.SupportedKeys[i]] = 0;
                    }
                }
            }
        }

        public static bool IsKeyDown(Key key)
        {
            if (keysPressed.ContainsKey((int)key))
            {
                if (keysPressed[(int)key] == 1)
                    return true;
            }
            return false;
        }

        public static bool IsKeyUp(Key key)
        {
            if (keysPressed.ContainsKey((int)key))
            {
                if (keysPressed[(int)key] == 0)
                    return true;
            }
            return false;
        }

        public static void Clear()
        {
            keysPressed.Clear();
        }
    }
}
