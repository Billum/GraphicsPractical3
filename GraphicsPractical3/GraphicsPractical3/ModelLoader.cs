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
using Model = GraphicsPractical3.Geometry.Model;

namespace GraphicsPractical3
{
    class ModelLoader
    {
        public Primitive[] Primitives {
            get {
                int num = models.Sum(m => m.Primitives.Count());
                var p = new Primitive[num];
                int c = 0;
                foreach (var m in models)
                {
                    m.Primitives.CopyTo(p, c);
                    c += m.Primitives.Count();
                }
                return p;
            }
        }

        public PointLight[] PointLights {
            get {
                return pointLights.ToArray();
            }
        }

        private List<PointLight> pointLights = new List<PointLight>();
        private List<Model> models = new List<Model>();

        public void LoadModel(Model m)
        {
            models.Add(m);
        }

        public void LoadPointLight(PointLight pl)
        {
            pointLights.Add(pl);
        }
    }
}
