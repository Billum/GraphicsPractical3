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
        public Vector3 Hit(Ray ray, Sphere sphere)
        {
            Vector3 v = ray.Origin - sphere.Center;
            float a = Vector3.Dot(v, ray.Direction);
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
