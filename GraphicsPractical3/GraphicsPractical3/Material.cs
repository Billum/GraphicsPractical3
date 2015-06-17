using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Primitive = GraphicsPractical3.Geometry.Primitive;
using Ray = GraphicsPractical3.Geometry.Ray;

namespace GraphicsPractical3
{
    public struct Material
    {
        public Vector3 Color;
        public bool Reflective;
        public bool Translucent;
    }
}