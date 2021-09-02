using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Futuristic_Steve
{
    abstract class GameObject
    {
        protected Texture2D asset;
        protected Rectangle rectangle;

        /// <summary>
        /// Object scrolling speed
        /// </summary>
        protected double scrollingSpeed;
        protected double elapsedTime;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="asset">Sprite of a object</param>
        /// <param name="rectangle">Rectangle object of the current sprite</param>
        protected GameObject(Texture2D asset, Rectangle rectangle, double elapsedTime)
        {
            this.asset = asset;
            this.rectangle = rectangle;
            this.elapsedTime = elapsedTime;
        }

        public int XPos
        {
            get { return rectangle.X; }
        }

        /// <summary>
        /// Draw the object
        /// </summary>
        /// <param name="sb">SpriteBatch object</param>
        public virtual void Draw(SpriteBatch sb)
        {
            sb.Draw(asset, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height), Color.White);
        }


        /// <summary>
        /// Update object
        /// </summary>
        /// <param name="gameTime">GameTime object</param>
        public virtual void Update(GameTime gameTime, Player player)
        {
            elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;

            rectangle.X -= (int)Math.Round((ScrollingSpeed(elapsedTime)));
            Rectangle collisionRect = CheckCollision(player, out GameObject obj);
            player.ProcessCollision(collisionRect, obj);
        }


        /// <summary>
        /// Check if the current object is colliding with player. Return specific values depending on if they collide or not.
        /// </summary>
        /// <param name="player">Object being checked for collision. Mostly the player</param>
        /// <param name="obj">The object being collided with. Returns null if there is no collision.</param>
        /// <returns>Collided object's rectangle</returns>
        public virtual Rectangle CheckCollision(Player player, out GameObject obj)
        {
            // If rectangle objects from both object collides
            if (rectangle.Intersects(player.Position))
            {
                obj = this;
            }
            else { obj = null; }
            return this.rectangle;
        }

        /// <summary>
        /// Gets the current scrolling speed from the elapsed time
        /// </summary>
        /// <param name="elapsedTime">The time since the start of the game, in seconds</param>
        /// <returns>The current scrolling speed, in pixels/frame</returns>
        private float ScrollingSpeed(double elapsedTime)
        {
            const float speedCoefficient = 1;
            const float timeCoefficient = 1;
            const float maxSpeed = 25;
            const double speedBase = 0.991;

            return (float)Math.Min(speedCoefficient * Math.Sqrt(timeCoefficient * elapsedTime), maxSpeed);
            // return (float)(maxSpeed * (1 - Math.Pow(speedBase, timeCoefficient * elapsedTime)));
        }
    }
}
