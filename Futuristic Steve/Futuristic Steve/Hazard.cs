using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Futuristic_Steve
{
    class Hazard : GameObject
    {

        /// <summary>
        /// Constructor for the hazard class
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="rectangle"></param>
        /// <param name="elapsedTime"></param>
        public Hazard(Texture2D asset, Rectangle rectangle, double elapsedTime)
            : base(asset, rectangle, elapsedTime)
        {
            this.asset = asset;
            this.rectangle = rectangle;
            this.elapsedTime = elapsedTime;
        }

        /// <summary>
        /// Override for the draw method in objects class
        /// </summary>
        /// <param name="sb"></param>
        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
        }

    }
}
