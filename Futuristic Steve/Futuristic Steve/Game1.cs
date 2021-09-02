using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

//REMEMBER TO FIX THE NAMESPACE WHEN MAKING NEW CLASSES
namespace Futuristic_Steve
{
    /// <summary>
    /// The state of the <b>entire</b> game
    /// </summary>
    enum GameState
    {
        MainMenu,
        Gameplay,
        Pause,
        GameOver
    }

    enum SaveHighScore
    {
        NameInput,
        SaveScores
    }

    public class Game1 : Game
    {
        bool delete = false;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private KeyboardState kbState;
        private KeyboardState previousKbState;
        private GameState gameState;
        private Player player;

        private ObjectSpawner objectSpawner;

        //Loaded content
        private Texture2D mainMenuTexture;
        private Texture2D backgroundTexture;
        private Texture2D newHighScoreTexture;
        private Texture2D gameoverTexture;
        private Texture2D playerTexture;
        private Texture2D platformTexture;
        private Texture2D hazardTexture;
        private Texture2D pickupTexture;
        private Texture2D offScreenIndicator;
        private SpriteFont menuFont;
        private SpriteFont highScoreFont;

        private Random rng;

        private SortedScores highScores;
        private string newScoreName;
        private SaveHighScore saveHS;

        //Button Content
        private Button menuButton;
        private Button pauseButton;
        private Button endButton;
        private SpriteFont font;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            gameState = GameState.MainMenu;

            Window.TextInput += TextInputHandler;

            //Set the game resolution
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();

            rng = new Random();

            saveHS = SaveHighScore.SaveScores;
            highScores = new SortedScores();
            newScoreName = "";

            //Button content
            font = Content.Load<SpriteFont>("buttonFont");//loads the button font

            /*menuButton = new Button(
                    _graphics.GraphicsDevice,           // create a custom texture
                    new Rectangle((_graphics.PreferredBackBufferWidth - 200) / 2,   //position of x-axis
                    (_graphics.PreferredBackBufferHeight - 100) / 2,                //position of y-axis 
                    200, 100),                                                      // size of button
                    "Press Enter to Play",              // button label
                    font,                               // label font
                    Color.SaddleBrown);*/                 // button color
            pauseButton = new Button(
                  _graphics.GraphicsDevice,           // create a custom texture
                  new Rectangle((_graphics.PreferredBackBufferWidth - 200) / 2,     //position of x-axis
                  (_graphics.PreferredBackBufferHeight - 100) / 2,                  //position of y-axis 
                  200, 100),                                                        // size of button
                  "Resume Game",                      // button label
                  font,                               // label font
                  Color.SaddleBrown);                 // button color
            endButton = new Button(
                  _graphics.GraphicsDevice,           // create a custom texture
                  new Rectangle((_graphics.PreferredBackBufferWidth - 200) / 2,   //position of x-axis
                  ((_graphics.PreferredBackBufferHeight - 100) / 8) * 7,                //position of y-axis 
                  200, 100),                                                      // size of button
                  "Play again",                       // button label
                  font,                               // label font
                  Color.SaddleBrown);                 // button color

            //menuButton.OnLeftButtonClick += this.NextState;//if the button is clicked we go to the method
            pauseButton.OnLeftButtonClick += this.NextState;//if the button is clicked we go to the method
            endButton.OnLeftButtonClick += this.NextState;//if the button is clicked we go to the method

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            //Loading textures
            mainMenuTexture = this.Content.Load<Texture2D>("MainMenu2");
            backgroundTexture = this.Content.Load<Texture2D>("Background");
            gameoverTexture = this.Content.Load<Texture2D>("Game_Over");
            playerTexture = this.Content.Load<Texture2D>("SpriteSheetTemplate");
            newHighScoreTexture = this.Content.Load<Texture2D>("NewHighScore");
            platformTexture = this.Content.Load<Texture2D>("Platform_V1");
            hazardTexture = this.Content.Load<Texture2D>("Spike_V4");
            pickupTexture = this.Content.Load<Texture2D>("Digital_Coin");
            offScreenIndicator = this.Content.Load<Texture2D>("GreenArrow");
            menuFont = this.Content.Load<SpriteFont>("MenuFont");
            highScoreFont = this.Content.Load<SpriteFont>("HighScoreFont");

            objectSpawner = new ObjectSpawner(platformTexture,
                hazardTexture,
                pickupTexture,
                _graphics.PreferredBackBufferWidth);

            objectSpawner.LoadFile("Content/GameChunks.txt");
            
            //Initializing things that require loaded files
            player = new Player(playerTexture, offScreenIndicator);

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            //Get current keyboard input
            kbState = Keyboard.GetState();

            //State changing
            switch (gameState)
            {
                case GameState.MainMenu:
                    //Call the menuButtons update() method
                    //menuButton.Update();
                    //Call object Update() methods

                    //Switch states
                    if (SingleKeyPress(Keys.Enter))
                    {
                        player.Reset();
                        objectSpawner.Reset(0);
                        objectSpawner.Update(gameTime, player, rng);
                        gameState = GameState.Gameplay;
                        newScoreName = "";
                        LoadHighScores("..\\..\\..\\HighScores.txt");
                    }
                    break;

                case GameState.Gameplay:
                    //call the gameButtons update() method
                    pauseButton.Update();
                    //Call object Update() methods
                    player.Update(gameTime, kbState, previousKbState);

                    objectSpawner.Update(gameTime, player, rng);
                    //Switch states
                    if (!player.IsAlive)
                    {
                        gameState = GameState.GameOver;
                        if (highScores.Count < 10 || player.Score >= highScores.Min().ScoreNumber)
                        {
                            saveHS = SaveHighScore.NameInput;
                        }
                    }
                    //Press 'P' to go to the Pause state
                    if (SingleKeyPress(Keys.P))
                    {
                        gameState = GameState.Pause;
                    }
                    break;

                case GameState.Pause:
                    //call the gameButtons update() method
                    pauseButton.Update();

                    //Press 'P' to go to the GamePlay state
                    if (SingleKeyPress(Keys.P))
                    {
                        gameState = GameState.Gameplay;
                    }
                    break;

                case GameState.GameOver:
                    //Call the endButtons update() method
                    endButton.Update();
                    //Call object Update() methods

                    switch (saveHS)
                    {
                        case SaveHighScore.NameInput:
                            if(SingleKeyPress(Keys.Enter))
                            {
                                saveHS = SaveHighScore.SaveScores;
                                highScores.Add(new Score(newScoreName, player.Score));
                            }
                            break;
                        case SaveHighScore.SaveScores:
                            
                            if (SingleKeyPress(Keys.Enter))
                            {
                                gameState = GameState.MainMenu;
                                SaveHighScores("..\\..\\..\\HighScores.txt");
                            }
                            break;
                    }
                    //Switch states
                    break;
            }

            previousKbState = kbState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // TODO: Add your drawing code here
            _spriteBatch.Begin();

            switch (gameState)
            {
                case GameState.MainMenu:
                    //Draw the button
                    _spriteBatch.Draw(mainMenuTexture, new Rectangle(0, 0, 1280, 720), Color.White);
                    //menuButton.Draw(_spriteBatch);

                    //Call object Draw() methods

                    //Fill with background color
                    GraphicsDevice.Clear(Color.CornflowerBlue);
                    break;

                case GameState.Gameplay:
                    //Call object Draw() methods
                    _spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, 1280, 720), Color.White);
                    objectSpawner.Draw(_spriteBatch);
                    player.Draw(_spriteBatch, gameTime, menuFont);

                    //Fill with background color
                    break;

                case GameState.Pause:
                    //Draw the Button
                    _spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, 1280, 720), Color.White);
                    pauseButton.Draw(_spriteBatch);

                    //Call object Draw() methods
                    player.Draw(_spriteBatch, gameTime, menuFont);
                    objectSpawner.Draw(_spriteBatch);

                    //Fill with background color
                    GraphicsDevice.Clear(Color.White);
                    break;

                case GameState.GameOver:
                    GraphicsDevice.Clear(Color.Black);
                    switch (saveHS)
                    {
                        case SaveHighScore.NameInput:
                            _spriteBatch.Draw(newHighScoreTexture, new Rectangle(0, 0, 1280, 720), Color.White);
                            _spriteBatch.DrawString(highScoreFont,
                                newScoreName,
                                new Vector2(510, (_graphics.PreferredBackBufferHeight / 2) + 277),
                                Color.White);
                            break;
                        case SaveHighScore.SaveScores:
                            _spriteBatch.Draw(gameoverTexture, new Rectangle(0, 0, 1280, 720), Color.White);
                            for (int i = 0; i < highScores.Count; i++)
                            {
                                //Draw the button
                                //endButton.Draw(_spriteBatch);

                                _spriteBatch.DrawString(menuFont,
                                    String.Format("{0}. {1}", i + 1, highScores[i]),
                                    new Vector2((_graphics.PreferredBackBufferWidth / 2) - 35, 240 + (30 * i)),
                                    Color.White);

                                //Call object Draw() methods
                                //_spriteBatch.DrawString(menuFont, "Game Over: Press Enter to return to menu", new Vector2(100, 100), Color.Orange);
                            }
                            break;
                    }
                    
                    //Fill with background color
                    break;
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        //method for if the button is clicked
        public void NextState()
        {
            switch (gameState)
            {
                case GameState.MainMenu:
                    gameState = GameState.Gameplay;
                    break;

                case GameState.Pause:
                    gameState = GameState.Gameplay;
                    break;

                case GameState.GameOver:
                    gameState = GameState.MainMenu;
                    break;
            }
        }

        /// <summary>
        /// Get single key press
        /// </summary>
        /// <param name="key">The key to check</param> 
        /// <returns>True if the key was just pressed, false otherwise</returns>
        private bool SingleKeyPress(Keys key)
        {
            if (previousKbState != null)
            {
                if (previousKbState.IsKeyDown(key) && kbState.IsKeyUp(key))
                {
                    return true;
                }
                else return false;
            }
            else if (kbState.IsKeyDown(key) == true)
            {
                return true;
            }
            else return false;
        }

        private void LoadHighScores(string fileName)
        {
            StreamReader reader = null;

            highScores.Clear();

            try
            {
                reader = new StreamReader(fileName);

                string line = null;


                while ((line = reader.ReadLine()) != null)
                {
                    String[] loadedHighScores = line.Split(',');
                    String[] input = line.Split(',');

                    highScores.Add(new Score(input[0], int.Parse(input[1])));
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            reader.Close();
        }

        private void SaveHighScores(string fileName)
        {
            StreamWriter writer = null;

            File.WriteAllText("..\\..\\..\\HighScores.txt", String.Empty);
            try
            {
                writer = new StreamWriter(fileName);

                for (int i = 0; i < highScores.Count; i++)
                {
                    writer.WriteLine("{0}, {1}", highScores[i].Name, highScores[i].ScoreNumber);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            writer.Close();
        }

        private void TextInputHandler(object sender, TextInputEventArgs args)
        {
            var character = args.Character;
            var key = args.Key;


            if (saveHS == SaveHighScore.NameInput)
            {
                if (key == Keys.Back && newScoreName.Length > 0)
                {
                    newScoreName = newScoreName.Remove(newScoreName.Length - 1);
                }
                else if (key == Keys.Enter || (!highScoreFont.Characters.Contains(character) && character != '\r')) 
                { 
                    return; 
                }
                else
                {
                    newScoreName += character;
                }
            }
        }
    }
}
