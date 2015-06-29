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
        private FrameRateCounter frameRateCounter;
        private Texture2D texture;
        private Color[] pixels;

        // Game objects and variables
        private Camera camera;
        private Screen screen;
        private Eye eye;
        private Engine engine;

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

            this.eye = new Eye(new Vector3(1.5f, 0, 2), new Vector3(-2, 0, -1), 1f);
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
            FileModel bunny = new FileModel(Content.Load<XnaModel>("Models/bunny"), bunnyMaterial, new Vector3(0, 0.1f, 1.2f), new Vector3(-2, -2, 2));

            // Actually load
            //loader.LoadModel(Model.LoadFromSinglePrimitive(sphere));
            //loader.LoadModel(Model.LoadFromSinglePrimitive(triangle));
            loader.LoadModel(bunny);

            /*
             *  Load engine with primites and point lights managed by the model loader,
             *  primitives are converted to models before loading using
             *  LoadFromSinglePrimitve.
             */
            this.engine = new Engine(
                loader.Primitives,
                loader.PointLights);

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
            this.Window.Title = "Ray Tracer | FPS: " + this.frameRateCounter.FrameRate.ToString();

            KeyboardState keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }
            if (keyState.IsKeyDown(Keys.Left))
            {
                eye.UpdateDirection(MathHelper.ToRadians(90));
            }
            if (keyState.IsKeyDown(Keys.Right))
            {
                eye.UpdateDirection(-1 * MathHelper.ToRadians(90));
            }

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