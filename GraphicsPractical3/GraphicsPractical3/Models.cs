using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Triangle = GraphicsPractical3.Geometry.Triangle;
using Sphere = GraphicsPractical3.Geometry.Sphere;
using Light = GraphicsPractical3.Geometry.PointLight;
using Primitive = GraphicsPractical3.Geometry.Primitive;
using PointLight = GraphicsPractical3.Geometry.PointLight;

namespace GraphicsPractical3
{
    class Models
    {
        private Primitive[] primitives;
        private PointLight[] pointLights;
        
        public Models()
        {
            this.pointLights = new PointLight[1];
            // Initiate Point Lights here

            this.pointLights[0] = new Light(new Vector3(20, -15, -20), new Vector3(100, 100, 100));

            this.primitives = new Primitive[2];
            // Initiate Primitives here

            this.primitives[0] = new Sphere(new Vector3(0, 0, 10), 1f);
            this.primitives[0].Color = new Vector3(1f, 0f, 0f);

            this.primitives[1] = new Triangle(new Vector3(-3, -3, 12), new Vector3(-3, 3, 12), new Vector3(3, 3, 12));
            this.primitives[1].Color = new Vector3(0f, 0f, 1f);
        }

        public Primitive[] Primitives()
        {
            return primitives;
        }

        public PointLight[] PointLights()
        {
            return pointLights;
        }
    }
}
