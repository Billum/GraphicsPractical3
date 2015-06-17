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

namespace GraphicsPractical3
{
    class MyModel
    {
        public MyModel(Model xnaModel)
        {
            List<Vector3> vertices = new List<Vector3>();

            foreach (var mesh in xnaModel.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var vertexElements = meshPart.VertexBuffer.VertexDeclaration.GetVertexElements();
                    VertexElement? vertexPos = null;
                    foreach (var vertexElement in vertexElements)
                    {
                        if (vertexElement.VertexElementUsage == VertexElementUsage.Position &&
                            vertexElement.VertexElementFormat == VertexElementFormat.Vector3)
                        {
                            // Found suported vertices!
                            vertexPos = vertexElement;
                            break;
                        }
                    }

                    if (!vertexPos.HasValue)
                        throw new Exception("Failed to load model, incorrect format");

                    // Allocate memory for all vertices
                    var tmpVertices = new Vector3[meshPart.NumVertices];

                    // Load vertices in memory
                    meshPart.VertexBuffer.GetData<Vector3>
                    (
                        // Calculate offset in model vertices
                        meshPart.VertexOffset * meshPart.VertexBuffer.VertexDeclaration.VertexStride + vertexPos.Value.Offset,
                        tmpVertices,
                        0,
                        meshPart.NumVertices,
                        meshPart.VertexBuffer.VertexDeclaration.VertexStride // Use correct format size
                    );

                    vertices.AddRange(tmpVertices);
                }
            }

            // Copy list to array!
            Vertices = vertices.ToArray();

            // Transform!
            transformVertices(Vertices);
        }

        private void transformVertices(Vector3[] v)
        {
            for (int i = 0; i < v.Length; i++)
                v[i] = Vector3.Transform(v[i], new Matrix() /* TODO! */);
        }

        public Vector3[] Vertices { get; private set; }
    }
}
