using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GraphicsPractical3
{
    /// <summary>
    /// This class keeps track of the framerate of the game.
    /// </summary>
    class FrameRateCounter : DrawableGameComponent
    {
        private float frameRate;
        private System.Diagnostics.Stopwatch sw;

        public FrameRateCounter(Game game)
            : base(game)
        {
            this.frameRate = 0;
            this.sw = new System.Diagnostics.Stopwatch();
            this.sw.Start();
        }

        public override void Update(GameTime gameTime)
        {

        }

        public override void Draw(GameTime gameTime)
        {
            sw.Stop();
            this.frameRate = 1f / sw.Elapsed.Seconds;
            sw.Reset();
            sw.Start();
        }

        /// <summary>
        /// Returns the current framerate of the game.
        /// </summary>
        public float FrameRate
        {
            get { return this.frameRate; }
        }
    }
}