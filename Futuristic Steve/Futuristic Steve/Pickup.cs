using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Futuristic_Steve
{
    class Pickup : GameObject
    {
        //Fields
        private bool isActive = true;

        //Properties
        public bool IsActive
        { get { return isActive; } }

        /// <summary>
        /// Constructor for the pickups class
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="rectangle"></param>
        /// <param name="elapsedTime"></param>
        public Pickup(Texture2D asset, Rectangle rectangle, double elapsedTime)
            : base(asset, rectangle, elapsedTime)
        {
            this.asset = asset;
            this.rectangle = rectangle;
            this.elapsedTime = elapsedTime;
            isActive = true;
        }

        //Methods
        /// <summary>
        /// Override for the draw method in the object class
        /// </summary>
        /// <param name="sb"></param>
        public override void Draw(SpriteBatch sb)
        {
            if (isActive == true)
            {
                base.Draw(sb);
            }
        }

        public override Rectangle CheckCollision(Player player, out GameObject obj)
        {
            if (rectangle.Intersects(player.Position) && isActive)
            {
                isActive = false;
                player.Score += 100;
            }
            return base.CheckCollision(player, out obj);
        }
    }
}
