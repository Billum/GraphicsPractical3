using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Ray = GraphicsPractical3.Geometry.Ray;
using Primitive = GraphicsPractical3.Geometry.Primitive;
using PointLight = GraphicsPractical3.Geometry.PointLight;


namespace GraphicsPractical3.RayTracing
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
        private Primitive[] primitives;
        private PointLight[] pointLights;

        public Engine(Primitive[] p, PointLight[] pL)
        {
            primitives = p;
            pointLights = pL;
        }

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
                    Ray ray = new Ray(direction, origin);
                    Vector3 colour = tracer(ray);
                }
            }
        }

        private Vector3 tracer(Ray r)
        {
            hitOutput h = hit(r);
            if (h.P == null)
            {
                return new Vector3 ( 0.0f, 0.0f, 0.0f );
            }
            return Matt(r, h.P);
        }

        private hitOutput hit(Ray r)
        {
            hitOutput h = new hitOutput();
            float shortest = 0.0f;
            Primitive thing = null;
            foreach (Primitive p in primitives)
            {
                float pHit = p.HitDistance(r);
                if (pHit != 0.0f)
                {
                    if (pHit < shortest)
                    {
                        shortest = pHit;
                        thing = p;
                    }
                }
            }
            h.P = thing;
            if (h.P != null)
            {
                h.R = Reflection(r, h.P);
            }
            return h;
        }

        private struct hitOutput
        {
            public Ray R;
            public Primitive P;
        }


        public Ray Reflection(Ray r, Primitive p)
        {
            Vector3 origin = p.Hit(r);
            Vector3 I = r.Direction;
            Vector3 N = p.Normal(r);
            float cos_i = -1 * Vector3.Dot(I, N);

            Vector3 direction = I - 2 * cos_i * N;

            return new Ray(direction, origin);
        }

        public Ray Refraction(Ray r, Primitive p, float n1, float n2)
        {
            float refrac = n1 / n2;
            Vector3 origin = p.Hit(r);
            Vector3 I = r.Direction;
            Vector3 N = p.Normal(r);

            float cos_i = Vector3.Dot(I, N) / (N.Length() * I.Length());
            Vector3 T = n1 / n2 * (I + cos_i * N);
            float U = (float)Math.Sqrt(1 - T.Length() * T.Length());

            Vector3 direction = refrac * I + (refrac * cos_i - U) * N;

            return new Ray(direction, origin);
        }

        public Vector3 DirectIllumination(Ray r, Primitive p)
        {
            Vector3 origin = p.Hit(r);
            float nOfLights = (float)pointLights.Length;
            Vector3 result = new Vector3(0.0f, 0.0f, 0.0f);
            foreach (PointLight pL in pointLights)
            {
                Vector3 l = pL.Point - origin;
                Vector3 direction = Vector3.Normalize(l);
                Ray nR = new Ray(direction, origin);
                hitOutput h = hit(nR);
                if (h.P == null)
                {
                    Vector3 normal = p.Normal(r);
                    float dist = l.Length();
                    float attenuation = 1.0f / (dist * dist);
                    result = result + pL.Color * attenuation * Vector3.Dot(normal, l);
                }
            }
            return result;
        }
    }
}
