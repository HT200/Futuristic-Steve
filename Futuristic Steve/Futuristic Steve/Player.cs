using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Futuristic_Steve
{
    enum PlayerState
    {
        Grounded,
        Jump,
        Fall,
        Switch,
        Dead
    }

    /// <summary>
    /// <br>The player</br>
    /// <br>Written by Noah G.</br>
    /// </summary>
    class Player
    {
        //Constant Fields - Use these to adjust player properties
        //Player Movement
        private const float MoveSpeed = 7.5f;    //The player's left/right movement speed in px/frame, positive is right
        private const float JumpSpeed = 12f;    //The player's initial jump velocity in px/frame
        private const float GravityStrength = .6f;      //Accelaration due to gravity in px/frame^2, positive is down
        //    Animation - ___AnimFrames are the number of frames for the animation
        //              - ___AnimLength is the time (in seconds) that the animation lasts
        private const int GroundedAnimFrames = 4;
        private const double GroundedAnimSpeed = 1; //A multiplier representing how fast the running animation should play
        private const int JumpAnimFrames = 1;
        private const double JumpAnimLength = 0;
        private const int FallAnimFrames = 1;
        private const double FallAnimLength = 0;
        private const int SwitchAnimFrames = 4;
        private const double SwitchAnimLength = 0.3;
        private const int DeadAnimFrames = 1;
        private const double DeadAnimLength = 0;
        //    Other
        private const double scoreMultiplier = 0.05; //A multiplier for distance-based score gain

        //Fields
        Rectangle screenRect; //A rectangle representing the screen
        //    Player Drawing
        private PlayerState playerState; //The player's current state
        private PlayerState prevPlayerState; //The player's state on the previous frame, used to track state switches for animation
        private Texture2D texture; //The sprite sheet of the player
        private Texture2D offScreenTexture; //The sprite used to indicate the player's position while off the screen
        private Rectangle texturePosition; //Defines what part of the sprite sheet to draw from
        private int frame = 0; //Tracks the current frame of the animation (0-indexed)
                               //Needs to be initialized here so it can be incremented when walking rather than being set each time
                               //    Movement Properties
        private Rectangle position; //The player's bounding box on the screen
        private Vector2 velocity;   //Velocity in px/frame, +x is right, +y is down
        private float gravity;
        private bool canJump; //If the player can currently jump
        private bool canSwitch; //If the player can currently switch
        //    Game State Related
        private bool isAlive; //If the player is currently alive
        private double score; //The current score, which is stored in the player since that's where ProcessCollision is
        //    Timing
        private double elapsedTime; //The time since the game was started, in seconds
        private double timeSinceFrameSwitch; //Used to track the uneven timings for the walking animation
        private double timeSinceStateSwitch; //The time since the state changed most recently, used for animation.
                                             //MUST be reset whenever playerState is reset.

        //Properties
        /// <summary>
        /// The bounding box of the player
        /// X and Y are at the top left corner
        /// </summary>
        public Rectangle Position
        { get { return position; } }

        /// <summary>
        /// If the player is alive
        /// </summary>
        public bool IsAlive
        { get { return isAlive; } }

        /// <summary>
        /// The current score
        /// </summary>
        public int Score
        {
            get { return (int)score; }
            set { score = value; }
        }

        /// <summary>
        /// Constructs a player with the given texture, and sets up many fields with starting values
        /// </summary>
        /// <param name="texture">The player's texture</param>
        public Player(Texture2D texture, Texture2D offScreenTexture)
        {
            //Set up everything that doesn't need to be reset
            this.texture = texture;
            this.offScreenTexture = offScreenTexture;
            screenRect = new Rectangle(0, 0, 1280, 720);

            //Use Reset() to set up everything that will need to be reset when going from the main menu to the game
            Reset();
        }

        //Methods
        /// <summary>
        /// Resets values for the player
        /// </summary>
        public void Reset()
        {
            playerState = PlayerState.Grounded;
            prevPlayerState = playerState;
            texturePosition = new Rectangle(0, 0, 40, 80);
            position = new Rectangle(0, (int)screenRect.Height - texturePosition.Height, texturePosition.Width, texturePosition.Height);
            isAlive = true;
            velocity = new Vector2(0, 0);
            gravity = GravityStrength;
            elapsedTime = 0;
            timeSinceStateSwitch = 0;
            score = 0;
        }

        public void Update(GameTime gameTime, KeyboardState kbState, KeyboardState preKBState)
        {
            //Update the player's position based on their velocity
            position.Location += velocity.ToPoint();

            if (playerState != PlayerState.Dead) //Most physics should only be done while alive
            {
                //Update Y velocity via gravity
                velocity.Y += gravity;

                //Handle player input
                //Horizontal movement (Left/Right)
                //Set the horizontal velocity based on the left/right keys
                //Left+Right cancels
                velocity.X = 0;
                if (kbState.IsKeyDown(Keys.Left))
                {
                    velocity.X -= MoveSpeed;
                }

                if (kbState.IsKeyDown(Keys.Right))
                {
                    velocity.X += MoveSpeed;
                }

                //Jumping (Z)
                if (SingleKeyPress(Keys.Z, kbState, preKBState) && canJump)
                {
                    //Change the Y velocity based on jumpSpeed and the sign of gravity
                    //Multiplied by -1 since up is negative
                    velocity.Y = JumpSpeed * -1 * (Math.Sign(gravity));

                    //The player can no longer jump until they touch the ground
                    canJump = false;
                    playerState = PlayerState.Jump;
                }

                //Gravity switching (X)
                if (SingleKeyPress(Keys.X, kbState, preKBState) && canSwitch)
                {
                    //Negate the gravity
                    gravity *= -1;

                    //The player can no longer switch gravity until they touch the ground
                    canSwitch = false;
                    playerState = PlayerState.Switch;
                }

                //Check if the player is grounded using canJump and canSwitch
                if (canJump && canSwitch)
                {
                    playerState = PlayerState.Grounded;
                }
                else //If the player is airborne
                {
                    //Check if Y velocity is either 0 or the same sign as gravity, and use it to decide if the player should move from Jump to Fall
                    //OR
                    //Check if the gravity switch animation is finished, and if so , switch to Fall
                    if ((Math.Sign(velocity.Y) * Math.Sign(gravity) >= 0 && playerState == PlayerState.Jump) || (playerState == PlayerState.Switch && frame == SwitchAnimFrames - 1))
                    {
                        playerState = PlayerState.Fall;
                    }
                }

                //Check if the state has switched
                if (playerState != prevPlayerState)
                {
                    //reset the timeSince___ variables, and reset frame in case it switched to Grounded
                    timeSinceFrameSwitch = 0;
                    timeSinceStateSwitch = 0;
                    frame = 0;
                }

                //Kill the player if they go off the top, bottom or left of the screen.
                if ((position.Y > screenRect.Width && gravity > 0) || (position.Y + position.Height < 0 && gravity < 0) || position.X + position.Width < 0)
                {
                    playerState = PlayerState.Dead;
                }

                //Update prevPlayerState
                prevPlayerState = playerState;

                //Update timing and score variables
                elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
                timeSinceStateSwitch += gameTime.ElapsedGameTime.TotalSeconds;
                timeSinceFrameSwitch += gameTime.ElapsedGameTime.TotalSeconds;
                score += ScrollingSpeed(elapsedTime) * scoreMultiplier;
            }
            else //if dead, only be affected by gravity and ignore input and collision
            {
                velocity.Y += gravity;

                //Check if the player is off the screen, and if so, set isAlive to false
                if (!screenRect.Contains(position))
                {
                    isAlive = false;
                }
            }
        }

        public void Draw(SpriteBatch sb, GameTime gameTime, SpriteFont font)
        {
            //Check if the player is within the screen vertically
            if (screenRect.Intersects(position))
            {
                //Determine the player's horizontal velocity relative to the platforms
                double relativeXVel = velocity.X + ScrollingSpeed(elapsedTime);

                //Determine texturePosition using playerState, the time since the last state switch, and all the animation constants.
                switch (playerState)
                {
                    case PlayerState.Grounded:  //Loops as long as the player is moving, with timing depending on the player's relative velocity
                                                //calculate current frame
                                                //Increment the frame if it's been the required amount of time between frames, 
                        if (relativeXVel > 0) //if the player is moving right relative to the platforms
                        {
                            while (timeSinceFrameSwitch > 1 / (relativeXVel * GroundedAnimSpeed))
                            {
                                frame++;
                                timeSinceFrameSwitch -= 1 / (relativeXVel * GroundedAnimSpeed);
                            }
                        }
                        else if (relativeXVel < 0) //if the player is moving left relative to the platforms
                        {
                            while (timeSinceFrameSwitch > -1 / (relativeXVel * GroundedAnimSpeed))
                            {
                                frame++;
                                timeSinceFrameSwitch -= -1 / (relativeXVel * GroundedAnimSpeed);
                            }
                        }
                        else //if relativeXVel = 0, stand still
                        {
                            frame = 0;
                        }

                        //Make sure the frame is valid
                        frame = frame % GroundedAnimFrames;

                        texturePosition = new Rectangle(frame * 40, 0, 40, 80);
                        break;
                    case PlayerState.Jump:  //Plays once then stops on last frame
                        frame = Math.Min((int)(timeSinceStateSwitch / JumpAnimLength * JumpAnimFrames), JumpAnimFrames - 1);
                        texturePosition = new Rectangle(frame * 40, 80, 40, 80);
                        break;
                    case PlayerState.Fall:  //Plays once then stops on last frame
                        frame = Math.Min((int)(timeSinceStateSwitch / FallAnimLength * FallAnimFrames), FallAnimFrames - 1);
                        texturePosition = new Rectangle(frame * 40, 160, 40, 80);
                        break;
                    case PlayerState.Switch:    //Plays once, stops on last frame
                        frame = Math.Min((int)(timeSinceStateSwitch / SwitchAnimLength * SwitchAnimFrames), SwitchAnimFrames - 1);
                        texturePosition = new Rectangle(frame * 40, 240, 40, 80);
                        break;
                    case PlayerState.Dead:  //Plays once then stops on last frame
                        frame = Math.Min((int)(timeSinceStateSwitch / DeadAnimLength * DeadAnimFrames), DeadAnimFrames - 1);
                        texturePosition = new Rectangle(frame * 40, 320, 40, 80);
                        break;
                }


                //Draw the texture, and determine the flips depending on the gravity and the player's relative velocity.
                if (gravity > 0)
                {
                    if (relativeXVel >= 0)
                    {
                        sb.Draw(texture, position, texturePosition, Color.White);
                    }
                    else
                    {
                        sb.Draw(texture, position, texturePosition, Color.White, 0, new Vector2(0, 0), SpriteEffects.FlipHorizontally, 0);
                    }
                }
                else
                {
                    if (relativeXVel >= 0)
                    {
                        sb.Draw(texture, position, texturePosition, Color.White, 0, new Vector2(0, 0), SpriteEffects.FlipVertically, 0);
                    }
                    else
                    {
                        sb.Draw(texture, position, texturePosition, Color.White, (float)Math.PI, new Vector2(40, 80), SpriteEffects.None, 0);
                    }
                }
            }
            else
            {
                if (position.Y < screenRect.Height / 2) //If the player is above the center of the screen
                { //Draw the offscreen indicator at the top of the screen
                    sb.Draw(offScreenTexture,
                        new Vector2(position.Center.X - (offScreenTexture.Bounds.Center.X), 0),
                        null,
                        Color.White,
                        0,
                        new Vector2(offScreenTexture.Width / 2, 0),
                        (float)(1 - Math.Min(0.6 * Math.Sqrt(Math.Abs(position.Y - screenRect.Height / 2) / (screenRect.Height / 8)) - 1, 0.9)),
                        SpriteEffects.None,
                        1);
                }
                else //If the player is below the center of the screen
                { //Draw the offscreen indicator at the bottom of the screen and flipped
                    sb.Draw(offScreenTexture,
                        new Vector2(position.Center.X - (offScreenTexture.Bounds.Center.X), screenRect.Height - offScreenTexture.Height),
                        null,
                        Color.White,
                        0,
                        new Vector2(offScreenTexture.Width / 2, 0),
                        (float)(1-Math.Min(0.6*Math.Sqrt(Math.Abs(position.Y - screenRect.Height / 2) / (screenRect.Height / 8)) - 1, 0.9)),
                        SpriteEffects.FlipVertically,
                        0);
                }
            }

            //Draw the score
            sb.DrawString(font, "Score: " + Score, new Vector2(20, 20), Color.SkyBlue);
        }

        /// <summary>
        /// <br>Processes collision with an object</br>
        /// <br>Inputs should be gotten from Object.CheckCollision()</br>
        /// </summary>
        /// <param name="collidedRect">The bounding box of the collided object</param>
        /// <param name="collidedObj">The collided object</param>
        public void ProcessCollision(Rectangle collidedRect, GameObject collidedObj)
        {
            //Only process collision if there is a collision, which will cause the method to get a non-null input for collidedRect,
            //and if the player is alive
            if (collidedObj != null && playerState != PlayerState.Dead)
            {

                //Determine what type collidedObj is
                if (collidedObj is Platform)
                {
                    //If collidedObj is a platform, collide with it
                    //Determine the collision side
                    //For reference, the player is 40x80, and the platform is 40x40

                    //Get the angle of the displacement vector from the platform to the player (clockwise from +X),
                    //represented by a number from 0 to 1
                    double direction; //The angle of the vector from the center of collidedRect to the center of position

                    double xDisp = position.Center.X - collidedRect.Center.X;
                    double yDisp = position.Center.Y - collidedRect.Center.Y;

                    if (xDisp != 0)
                    {
                        direction = (Math.Atan(yDisp / xDisp) / (Math.PI * 2)) + 0.5 + (0.25 * Math.Sign(xDisp));
                    }
                    else
                    {
                        //If the X displacement is 0, get the angle using the Y displacement instead of dividing by 0
                        if (yDisp >= 0)
                        {
                            direction = 0.25;
                        }
                        else
                        {
                            direction = 0.75;
                        }
                    }
                    if (direction < 0)
                    { direction++; }
                    if (direction >= 1)
                    { direction--; }


                    //Create a constant of the angle where the lower right of the platform and the upper left of the player are touching, to use for reference.
                    double cornerTouchYDisp = position.Height + collidedRect.Height;
                    double cornerTouchXDisp = -position.Width - collidedRect.Width;
                    double cornerTouchAngle = (Math.Atan(cornerTouchYDisp / cornerTouchXDisp) / (Math.PI * 2)) + 0.5 + (0.25 * Math.Sign(cornerTouchXDisp));

                    /*Angle Reference - CTA is cornerTouchAngle - Angles not to scale
                     *.5-CTA   .5+CTA
                     * \         /
                     *  \       /
                     *   \     /
                     *    \   /
                     *     \ /
                     *     /|\
                     *    / | \
                     *   /  |  \
                     *  /   |   \
                     * /    |    \
                     *CTA  0,1  1-CTA
                     */

                    //Check for up, then down, then left, then right
                    if (0.5 - cornerTouchAngle < direction && direction <= 0.5 + cornerTouchAngle)
                    {
                        //Collide with top edge
                        if (velocity.Y >= 0)
                        {
                            velocity.Y = 0;
                            position.Y = collidedRect.Y - position.Height;

                            //Check gravity, and recharge jump and switch if applicable
                            if (gravity > 0)
                            {
                                canJump = true;
                                canSwitch = true;
                            }
                        }
                    }
                    else if ((0 <= direction && direction <= cornerTouchAngle) || (1 - cornerTouchAngle < direction && direction <= 1))
                    {
                        //Collide with bottom edge
                        if (velocity.Y <= 0)
                        {
                            velocity.Y = 0;
                            position.Y = collidedRect.Y + collidedRect.Height;

                            //Check gravity, and recharge jump and switch if applicable
                            if (gravity < 0)
                            {
                                canJump = true;
                                canSwitch = true;
                            }
                        }
                    }
                    else //Colliding with left and right edges is determined without the angle
                    {
                        //Check the X displacement to determine the collision side
                        if (xDisp < 0)
                        {
                            //Collide with left edge
                            if (velocity.X >= 0)
                            {
                                velocity.X = 0;
                                position.X = collidedRect.X - position.Width;
                            }
                        }
                        else
                        {
                            //Collide with right edge
                            if (velocity.X <= 0)
                            {
                                velocity.X = 0;
                                position.X = collidedRect.X + collidedRect.Width;
                            }
                        }
                    }
                }
                else if (collidedObj is Hazard)
                {
                    //If collidedObj is a spike, die
                    playerState = PlayerState.Dead;
                    velocity.Y = -20 * Math.Sign(gravity);
                }
                //Don't need to do anything special for Pickup
            }
        }

        /// <summary>
        /// Checks if a key was just pressed
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <param name="kbState">The current keyboard state</param>
        /// <param name="preKBState">The keyboard state of the previous frame</param>
        /// <returns>True if the key is down in kbState and up in preKBState, false otherwise</returns>
        private bool SingleKeyPress(Keys key, KeyboardState kbState, KeyboardState preKBState)
        {
            return kbState.IsKeyDown(key) && preKBState.IsKeyUp(key);
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
