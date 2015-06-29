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
using Model = GraphicsPractical3.Geometry.Model;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;

namespace GraphicsPractical3
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        // Often used XNA objects
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Texture2D texture;
        private Color[] pixels;

        // Game objects and variables
        private Screen screen;
        private Eye eye;
        private Engine engine;

        // View variables
        private float viewAngle;
        private Vector3 viewCenter;

        // Diagnostic variables
        private System.Diagnostics.Stopwatch sw;

        public Game1()
        {
            this.graphics = new GraphicsDeviceManager(this);
            this.Content.RootDirectory = "Content";
            this.viewAngle = 0f;
            this.viewCenter = Vector3.Zero;
            this.sw = new System.Diagnostics.Stopwatch();
        }

        protected override void Initialize()
        {
            this.screen = new Screen(640, 480, 0.001f);
            /* Initialize Graphics Device */

            // Copy over the device's rasterizer state to change the current fillMode
            this.GraphicsDevice.RasterizerState = new RasterizerState() { CullMode = CullMode.None };
            // Set up the window
            this.graphics.IsFullScreen = false;
            this.graphics.PreferredBackBufferWidth = screen.Width;
            this.graphics.PreferredBackBufferHeight = screen.Height;
            // Let the renderer draw and update as often as possible
            this.graphics.SynchronizeWithVerticalRetrace = false;
            this.IsFixedTimeStep = false;
            // Flush the changes to the device parameters to the graphics card
            this.graphics.ApplyChanges();

            /* Initialize Ray Tracer */
            this.viewCenter = new Vector3(0, -0.1f, 0); // Center of bunny
            var eyePosition = new Vector3(0, -0.3f, 1.2f);
            this.eye = new Eye(eyePosition, viewCenter - eyePosition, 1f);

            this.pixels = new Color[screen.Height * screen.Width];
            this.texture = new Texture2D(GraphicsDevice, screen.Width, screen.Height);

            /* Initialize models! */

            ModelLoader loader = new ModelLoader();

            // Add lights
            loader.LoadPointLight(new Light(new Vector3(20, -50, -30), new Vector3(80, 80, 80)));
            loader.LoadPointLight(new Light(new Vector3(50, 20, -30), new Vector3(80, 80, 80)));
            loader.LoadPointLight(new Light(eye.Position, eye.Direction)); // Always a flashlight behind the camera :)

            // Init models (also single-primitve-models)
            Primitive sphere = new Sphere(new Vector3(0, 0, 8), 1f);
            sphere.Material.Color = new Vector3(0.5f, 0.5f, 0f);

            Primitive triangle = new Triangle(new Vector3(-3, -3, 8), new Vector3(3, 3, 8), new Vector3(-3, 3, 8));
            triangle.Material.Color = new Vector3(0.1f, 0.75f, 0.8f);
            triangle.Material.Reflective = false;

            // Init bunny model
            Material bunnyMaterial = new Material();
            bunnyMaterial.Color = new Vector3(0.8f, 0.2f, 0.2f);
            bunnyMaterial.Reflective = false;
            FileModel bunny = new FileModel(Content.Load<XnaModel>("Models/bunny"), bunnyMaterial, new Vector3(0, 0.1f, 0), new Vector3(-2, -2, 2));

            // Actually load
            //loader.LoadModel(Model.LoadFromSinglePrimitive(sphere));
            //loader.LoadModel(Model.LoadFromSinglePrimitive(triangle));
            loader.LoadModel(bunny);

            /*
             *  Load engine with primites and point lights managed by the model loader,
             *  primitives are converted to models before loading using
             *  LoadFromSinglePrimitve.
             */
            this.engine = new Engine
                (
                    loader.Primitives,
                    loader.PointLights,
                    regenerateBvhTree: false // Switch to false to load from file, when true the tree will be regenerated and written to a file
                );

            this.IsMouseVisible = true;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a SpriteBatch object
            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
        }

        protected void UpdateEye()
        {
            // Update eye so that it rotates around the view center
            // with angle viewAngle
            var ep = eye.Position;
            var r = Math.Sqrt (   Math.Pow(viewCenter.X - eye.Position.X, 2)
                                + Math.Pow(viewCenter.Z - eye.Position.Z, 2)
                              );
            var x = viewCenter.X + (float) (r * Math.Cos(viewAngle));
            var y = eye.Position.Y;
            var z = viewCenter.Z + (float) (r * Math.Sin(viewAngle));
            ep = new Vector3(x, y, z);
            this.eye = new Eye(ep, viewCenter - ep, 1f);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.Escape))
                Exit();

            // Rotate
            //
            if (keyState.IsKeyDown(Keys.Left))
                viewAngle -= (float)(0.5 * Math.PI * 0.1);
            if (keyState.IsKeyDown(Keys.Right))
                viewAngle += (float)(0.5 * Math.PI * 0.1);

            // Zoom
            //
            if (keyState.IsKeyDown(Keys.Up))
                Vector3.Multiply(ref eye.Position, 0.9f, out eye.Position);
            if (keyState.IsKeyDown(Keys.Down))
                Vector3.Multiply(ref eye.Position, 1.1f, out eye.Position);

            UpdateEye();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clear the screen in a predetermined color and clear the depth buffer
            // Also unset the texture buffer on the Grahpics Device for now
            this.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
            this.GraphicsDevice.Textures[0] = null;

            // (measure Framerate)
            sw.Reset();
            sw.Start();

            // Load texture and draw texture, the texture is the 2D output of the
            // ray tracer!
            //
            pixels = engine.Update(eye, screen);
            texture.SetData(pixels);
            this.GraphicsDevice.Textures[0] = texture; // Load texture here! (necessary?)
            spriteBatch.Begin();
            spriteBatch.Draw(texture, Vector2.Zero, Color.Azure);
            spriteBatch.End();

            // (print Framerate)
            sw.Stop();
            this.Window.Title = "Ray Tracer | FPS: " + (1f / sw.ElapsedMilliseconds * 1000).ToString();

            base.Draw(gameTime);
        }
    }
}