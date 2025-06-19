using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpatialEngine
{
    public static class Settings
    {
        

        public static class RendererSettings
        {
            /// <summary>
            /// <para>Optimizes updating the rendersets buffers</para>
            /// 0 = Updates every frame (halfs fps compared to the optimized version)<br />
            /// 1 = Updates every second or when a new object is added<br />
            /// 2 = Updates when a new object is added (aggressive optimization, but only updates on new object)
            /// </summary>
            public static int OptimizeUpdatingBuffers = 1;
            /// <summary>
            /// 
            /// </summary>
            public static bool UseMultiDraw = true;
        }
    }
}
