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
        public abstract Vector3 Normal(Ray r);
        public abstract Vector3 Hit(Ray r);
        public abstract float HitDistance(Ray r);
    }

    public class Sphere : Primitive
    {
        public Color Color;
        public Material Material;
        public Vector3 Center;
        public float Radius;
        public Sphere(Vector3 Center, float Radius)
        {
            this.Center = Center;
            this.Radius = Radius;
        }

        public override float HitDistance(Ray r)
        {
            float t = 0.0f;

            Vector3 m = r.Origin - Center;
            float b = Vector3.Dot(m, r.Direction);
            float c = Vector3.Dot(m, m) - Radius * Radius;

            if (c > 0.0f && b > 0.0f)
            {
                return 0.0f;
            }

            float discr = b * b - c;

            if (discr < 0.0f)
            {
                return 0.0f;
            }

            t = -b - (float)Math.Sqrt(discr);

            if (t < 0.0f)
            {
                t = 0.0f;
            }
            return t;
        }

        public override Vector3 Hit(Ray r)
        {
            return r.Origin + this.HitDistance(r) * r.Direction;
        }

        public override Vector3 Normal(Ray r)
        {
            Vector3 i = Hit(r);
            Vector3 a = i - Center;

            return Vector3.Normalize(a);
        }
    }
    public class Triangle : Primitive
    {
        public Material Material;
        public Color Color;
        public Vector3 A;
        public Vector3 B;
        public Vector3 C;

        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            this.A = a;
            this.B = b;
            this.C = c;
        }

        //public override float HitDistance(Ray r) // See at pages 78-79 Fundamentals of Computer Graphics 3rd Edition
        //{
        //    float a = A.X - B.X;
        //    float b = A.Y - B.Y;
        //    float c = A.Z - B.Z;
        //    float d = A.X - C.X;
        //    float e = A.Y - C.Y;
        //    float f = A.Z - C.Z;
        //    float g = r.Direction.X;
        //    float h = r.Direction.Y;
        //    float i = r.Direction.Z;
        //    float j = A.X - r.Origin.X;
        //    float k = A.Y - r.Origin.Y;
        //    float l = A.Z - r.Origin.Z;

        //    float ei_hf = e * i - h * f;
        //    float gf_di = g * f - d * i;
        //    float dh_eg = d * h - e * g;
        //    float ak_jb = a * k - j * b;
        //    float jc_al = j * c - a * l;
        //    float bl_kc = b * l - k * c;

        //    float M = a * ei_hf + b * gf_di + c * dh_eg;
        //    float t = -1 * (f * ak_jb + e * jc_al + d * bl_kc) / M;
        //    if (t < 0)
        //    {
        //        return 0.0f;
        //    }

        //    float gamma = (i * ak_jb + h * jc_al + g * bl_kc) / M;
        //    if (gamma < 0 || gamma > 1)
        //    {
        //        return 0.0f;
        //    }

        //    float beta = (j * ei_hf + k * gf_di + l * dh_eg) / M;
        //    if (beta < 0 || beta > 1 - gamma)
        //    {
        //        return 0.0f;
        //    }

        //    if (beta > 0 && gamma > 0 && beta + gamma < 1)
        //    {
        //        return t;
        //    }

        //    return 0.0f;
        //}

        public override float HitDistance(Ray r) // http://people.cs.clemson.edu/~dhouse/courses/405/papers/raytriangle-moeller02.pdf
        {
            const float epsilon = 10e-5f;

            Vector3 e1 = B - A;
            Vector3 e2 = C - A;
            Vector3 p = Vector3.Cross(r.Direction, e2);
            float a = Vector3.Dot(e1, p);

            if (a > -1 * epsilon && a < epsilon) 
                return 0.0f;

            Vector3 s = r.Origin - A;
            float f = 1 / a;
            float u = f * (Vector3.Dot(s, p));

            if (u < 0.0f || u > 1.0f)
                return 0.0f;

            Vector3 q = Vector3.Cross(s, e1);
            float v = f * (Vector3.Dot(r.Direction, q));

            if (v < 0.0f || u + v > 1.0f)
                return 0.0f;

            float t = f * (Vector3.Dot(e2, q));

            return t;

        }

        public override Vector3 Hit(Ray r)
        {
            return r.Origin + this.HitDistance(r) * r.Direction;
        }

        public override Vector3 Normal(Ray r)
        {
            Vector3 a = Vector3.Cross(B - A, C - A);

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

    public struct PointLight
    {
        public Vector3 Point;
        public Vector3 Color;
    }
}