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
            public float PixelSize;
            
            public float RealWidth
            {
                get { return (float)Width * PixelSize; }
            }

            public float RealHeight
            {
                get { return (float)Height * PixelSize; }
            }
        }

        public class Eye
        {
            public Vector3 Position;
            public Vector3 Direction;
            public Vector3 Up;
            public Vector3 Left;
            public float DistanceToScreen;
        }

        public class Engine
        {
            public void Update(Eye e, Screen s)
            {
                Vector3 o = e.Position + e.Direction * e.DistanceToScreen;
                o = o + e.Left * (0.5f * s.RealWidth);
                o = o + e.Up * (0.5f * s.RealHeight);
                Vector3 xTrans = -1 * e.Left * s.PixelSize;
                Vector3 yTrans = -1 * e.Up * s.PixelSize;
                for (uint i = 0; i < s.Width; i++)
                {
                    for (uint j = 0; j < s.Height; j++)
                    {
                        Vector3 direction = new Vector3();
                        Vector3 origin = new Vector3();
                        origin = o + (i * xTrans) + (j * yTrans);
                        direction = e.Position - origin;
                        direction = Vector3.Normalize(direction);
                    }
                }
            }
        }
    }
}
