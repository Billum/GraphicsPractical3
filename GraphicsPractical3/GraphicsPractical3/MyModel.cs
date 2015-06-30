using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Model = GraphicsPractical3.Geometry.Model;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
using Triangle = GraphicsPractical3.Geometry.Triangle;

namespace GraphicsPractical3
{
    class FileModel : Model
    {
        public FileModel(XnaModel xnaModel, Material material, Vector3 dpos, Vector3 transform)
        {
            Primitives = new Triangle[xnaModel.Meshes.Sum(m => m.MeshParts.Sum(mp => mp.PrimitiveCount))];
            int woff = 0;

            foreach (var mesh in xnaModel.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    /*
                     * Check supported vertices, only load from the first supported vertex
                     * (format).
                     */

                    var vertexElements = meshPart.VertexBuffer.VertexDeclaration.GetVertexElements();
                    VertexElement? vertexPos = null;
                    foreach (var vertexElement in vertexElements)
                    {
                        if (vertexElement.VertexElementUsage == VertexElementUsage.Position &&
                            vertexElement.VertexElementFormat == VertexElementFormat.Vector3)
                        {
                            // Found supported vertices!
                            vertexPos = vertexElement;
                            break;
                        }
                    }

                    if (!vertexPos.HasValue)
                        throw new Exception("Failed to load model, incorrect format");

                    /*
                     * Load vertex data into vertices buffer.
                     */

                    var vertices = new Vector3[meshPart.NumVertices];
                    meshPart.VertexBuffer.GetData<Vector3>(meshPart.VertexOffset * meshPart.VertexBuffer.VertexDeclaration.VertexStride
                                                                + vertexPos.Value.Offset // Calculate offset in model vertices
                                                           , vertices
                                                           , 0
                                                           , meshPart.NumVertices
                                                           , meshPart.VertexBuffer.VertexDeclaration.VertexStride); // Use correct format size

                    transformVertices(vertices, dpos, transform);

                    /*
                     *  Load indices into indices buffer.
                     */

                    var indices = new short[meshPart.PrimitiveCount * 3];
                    meshPart.IndexBuffer.GetData<short>(  0
                                                        , indices
                                                        , 0
                                                        , meshPart.PrimitiveCount * 3);

                    /*
                     *  Copy triangles into triangle buffer by using the indices
                     *  and vertices buffer;
                     */

                    for (int i = woff; i < (woff + meshPart.PrimitiveCount); i++)
                    {
                        int vi = i - woff;
                        int i0 = indices[vi * 3 + 0],
                            i1 = indices[vi * 3 + 1],
                            i2 = indices[vi * 3 + 2];
                        Primitives[i] = new Triangle( vertices[i0]
                                                    , vertices[i1]
                                                    , vertices[i2]);

                        Primitives[i].Material = material; // Set material
                    }

                    woff += meshPart.PrimitiveCount;
                }
            }
        }

        private void transformVertices(Vector3[] v, Vector3 dpos, Vector3 transform)
        {
            for (int i = 0; i < v.Length; i++)
            {
                v[i] = Vector3.Multiply(v[i], transform);
                v[i] = Vector3.Add(v[i], dpos);
            }
        }
    }
}
