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

namespace GraphicsPractical3
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        // Often used XNA objects
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private FrameRateCounter frameRateCounter;

        // Game objects and variables
        private Camera camera;

        // Model
        private Model model;
        private Material modelMaterial;

        // Quad
        private VertexPositionNormalTexture[] quadVertices;
        private short[] quadIndices;
        private Matrix quadTransform;

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
            // Copy over the device's rasterizer state to change the current fillMode
            this.GraphicsDevice.RasterizerState = new RasterizerState() { CullMode = CullMode.None };
            // Set up the window
            this.graphics.PreferredBackBufferWidth = 800;
            this.graphics.PreferredBackBufferHeight = 600;
            this.graphics.IsFullScreen = false;
            // Let the renderer draw and update as often as possible
            this.graphics.SynchronizeWithVerticalRetrace = false;
            this.IsFixedTimeStep = false;
            // Flush the changes to the device parameters to the graphics card
            this.graphics.ApplyChanges();
            // Initialize the camera
            this.camera = new Camera(new Vector3(0, 50, 100), new Vector3(0, 0, 0), new Vector3(0, 1, 0));

            // Initialize material properties
            this.modelMaterial = new Material();
            modelMaterial.AmbientColor = Color.Red;
            modelMaterial.AmbientIntensity = 0.2f;
            modelMaterial.DiffuseColor = Color.Red;
            modelMaterial.DiffuseTexture = this.Content.Load<Texture>("Textures/CobblestonesDiffuse");
            modelMaterial.SpecularColor = Color.White;
            modelMaterial.SpecularIntensity = 2f;
            modelMaterial.SpecularPower = 25f;

            modelMaterial.NormalColoring = false;       // Set to true to use normal coloring!
            modelMaterial.ProceduralColoring = false;   // Set to true to get the checkerboard pattern

            this.IsMouseVisible = true;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a SpriteBatch object
            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
            // Load the "Simple" effect
            Effect effect = this.Content.Load<Effect>("Effects/Simple");
            // Load the model and let it use the "Simple" effect
            this.model = this.Content.Load<Model>("Models/Teapot");
            this.model.Meshes[0].MeshParts[0].Effect = effect;
            // Setup the quad
            this.setupQuad();
        }

        /// <summary>
        /// Sets up a 2 by 2 quad around the origin.
        /// </summary>
        private void setupQuad()
        {
            float scale = 50.0f;
            float height = -1.5f;

            // Normal points up
            Vector3 quadNormal = new Vector3(0, 1, 0);

            this.quadVertices = new VertexPositionNormalTexture[4];
            // Top left
            this.quadVertices[0].Position = new Vector3(-6, height, -7);
            this.quadVertices[0].Normal = quadNormal;
            this.quadVertices[0].TextureCoordinate = new Vector2(-1, -1);
            // Top right
            this.quadVertices[1].Position = new Vector3(6, height, -7);
            this.quadVertices[1].Normal = quadNormal;
            this.quadVertices[1].TextureCoordinate = new Vector2(1, -1);
            // Bottom left
            this.quadVertices[2].Position = new Vector3(-6, height, 5);
            this.quadVertices[2].Normal = quadNormal;
            this.quadVertices[2].TextureCoordinate = new Vector2(-1, 1);
            // Bottom right
            this.quadVertices[3].Position = new Vector3(6, height, 5);
            this.quadVertices[3].Normal = quadNormal;
            this.quadVertices[3].TextureCoordinate = new Vector2(1, 1);

            this.quadIndices = new short[] { 0, 1, 2, 1, 2, 3 };
            this.quadTransform = Matrix.CreateScale(scale);
        }

        protected override void Update(GameTime gameTime)
        {
            float timeStep = (float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f;

            // Update the window title
            this.Window.Title = "XNA Renderer | FPS: " + this.frameRateCounter.FrameRate;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clear the screen in a predetermined color and clear the depth buffer
            this.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

            // Get the model's only mesh
            ModelMesh mesh = this.model.Meshes[0];
            Effect effect = mesh.Effects[0];

            effect.CurrentTechnique = effect.Techniques["Surface"];

            // Draw the index/vertices from the surface texture
            // , the texture itself will be applied in the shader
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                this.GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    quadVertices,
                    0,
                    quadVertices.Length,
                    quadIndices,
                    0,
                    quadIndices.Length / 3);
            }

            // Set the effect parameters
            effect.CurrentTechnique = effect.Techniques["Simple"];

            // Matrices for 3D perspective projection
            this.camera.SetEffectParameters(effect);
            this.modelMaterial.SetEffectParameters(effect);
            effect.Parameters["World"].SetValue(Matrix.CreateScale(10.0f));
            effect.Parameters["WorldInverseTranspose"].SetValue(Matrix.CreateScale(10.0f));
            effect.Parameters["LightSourcePosition"].SetValue(new Vector3(50, 20, 50));
            effect.Parameters["Camera"].SetValue(camera.Eye);

            // Draw the model
            mesh.Draw();

            base.Draw(gameTime);
        }
    }
}