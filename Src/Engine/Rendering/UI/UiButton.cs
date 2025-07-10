using Silk.NET.OpenGL;
using Silk.NET.SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Input;


namespace SpatialEngine.Rendering
{
    public class UiButton
    {
        public Vector2 position;
        public Vector2 size;
        Action onClick;

        public UiButton(Vector2 position, Vector2 size, Action onClick)
        {
            this.position = position;
            this.size = size;
            this.onClick = onClick;
            Mouse.mouse.MouseDown += RunOnClick;
        }
        
        public void Update()
        {
            Mouse.uiWantMouse = false;
            if (!BoundsCheck(Mouse.mouse.Position))
                return;

            Mouse.uiWantMouse = true;
        }

        void RunOnClick(IMouse mouse, MouseButton button)
        {
            if (Mouse.uiWantMouse && button == MouseButton.Left)
            {
                onClick.Invoke();
            }
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool BoundsCheck(Vector2 pos)
        {
            if (pos.X < position.X - size.X || pos.X > position.X + size.X || pos.Y < position.Y - size.Y || pos.Y > position.Y + size.Y)
                return false;
            return true;
        }
    }
}