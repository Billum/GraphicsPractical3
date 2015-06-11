using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Geometry
{
    class Sphere
    {
        public Vector3 Center;
        public float Radius;
        public Sphere(Vector3 center, float radius)
        {
            this.Center = center;
            this.Radius = radius;
        }

        public Vector3 Hit(Ray r)
        {
            float t = 0;
            Vector3 q = new Vector3();

            Vector3 m = r.Origin - Center;
            float b = Vector3.Dot(m, r.Direction);
            float c = Vector3.Dot(m, m) - Radius * Radius;

            if (c > 0.0f && b > 0.0f)
            {
                t = 0.0f;
            }

            float discr = b * b - c;

            if (discr < 0.0f)
            {
                t = 0.0f;
            }

            t = -b - (float)Math.Sqrt(discr);

            if (t < 0.0f)
            {
                t = 0.0f;
            }

            q = r.Origin + r.Direction * t;
            return q;
        }
    }

    class Ray
    {
        public Vector3 Direction;
        public Vector3 Origin;
        public Ray(Vector3 direction, Vector3 origin)
        {
            this.Direction = direction;
            this.Origin = origin;
        }
    }

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
