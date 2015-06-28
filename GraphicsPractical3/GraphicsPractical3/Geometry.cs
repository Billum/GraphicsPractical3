using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GraphicsPractical3.Geometry
{
    public class Model
    {
        public static Model LoadFromSinglePrimitive(Primitive p)
        {
            Model m = new Model();
            m.Primitives = new Primitive[1];
            m.Primitives[0] = p;
            return m;
        }

        public Primitive[] Primitives;
    }

    public class BoundingBox
    {
        public Vector3 MinCorner;
        public Vector3 MaxCorner;

        public BoundingBox(Vector3 minCorner, Vector3 maxCorner)
        {
            MinCorner = minCorner;
            MaxCorner = maxCorner;
        }

        public float Intersect(Ray r)
        {
            var inverseDirection = new Vector3 (
                    1.0f / r.Direction.X,
                    1.0f / r.Direction.Y,
                    1.0f / r.Direction.Z
                );

            float txmin = (MinCorner.X - r.Origin.X) * inverseDirection.X,
                  txmax = (MaxCorner.X - r.Origin.X) * inverseDirection.X,
                  tymin = (MinCorner.Y - r.Origin.Y) * inverseDirection.Y,
                  tymax = (MaxCorner.Y - r.Origin.Y) * inverseDirection.Y,
                  tzmin = (MinCorner.Z - r.Origin.Z) * inverseDirection.Z,
                  tzmax = (MaxCorner.Z - r.Origin.Z) * inverseDirection.Z;

            float tMin = Math.Max(
                            Math.Max(
                                Math.Min(txmin, txmax),
                                Math.Min(tymin, tymax)
                            ),
                            Math.Min(tzmin, tzmax)
                         );

            float tMax = Math.Min(
                            Math.Min(
                                Math.Max(txmin, txmax),
                                Math.Max(tymin, tymax)
                            ),
                            Math.Max(tzmin, tzmax)
                        );

            if ((tMax > 0) && (tMin <= tMax))
                return tMin;
            else
                return 0.0f;
        }

        public float SurfaceArea()
        {
            var side1 = (MaxCorner.X - MinCorner.X) * (MaxCorner.Y - MinCorner.Y);
            var side2 = (MaxCorner.Y - MinCorner.Y) * (MaxCorner.Z - MinCorner.Z);
            var side3 = (MaxCorner.Z - MinCorner.Z) * (MaxCorner.X - MinCorner.X);

            return 2 * (side1 + side2 + side3);
        }
    }

    public abstract class Primitive
    {
        public Material Material;
        public abstract Vector3 Normal(Ray r);
        public abstract Vector3 Hit(Ray r);
        public abstract float HitDistance(Ray r);
        public abstract Vector3 Center();
        public abstract BoundingBox BoundingBox();
    }

    public class Sphere : Primitive
    {
        public Vector3 Centre;
        public float Radius;
        public Sphere(Vector3 Center, float Radius)
        {
            this.Centre = Center;
            this.Radius = Radius;
        }

        public override Vector3 Center()
        {
            return Centre;
        }

        public override BoundingBox BoundingBox()
        {
            var minCorner = new Vector3(Centre.X - Radius, Centre.Y - Radius, Centre.Z - Radius);
            var maxCorner = new Vector3(Centre.X + Radius, Centre.Y + Radius, Centre.Z + Radius);

            return new BoundingBox(minCorner, maxCorner);
        }

        public override float HitDistance(Ray r)
        {
            float t = 0.0f;

            Vector3 m = r.Origin - Centre;
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

            t = Math.Min(-b - (float)Math.Sqrt(discr), -b + (float)Math.Sqrt(discr));

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
            Vector3 a = i - Centre;

            return Vector3.Normalize(a);
        }
    }

    public class Triangle : Primitive
    {
        public Vector3 A;
        public Vector3 B;
        public Vector3 C;

        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            this.A = a;
            this.B = b;
            this.C = c;
        }

        public override BoundingBox BoundingBox()
        {
            float minX = Math.Min(Math.Min(A.X, B.X), C.X);
            float minY = Math.Min(Math.Min(A.Y, B.Y), C.Y);
            float minZ = Math.Min(Math.Min(A.Z, B.Z), C.Z);

            float maxX = Math.Max(Math.Max(A.X, B.X), C.X);
            float maxY = Math.Max(Math.Max(A.Y, B.Y), C.Y);
            float maxZ = Math.Max(Math.Max(A.Z, B.Z), C.Z);

            return new BoundingBox(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
        }

        public override Vector3 Center()
        {
            var c = new Vector3();
            c.X = (A.X + B.X + C.X) / 3;
            c.Y = (A.Y + B.Y + C.Y) / 3;
            c.Z = (A.Z + B.Z + C.Z) / 3;

            return c;
        }

        public override float HitDistance(Ray r) // See at pages 78-79 Fundamentals of Computer Graphics 3rd Edition
        {
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
            float t = -1 * (f * ak_jb + e * jc_al + d * bl_kc) / M;
            if (t < 0)
            {
                return 0.0f;
            }

            float gamma = (i * ak_jb + h * jc_al + g * bl_kc) / M;
            if (gamma < 0 || gamma > 1)
            {
                return 0.0f;
            }

            float beta = (j * ei_hf + k * gf_di + l * dh_eg) / M;
            if (beta < 0 || beta > 1 - gamma)
            {
                return 0.0f;
            }

            if (beta > 0 && gamma > 0 && beta + gamma < 1)
            {
                return t;
            }

            return 0.0f;
        }

        public override Vector3 Hit(Ray r)
        {
            return r.Origin + this.HitDistance(r) * r.Direction;
        }

        public override Vector3 Normal(Ray r)
        {
            Vector3 a = Vector3.Cross(B - A, C - A);

            float t = Vector3.Dot(a, r.Direction);

            return (t > 0.0f) ? -1 * Vector3.Normalize(a) : Vector3.Normalize(a);
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

        public PointLight(Vector3 point, Vector3 color)
        {
            Point = point;
            Color = color;
        }
    }
}