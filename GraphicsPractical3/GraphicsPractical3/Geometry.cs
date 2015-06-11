﻿using System;
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

        public override Vector3 Hit(Ray r)
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
        public override Vector3 Hit(Ray r) // See at pages 78-79 Fundamentals of Computer Graphics 3rd Edition
        {
            Vector3 q = new Vector3();

            float a = A.X - B.X;
            float b = A.Y - B.Y;
            float c = A.Z - B.Z;
            float d = A.X - C.X;
            float e = A.Y - C.Y;
            float f = A.Z - C.Z;
            float g = r.Direction.X;
            float h = r.Direction.Y;
            float i = r.Direction.Z;
            float j = A.X - r.Origin.X;
            float k = A.Y - r.Origin.Y;
            float l = A.Z - r.Origin.Z;

            float ei_hf = e * i - h * f;
            float gf_di = g * f - d * i;
            float dh_eg = d * h - e * g;
            float ak_jb = a * k - j * b;
            float jc_al = j * c - a * l;
            float bl_kc = b * l - k * c;

            float M = a * ei_hf + b * gf_di + c * dh_eg;
            
            float gamma = (i * ak_jb + h * jc_al + g * bl_kc) / M;
            if (gamma < 0 || gamma > 1)
            {
                return q * 0.0f;
            }

            float beta = (j * ei_hf + k * gf_di + l * dh_eg) / M;
            if (beta < 0 || beta > 1 - gamma)
            {
                return q * 0.0f;
            }

            float t = -1 * (f * ak_jb + e * jc_al + d * bl_kc) / M;
            if (beta > 0 && gamma > 0 && beta + gamma < 1) 
                q = r.Origin + t * r.Direction;

            return q;
        }
        public override Vector3 Normal(Ray r)
        {
            Vector3 a = Vector3.Cross(B - A, C - A);

            return a;
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