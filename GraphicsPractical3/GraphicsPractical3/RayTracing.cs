﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Ray = GraphicsPractical3.Geometry.Ray;
using Primitive = GraphicsPractical3.Geometry.Primitive;
using PointLight = GraphicsPractical3.Geometry.PointLight;
using BoundingBox = GraphicsPractical3.Geometry.BoundingBox;

namespace GraphicsPractical3.RayTracing
{
    public class Screen
    {
        private int width;
        private int height;
        private float pixelSize;

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        public float PixelSize
        {
            get { return pixelSize; }
        }
            
        public float RealWidth
        {
            get { return (float)width * pixelSize; }
        }

        public float RealHeight
        {
            get { return (float)height * pixelSize; }
        }

        public Screen(int width, int height, float pixelSize)
        {
            this.width = width;
            this.height = height;
            this.pixelSize = pixelSize;
        }
    }

    public class Eye
    {
        public Vector3 Position;
        public Vector3 Direction;
        public Vector3 Up;
        public Vector3 Right;
        public float DistanceToScreen;

        public Eye(Vector3 position, Vector3 direction, float distance)
        {
            DistanceToScreen = distance;
            Position = position;
            Direction = -1 * Vector3.Normalize(direction);
            Up = new Vector3(0, 1, 0);
            Right = Vector3.Normalize(Vector3.Cross(Up, Direction));
            Up = Vector3.Cross(Right, Direction);
        }
        public void UpdateDirection(float rotate)
        {
            Matrix matrix = Matrix.CreateRotationY(rotate);

            Position = Vector3.Transform(Position, matrix);
            Direction = Vector3.Transform(Direction, matrix);
            Up = Vector3.Transform(Up, matrix);
        }
    }

    public class Engine
    {
        private Primitive[] primitives;
        private PointLight[] pointLights;

        private BVHTree bvh;

        public Engine(Primitive[] p, PointLight[] pL, bool regenerateBvhTree = false)
        {
            primitives = p;
            pointLights = pL;

            if (regenerateBvhTree || (!regenerateBvhTree && !File.Exists("main.bvh")))
            {
                bvh = new BVHTree(primitives);
                bvh.SaveToFile("main.bvh"); // Also save to file
            }
            else
                bvh = new BVHTree(primitives, "main.bvh"); // Simply load from file
        }

        public Color[] Update(Eye e, Screen s, Primitive[] p = null, PointLight[] pL = null)
        {
            if (p != null)
            {
                primitives = p;
            }
            if (pL != null)
            {
                pointLights = pL;
            }
            Color[] result = new Color[s.Height * s.Width];
            Vector3 o = e.Position + ( -1 * e.Direction ) * e.DistanceToScreen;
            o = o - e.Right * (0.5f * s.RealWidth);
            o = o + e.Up * (0.5f * s.RealHeight);
            Vector3 xTrans = 1 * e.Right * s.PixelSize;
            Vector3 yTrans = -1 * e.Up * s.PixelSize;
            for (uint i = 0; i < s.Width; i++)
            {
                for (uint j = 0; j < s.Height; j++)
                {
                    Vector3 direction = new Vector3();
                    Vector3 origin = new Vector3();
                    origin = o + (i * xTrans) + (j * yTrans);
                    direction = origin - e.Position;
                    direction = Vector3.Normalize(direction);
                    Ray ray = new Ray(direction, origin);
                    Vector3 colour = tracer(ray);
                    result[j * s.Width + i] = new Color(colour);
                }
            }
            return result;
        }

        private Vector3 tracer(Ray r)
        {
            Primitive h;
            if ((h = hit(r)) != null)
            {
                if (h.Material.Glass)
                    return (0.1f * tracer(Reflection(r, h))) + (0.9f * tracer(Refraction(r, h, 9, 10)));

                if (h.Material.Reflective)
                    return (0.2f * tracer(Reflection(r, h))) + (0.8f * comineColorLight(h.Material.Color, DirectIllumination(r, h)));

                return comineColorLight(h.Material.Color, DirectIllumination(r, h));
            }
            else
                // No hit, ligth blue background
                return new Vector3 (0.9f, 0.9f, 1f);
        }

        private Vector3 comineColorLight(Vector3 color, Vector3 light)
        {
            float x = color.X * light.X;
            float y = color.Y * light.Y;
            float z = color.Z * light.Z;

            return new Vector3(x, y, z);
        }

        private Primitive hit(Ray r, Primitive o = null, float d = float.MaxValue)
        {
            return bvh.TryHit(r, o, d);
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
                float dist = l.Length();
                Vector3 direction = Vector3.Normalize(l);
                Ray nR = new Ray(direction, origin);
                Primitive h = hit(nR, p, dist);
                if (h == null)
                {
                    Vector3 normal = p.Normal(r);
                    float attenuation = 1.0f / (dist * dist);
                    float dot = Vector3.Dot(normal, l);
                    if (dot < 0)
                    {
                        dot = 0;
                    }
                    result += (pL.Color * attenuation * dot);
                }
            }
            return result;
        }
    }
}
