using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Futuristic_Steve
{
    public delegate void OnButtonClickDelegate();

    /// <summary>
    /// Builds, monitors, and draws a customized Button
    /// </summary>
    public class Button
    {
        // Button specific fields
        private SpriteFont font;
        private MouseState prevMState;
        private string text;
        private Rectangle position; // Button position and size
        private Vector2 textLoc;
        private Texture2D buttonImg;
        private Color textColor;

        //creating the events for our delegate
        public event OnButtonClickDelegate OnLeftButtonClick;

        /// <summary>
        /// Create a new custom button
        /// </summary>
        /// <param name="device">The graphics device for this game - needed to create custom button textures.</param>
        /// <param name="position">Where to draw the button's top left corner</param>
        /// <param name="text">The text to draw on the button</param>
        /// <param name="font">The font to use when drawing the button text.</param>
        /// <param name="color">The color to make the button's texture.</param>
        public Button(GraphicsDevice device, Rectangle position, String text, SpriteFont font, Color color)
        {
            // Save copies/references to the info we'll need later
            this.font = font;
            this.position = position;
            this.text = text;

            // Figure out where on the button to draw it
            Vector2 textSize = font.MeasureString(text);
            textLoc = new Vector2(
                (position.X + position.Width / 2) - textSize.X / 2,
                (position.Y + position.Height / 2) - textSize.Y / 2
            );

            textColor = Color.Black;

            // Make a custom 2d texture for the button itself
            buttonImg = new Texture2D(device, position.Width, position.Height, false, SurfaceFormat.Color);
            int[] colorData = new int[buttonImg.Width * buttonImg.Height]; // an array to hold all the pixels of the texture
            Array.Fill<int>(colorData, (int)color.PackedValue); // fill the array with all the same color
            buttonImg.SetData<Int32>(colorData, 0, colorData.Length); // update the texture's data
        }

        /// <summary>
        /// Each frame, update its status if it's been clicked.
        /// </summary>
        public void Update()
        {
            //checks the mouse state
            MouseState mState = Mouse.GetState();
            if (mState.LeftButton == ButtonState.Released &&
                prevMState.LeftButton == ButtonState.Pressed &&
                position.Contains(mState.Position))
            {
                if (OnLeftButtonClick != null)
                {
                    // Call ALL methods attached to this button
                    OnLeftButtonClick();
                }
            }

            prevMState = mState;
        }

        /// <summary>
        /// Override the draw for Game1
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the button
            spriteBatch.Draw(buttonImg, position, Color.White);

            // Draw button text over the button
            spriteBatch.DrawString(font, text, textLoc, textColor);
        }

    }
}
