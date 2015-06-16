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
using Engine = GraphicsPractical3.RayTracing.Engine;
using Eye = GraphicsPractical3.RayTracing.Eye;
using Screen = GraphicsPractical3.RayTracing.Screen;

namespace GraphicsPractical3
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        // Often used XNA objects
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private FrameRateCounter frameRateCounter;
        private Texture2D texture;
        private Color[] pixels;

        // Game objects and variables
        private Camera camera;
        private Screen screen;
        private Eye eye;
        private Engine engine;

        // Model
        private Primitive[] primitives;
        private PointLight[] pointLights;

        public Game1()
        {
            this.graphics = new GraphicsDeviceManager(this);
            this.Content.RootDirectory = "Content";
            // Create and add a frame rate counter
            this.frameRateCounter = new FrameRateCounter(this);
            this.Components.Add(this.frameRateCounter);
        }

        protected override void Initialize()
        {
            /* Initialize Graphics Device */

            // Copy over the device's rasterizer state to change the current fillMode
            this.GraphicsDevice.RasterizerState = new RasterizerState() { CullMode = CullMode.None };
            // Set up the window
            this.graphics.PreferredBackBufferWidth = 640;
            this.graphics.PreferredBackBufferHeight = 480;
            this.graphics.IsFullScreen = false;
            // Let the renderer draw and update as often as possible
            this.graphics.SynchronizeWithVerticalRetrace = false;
            this.IsFixedTimeStep = false;
            // Flush the changes to the device parameters to the graphics card
            this.graphics.ApplyChanges();

            /* Initialize Ray Tracer */

            this.pixels = new Color[307200];
            this.eye = new Eye(new Vector3(0, 1, 0), new Vector3(0, 0, 1), 1f);
            this.screen = new Screen(640, 480, 0.001f);
            this.pointLights = new PointLight[1];
            this.primitives = new Primitive[2];
            this.texture = new Texture2D(GraphicsDevice, 640, 480);

            this.primitives[0] = new Sphere(new Vector3(-1, 1, -10), 1f);
            this.primitives[0].Color = new Vector3(1f, 0f, 0f);
<<<<<<< HEAD
            this.pointLights[0] = new Light(new Vector3(15, 1, -30), new Vector3(100, 100, 100));
=======

            this.primitives[1] = new Triangle(new Vector3(-1, -1, -10), new Vector3(-1, 1, -5), new Vector3(-1, 3, -10));
            this.primitives[1].Color = new Vector3(0f, 0f, 1f);

            this.pointLights[0] = new Light(new Vector3(10, 10, 1), new Vector3(1, 1, 1));
>>>>>>> 029fda3de32127d1768f6e393335e413fc12d94e

            this.engine = new Engine(primitives, pointLights);

            /* Misc */

            this.IsMouseVisible = true;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a SpriteBatch object
            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            float timeStep = (float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f;

            // Update the window title
            this.Window.Title = "Ray Tracer | FPS: " + this.frameRateCounter.FrameRate;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clear the screen in a predetermined color and clear the depth buffer
            // Also unset the texture buffer on the Grahpics Device for now
            this.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
            this.GraphicsDevice.Textures[0] = null;

            // Load texture and draw texture, the texture is the 2D output of the
            // ray tracer!
            //
            pixels = engine.Update(eye, screen);
            texture.SetData(pixels);
            this.GraphicsDevice.Textures[0] = texture; // Load texture here! (necessary?)
            spriteBatch.Begin();
            spriteBatch.Draw(texture, Vector2.Zero, Color.Azure);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}