using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GraphicsPractical3
{
    class Screen
    {
        public class Screen
        {
            public int Width;
            public int Height;
        }

        public class Engine
        {
            public void Update(Vector3 e, Screen s)
        {
            for (uint i = 0; i < s.Width; i++)
            {
                for (uint j = 0; j < s.Height; j++)
                {
                    // perspective view ray construction
                    Vector3 d = new Vector3(s.Width/2, 0 /*?z=?*/, s.Height/2) - e;
                    var r = new Ray(-d *  , e);
                }
            }
        }
        }
    }
}
