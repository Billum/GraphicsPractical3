using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using Microsoft.Xna.Framework;

using BoundingBox = GraphicsPractical3.Geometry.BoundingBox;
using Primitive = GraphicsPractical3.Geometry.Primitive;
using Ray = GraphicsPractical3.Geometry.Ray;

namespace GraphicsPractical3
{
    public class BVHTree
    {
        public enum Axis
        {
            X,
            Y,
            Z,
            None
        }

        public class Node
        {
            public int Start;
            public int End;
            public Axis SplitOver;
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
                else if (!h2.HasValue)
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
            for (int i = 0; i < indices.Length; i++)
                indices[i] = i;

            this.primitives = primitives;
            this.treeRoot = new Node
            {
                Start = 0,
                End = primitives.Length,
                SplitOver = Axis.None,
                BoundingBox = BoundingBoxOverPrimitives(0, indices.Length)
            };

            Split(treeRoot);
        }

        public BVHTree(Primitive[] primitives, string bvhFilename)
        {
            this.indices = new int[primitives.Length];
            this.primitives = primitives;

            ReadFromFile(bvhFilename);
        }

        /*
         * Serialize
         */

        public void SaveToFile(string filename)
        {
            var outputStream = new FileStream(filename, FileMode.Create);
            var writer = new StreamWriter(outputStream);

            WriteNode(treeRoot, writer, 0, 'O');

            writer.WriteLine("0"); // Last line indicator of nodes indicator
            writer.WriteLine("E");

            WriteIndices(writer);

            writer.Close();
            outputStream.Close();
        }

        private void WriteNode(Node n, StreamWriter writer, int level, char code)
        {
            if (n == null)
                return;

            writer.WriteLine(level);
            writer.WriteLine(code);
            writer.WriteLine(n.Start);
            writer.WriteLine(n.End);
            writer.WriteLine(n.BoundingBox.MinCorner.X);
            writer.WriteLine(n.BoundingBox.MinCorner.Y);
            writer.WriteLine(n.BoundingBox.MinCorner.Z);
            writer.WriteLine(n.BoundingBox.MaxCorner.X);
            writer.WriteLine(n.BoundingBox.MaxCorner.Y);
            writer.WriteLine(n.BoundingBox.MaxCorner.Z);

            WriteNode(n.LeftChild, writer, level + 1, 'L');
            WriteNode(n.RightChild, writer, level + 1, 'R');
        }

        private void WriteIndices(StreamWriter writer)
        {
            for (int i = 0; i < indices.Length; i++)
                writer.WriteLine(indices[i].ToString());
        }

        /*
         * Unserialize
         */

        private void GetStreamPos(StreamReader reader, out int charpos, out int charlen)
        {
            charpos = (int)(Int32)reader.GetType().InvokeMember(
                "charPos",
                BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null,
                reader,
                null);

            charlen = (int)(Int32)reader.GetType().InvokeMember(
                "charLen",
                BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null,
                reader,
                null);
        }

        private void SetStreamPos(StreamReader reader, int charpos, int charlen)
        {
            reader.GetType().InvokeMember(
                "charPos",
                BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null,
                reader,
                new object[] { charpos });

            reader.GetType().InvokeMember(
                "charLen",
                BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null,
                reader,
                new object[] { charlen });
        }

        private void ReadFromFile(string filename)
        {
            var inputStream = new FileStream(filename, FileMode.Open);
            var reader = new StreamReader(inputStream);

            ReadNode(ref treeRoot, reader, 0, 'O');
            ReadIndices(reader);

            reader.Close();
            inputStream.Close();
        }

        private void ReadNode(ref Node n, StreamReader reader, int level, char code)
        {
            int charpos, charlen;
            GetStreamPos(reader, out charpos, out charlen);

            int rlevel = int.Parse(reader.ReadLine());
            char rcode = char.Parse(reader.ReadLine());

            if (rlevel != level || rcode != code)
            {
                if (rcode == 'E')
                    return; // End!

                // unread level and code lines
                SetStreamPos(reader, charpos, charlen);
                return;
            }

            n = new Node
            {
                Start = int.Parse(reader.ReadLine()),
                End = int.Parse(reader.ReadLine()),
                BoundingBox = new BoundingBox(
                    new Vector3(
                        float.Parse(reader.ReadLine()),
                        float.Parse(reader.ReadLine()),
                        float.Parse(reader.ReadLine())
                        ),
                    new Vector3(
                        float.Parse(reader.ReadLine()),
                        float.Parse(reader.ReadLine()),
                        float.Parse(reader.ReadLine())
                        ))
            };

            ReadNode(ref n.LeftChild, reader, level + 1, 'L');
            ReadNode(ref n.RightChild, reader, level + 1, 'R');
        }

        private void ReadIndices(StreamReader reader)
        {
            int i = 0;
            string line;
            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                indices[i] = int.Parse(line);
                i++;
            }
        }

        // ----

        public Primitive TryHit(Ray r, Primitive except = null, float minDistance = float.MaxValue)
        {
            Hit? hit = HitNode(treeRoot, r, except, minDistance);
            if (hit.HasValue)
                return hit.Value.Primitive;
            else
                return null; // No hit
        }

        private Hit? HitNode(Node n, Ray r, Primitive except, float minDistance)
        {
            if (n == null)
                return null; // No hit

            if (n.BoundingBox.Intersect(r) == 0.0f)
                return null; // No hit

            if (n.LeftChild == null || n.RightChild == null)
            {
                // We're in a leaf node, find the best hit in the current
                // subset of primitives and return it
                Hit? lowestHit = new Hit(minDistance, null);
                for (int i = n.Start; i < n.End; i++)
                {
                    Primitive p = primitives[indices[i]];
                    if (p == except)
                        continue; // Skip this one

                    float hitDistance;
                    if ((hitDistance = p.HitDistance(r)) != 0.0f)
                        lowestHit = Hit.BestHit(lowestHit, new Hit(hitDistance, p));
                }

                return lowestHit;
            }
            else
            {
                // Return the best hit from the two subtrees
                return Hit.BestHit(
                            HitNode(n.LeftChild, r, except, minDistance),
                            HitNode(n.RightChild, r, except, minDistance)
                        );
            }
        }

        private float BestSplit(Node n, out Axis splitOver, out float splitCost)
        {
            float lowestSplitValue = 0.0f;
            Axis lowestSplitOver = Axis.None;
            float lowestSplitCost = float.MaxValue;

            for (int i = n.Start; i < n.End; i++)
            {
                Primitive p = primitives[indices[i]];
                Vector3 c = p.Center();
                var costX = Cost(n.Start, n.End, p.Center().X, Axis.X);
                var costY = Cost(n.Start, n.End, p.Center().Y, Axis.Y);
                var costZ = Cost(n.Start, n.End, p.Center().Z, Axis.Z);

                // check whether lower for each split X, Y and Z
                // and overwrite if necessary
                // this way we select the best split over all dimensions

                if (costX < lowestSplitCost)
                {
                    lowestSplitValue = p.Center().X;
                    lowestSplitCost = costX;
                    lowestSplitOver = Axis.X;
                }

                if (costY < lowestSplitCost)
                {
                    lowestSplitValue = p.Center().Y;
                    lowestSplitCost = costY;
                    lowestSplitOver = Axis.Y;
                }

                if (costZ < lowestSplitCost)
                {
                    lowestSplitValue = p.Center().Z;
                    lowestSplitCost = costZ;
                    lowestSplitOver = Axis.Z;
                }
            }

            // also out the cost
            splitCost = lowestSplitCost;
            splitOver = lowestSplitOver;
            return lowestSplitValue;
        }

        private void Split(Node n, float previousSplitCost = float.MaxValue /* By default split anyways */)
        {
            if (n.Num() < 1)
                return; // No splitting necessary

            float splitCost = 0f;
            Axis splitOver = Axis.None;
            float split = BestSplit(n, out splitOver, out splitCost);

            if (splitCost >= previousSplitCost)
                return; // Terminal condition reached, when the split cost is actually
                        // worse than the split cost of the previous split, we won't
                        // continue

            var indicesLeft = new List<int>();
            var indicesRight = new List<int>();

            // Split indices in current subset of indices array over two lists,
            // left of the split point and right of the split point
            for (int i = n.Start; i < n.End; i++)
            {
                var p = primitives[indices[i]];

                // Determine x y or z value over which to split
                float psplit = 0f;
                switch (splitOver)
                {
                    case Axis.X: psplit = p.Center().X;
                        break;
                    case Axis.Y: psplit = p.Center().Y;
                        break;
                    case Axis.Z: psplit = p.Center().Z;
                        break;
                }

                if (psplit < split)
                    indicesLeft.Add(indices[i]);
                else
                    indicesRight.Add(indices[i]);
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

            // Split over ...
            n.SplitOver = splitOver;

            // Register left child with start and end offsets
            n.LeftChild = new Node
            {
                Start = n.Start,
                End = c1,
                SplitOver = Axis.None, // Is determined by splitting
                BoundingBox = BoundingBoxOverPrimitives(n.Start, c1)
            };

            // Register right child with start and end offsets
            n.RightChild = new Node
            {
                Start = c1,
                End = c2,
                SplitOver = Axis.None, // Is determined by splitting
                BoundingBox = BoundingBoxOverPrimitives(c1, c2)
            };

            // Further split left and right
            Split(n.LeftChild, splitCost);
            Split(n.RightChild, splitCost);
        }

        private float Cost(int start, int end, float split, Axis over)
        {
            Vector3 minCornerLeft = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 maxCornerLeft = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            Vector3 minCornerRight = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 maxCornerRight = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            int numLeft = 0;
            int numRight = 0;

            for (int i = start; i < end; i++)
            {
                Primitive p = primitives[indices[i]];

                var bbox = p.BoundingBox();

                // Determine x y or z value over which to split
                float psplit = 0f;
                switch (over)
                {
                    case Axis.X: psplit = p.Center().X;
                        break;
                    case Axis.Y: psplit = p.Center().Y;
                        break;
                    case Axis.Z: psplit = p.Center().Z;
                        break;
                }

                // Compare current x y or z to split x y or z to determine
                // whether to put in left or right bbox
                if (psplit < split)
                {
                    minCornerLeft.X = Math.Min(minCornerLeft.X, bbox.MinCorner.X);
                    minCornerLeft.Y = Math.Min(minCornerLeft.Y, bbox.MinCorner.Y);
                    minCornerLeft.Z = Math.Min(minCornerLeft.Z, bbox.MinCorner.Z);
                    maxCornerLeft.X = Math.Max(maxCornerLeft.X, bbox.MaxCorner.X);
                    maxCornerLeft.Y = Math.Max(maxCornerLeft.Y, bbox.MaxCorner.Y);
                    maxCornerLeft.Z = Math.Max(maxCornerLeft.Z, bbox.MaxCorner.Z);
                    numLeft++;
                }
                else
                {
                    minCornerRight.X = Math.Min(minCornerRight.X, bbox.MinCorner.X);
                    minCornerRight.Y = Math.Min(minCornerRight.Y, bbox.MinCorner.Y);
                    minCornerRight.Z = Math.Min(minCornerRight.Z, bbox.MinCorner.Z);
                    maxCornerRight.X = Math.Max(maxCornerRight.X, bbox.MaxCorner.X);
                    maxCornerRight.Y = Math.Max(maxCornerRight.Y, bbox.MaxCorner.Y);
                    maxCornerRight.Z = Math.Max(maxCornerRight.Z, bbox.MaxCorner.Z);
                    numRight++;
                }
            }

            var bboxLeft = new BoundingBox(minCornerLeft, maxCornerLeft);
            var bboxRight = new BoundingBox(minCornerRight, maxCornerRight);

            return (numLeft * bboxLeft.SurfaceArea()) + (numRight * bboxRight.SurfaceArea());
        }

        private BoundingBox BoundingBoxOverPrimitives(int start, int end)
        {
            Vector3 minCorner = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 maxCorner = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int i = start; i < end; i++)
            {
                var p = primitives[indices[i]];
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
}
