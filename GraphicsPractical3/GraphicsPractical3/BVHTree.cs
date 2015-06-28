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
                BoundingBox =
                    // Alwasy hit this bounding box, no need to recalculate
                    // the super bounding box over all primitives
                    new BoundingBox(
                          new Vector3( float.MinValue
                                     , float.MinValue
                                     , float.MinValue)
                        , new Vector3( float.MaxValue
                                     , float.MaxValue
                                     , float.MaxValue)
                    )
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

            if (n.BoundingBox.Intersect(r) == 0.0f)
                return null; // No hit

            if (n.LeftChild == null && n.RightChild == null)
            {
                // We're in a leaf node, find the best hit in the current
                // subset of primitives and return it
                Hit? lowestHit = null;
                for (int i = n.Start; i < n.End; i++)
                {
                    Primitive p = primitives[indices[i]];

                    float hitDistance;
                    if ((hitDistance = p.HitDistance(r)) != 0.0f)
                        lowestHit = Hit.BestHit(lowestHit, new Hit(hitDistance, p));
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
}
