using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Futuristic_Steve
{
    class Platform : GameObject
    {
        /// <summary>
        /// Constructor for the pickups class
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="rectangle"></param>
        /// <param name="elapsedTime"></param>
        //constructor
        public Platform(Texture2D asset, Rectangle rectangle, double elapsedTime) : base(asset, rectangle, elapsedTime)
        {
            this.asset = asset;
            this.rectangle = rectangle;
            this.elapsedTime = elapsedTime;
        }

        //methods

        /// <summary>
        /// Override for the draw method in the object class
        /// </summary>
        /// <param name="sb"></param>
        public override void Draw(SpriteBatch sb)
        {
            sb.Draw(asset, rectangle, Color.White);
        }
    }
}
