using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpatialEngine.Terrain
{
    public class Terrain
    {
        int id;

        public Terrain()
        {



            id = Globals.scene.SpatialObjects.Count;
            //Globals.scene.AddSpatialObject(new Rendering.Mesh());
        }
    }
}
