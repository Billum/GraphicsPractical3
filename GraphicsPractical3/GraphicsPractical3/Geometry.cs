using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GraphicsPractical3.Geometry
{
    public abstract class Primitive
    {
        public Material Material;
        public Color Color;
        public abstract Vector3 Normal;
        public abstract Vector3 Hit;
    }
    public class Sphere : Primitive
    {
        public Color Color;
        public Material Material;
        public Vector3 Center;
        public float Radius;
        public Sphere(Vector3 center, float radius)
        {
            this.Center = center;
            this.Radius = radius;
        }

        public Vector3 Hit(Ray r)
        {
            float t = 0.0f;
            Vector3 q = new Vector3();

            Vector3 m = r.Origin - Center;
            float b = Vector3.Dot(m, r.Direction);
            float c = Vector3.Dot(m, m) - Radius * Radius;

            if (c > 0.0f && b > 0.0f)
            {
                return q * 0.0f;
            }

            float discr = b * b - c;

            if (discr < 0.0f)
            {
                return q * 0.0f;
            }

            t = -b - (float)Math.Sqrt(discr);

            if (t < 0.0f)
            {
                t = 0.0f;
            }

            q = r.Origin + t * r.Direction;
            return q;
        }
        public Vector3 Normal(Ray r)
        {
            Vector3 i = Hit(r);
            Vector3 a = i - Center;

            return Vector3.Normalize(a);
        }
    }
    public struct Ray
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