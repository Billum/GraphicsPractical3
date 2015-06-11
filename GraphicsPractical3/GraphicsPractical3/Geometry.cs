using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GraphicsPractical3.Geometry
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

    
}
