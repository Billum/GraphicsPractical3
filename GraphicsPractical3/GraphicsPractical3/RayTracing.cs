using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    public class BVHTree
    {
        public class Node
        {
            public int Start;
            public int End;
            public BoundingBox BoundingBox;
            public Node LeftChild;
            public Node RightChild;

            public int Num() { return End - Start; }
        }

        public struct Hit
        {
            public float Distance;
            public Primitive Primitive;

            public Hit(float distance, Primitive p)
            {
                Distance = distance;
                Primitive = p;
            }

            public static Hit? BestHit(Hit? h1, Hit? h2)
            {
                if (!h1.HasValue && !h2.HasValue)
                    return null;

                if (!h1.HasValue)
                    return h2;
                else if
                    (!h2.HasValue)
                    return h1;

                if (h1.Value.Distance < h2.Value.Distance)
                    return h1;
                else
                    return h2;
            }
        }

        public BVHTree(Primitive[] primitives)
        {
            this.indices = new int[primitives.Length];
            this.primitives = primitives;
            this.treeRoot = new Node
                {
                    Start = 0,
                    End = primitives.Length,
                    //BoundingBox = BoundingBoxOverPrimitives(primitives)
                };

            Split(treeRoot);
        }

        public Primitive TryHit(Ray r)
        {
            Hit? hit = HitNode(treeRoot, r);
            if (hit.HasValue)
                return hit.Value.Primitive;
            else
                return null; // No hit
        }

        private Hit? HitNode(Node n, Ray r)
        {
            if (n == null)
                return null; // No hit

            if (n.LeftChild == null && n.RightChild == null)
            {
                // We're in a leaf node, find the best hit in the current
                // subset of primitives and return it
                Hit? lowestHit = null;
                for (int i = n.Start; i < n.End; i++)
                {
                    float hitDistance;
                    if ((hitDistance = primitives[i].HitDistance(r)) != 0.0f)
                        lowestHit = Hit.BestHit(lowestHit, new Hit(hitDistance, primitives[i]));
                }

                return lowestHit;
            }
            else
            {
                // Return the best hit from the two subtrees
                return Hit.BestHit(HitNode(n.LeftChild, r), HitNode(n.RightChild, r));
            }
        }

        private int BestSplitIndex(Node n, out float splitCost)
        {
            int numPrimitives = primitives.Count();
            int lowestSplitIndex = 0;
            float lowestSplitCost = float.MaxValue;

            for (int i = n.Start; i < n.End; i++)
            {
                var p = primitives[i];

                // Get center point over which to split
                var mid = p.Center();

                // TODO : Only checks over X-axis
                var left = primitives.Where(cp => cp.Center().X < mid.X);
                var right = primitives.Where(cp => cp.Center().X >= mid.X);

                // Determine SAH split cost
                var leftCost = left.Count() * BoundingBoxOverPrimitives(left).SurfaceArea();
                var rightCost = right.Count() * BoundingBoxOverPrimitives(right).SurfaceArea();

                var cost = leftCost + rightCost;

                if (cost < lowestSplitCost)
                {
                    lowestSplitIndex = i;
                    lowestSplitCost = cost;
                }
            }

            // also out the cost
            splitCost = lowestSplitCost;
            return lowestSplitIndex;
        }

        private void Split(Node n, float previousSplitCost = float.MaxValue /* By default split anyways */)
        {
            if (n.Num() == 1)
                return; // No splitting necessary

            float splitCost = 0f;
            int splitIndex = BestSplitIndex(n, out splitCost);

            if (splitCost > previousSplitCost)
                return; // Terminal condition reached, when the split cost is actually
                        // worst than the split cost of the previous split, we won't
                        // continue

            var splitPoint = primitives[splitIndex].Center();

            var indicesLeft = new List<int>();
            var indicesRight = new List<int>();

            // Split indices in current subset of indices array over two lists,
            // left of the split point and right of the split point
            for (int i = n.Start; i < n.End; i++)
            {
                if (primitives[i].Center().X < splitPoint.X)
                    indicesLeft.Add(i);
                else
                    indicesRight.Add(i);
            }

            // Now rearrange the current subset of the indices array
            // so that the left primitives are first, and then the
            // right primitives
            int c1 = n.Start;
            int c2 = 0;
            foreach (int i in indicesLeft)
            {
                indices[c1] = i;
                c1++;
            }
            c2 = c1;
            foreach (int i in indicesRight)
            {
                indices[c2] = i;
                c2++;
            }
            
            // Register left child with start and end offsets
            n.LeftChild = new Node
                {
                    Start = n.Start,
                    End = c1,
                    BoundingBox = BoundingBoxOverPrimitives(primitives.Skip(n.Start).Take(c1 - n.Start))
                };

            // Register right child with start and end offsets
            n.RightChild = new Node
                {
                    Start = c1,
                    End = c2,
                    BoundingBox = BoundingBoxOverPrimitives(primitives.Skip(c1).Take(c2 - c1))
                };

            // Further split left and right
            Split(n.LeftChild, splitCost);
            Split(n.RightChild, splitCost);
        }

        private BoundingBox BoundingBoxOverPrimitives(IEnumerable<Primitive> primitives)
        {
            Vector3 minCorner = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 maxCorner = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (var p in primitives)
            {
                var bbox = p.BoundingBox();

                minCorner.X = Math.Min(minCorner.X, bbox.MinCorner.X);
                minCorner.Y = Math.Min(minCorner.Y, bbox.MinCorner.Y);
                minCorner.Z = Math.Min(minCorner.Z, bbox.MinCorner.Z);

                maxCorner.X = Math.Max(maxCorner.X, bbox.MaxCorner.X);
                maxCorner.Y = Math.Max(maxCorner.Y, bbox.MaxCorner.Y);
                maxCorner.Z = Math.Max(maxCorner.Z, bbox.MaxCorner.Z);
            }

            return new BoundingBox(minCorner, maxCorner);
        }

        private int[] indices;
        private Primitive[] primitives;
        private Node treeRoot;
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
                if (h.Material.Reflective == true)
                {
                    return tracer(Reflection(r, h));
                }
                return comineColorLight(h.Material.Color, DirectIllumination(r, h));
            }
            else
                // No hit, black background
                return new Vector3 (0f, 0f, 0f);
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
            float shortest = d;
            Primitive thing = null;
            foreach (Primitive p in primitives)
            {
                if (o == null || p != o)
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
            }
            return thing;
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
                    //result += (pL.Color * attenuation * dot) + new Vector3(0.1f, 0.1f, 0.1f);// TODO REMOVE SIMPLE AO;
                    result = new Vector3(1, 1, 1);
                }
            }
            return result;
        }
    }
}
