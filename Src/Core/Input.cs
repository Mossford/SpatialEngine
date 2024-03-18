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
        public static List<int> keysPressed;
        public static IInputContext input;
        public static IKeyboard keyboard;

        public static void Init()
        {
            input = Globals.window.CreateInput();
            keyboard = input.Keyboards.FirstOrDefault();
            keysPressed = new List<int>();
        }

        public static void Update()
        {
            for (int i = 0; i < keyboard.SupportedKeys.Count; i++)
            {
                if (keyboard.IsKeyPressed(keyboard.SupportedKeys[i]))
                    keysPressed.Add((int)keyboard.SupportedKeys[i]);
            }
        }

        public static void Clear()
        {
            keysPressed.Clear();
        }
    }
}
