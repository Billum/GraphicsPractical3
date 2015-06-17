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
            this.pointLights = new PointLight[2];
            // Initiate Point Lights here

            this.pointLights[0] = new Light(new Vector3(50, -20, -30), new Vector3(80, 80, 80));
            this.pointLights[1] = new Light(new Vector3(20, -50, -30), new Vector3(80, 80, 80));

            this.primitives = new Primitive[2];
            // Initiate Primitives here

            this.primitives[0] = new Sphere(new Vector3(0, 0, 8), 1f);
            this.primitives[0].Material.Color = new Vector3(0.5f, 0.5f, 0f);

            this.primitives[1] = new Triangle(new Vector3(-3, -3, 8), new Vector3(3, 3, 8), new Vector3(-3, 3, 8));
            this.primitives[1].Material.Color = new Vector3(0.1f, 0.75f, 0.8f);
            this.primitives[1].Material.Reflective = false;
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
