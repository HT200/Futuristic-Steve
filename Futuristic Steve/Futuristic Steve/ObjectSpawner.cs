using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Futuristic_Steve
{
    /// <summary>
    /// Spawns all the collidable objects
    /// </summary>
    class ObjectSpawner
    {

        private StreamReader reader;
        private string fileName;

        private Texture2D platformTexture;
        private Texture2D hazardTexture;
        private Texture2D collectibleTexture;

        private int width;
        private float chunkWidth;

        private double elapsedTime;

        private List<GameObject> objects;

        private List<int[]> gameChunkSize;
        private List<char[,]> gameChunks;

        public int ChunkWidth(int chunk)
        {
            return gameChunks[chunk].GetLength(0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="platformTexture"></param>
        /// <param name="hazardTexture"></param>
        /// <param name="collectibleTexture"></param>
        public ObjectSpawner(Texture2D platformTexture, Texture2D hazardTexture, Texture2D collectibleTexture, int width)
        {
            reader = null;

            this.platformTexture = platformTexture;
            this.hazardTexture = hazardTexture;
            this.collectibleTexture = collectibleTexture;

            gameChunkSize = new List<int[]>();
            gameChunks = new List<char[,]>();
            objects = new List<GameObject>();

            this.width = width;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        public void LoadFile(string fileName)
        {
            this.fileName = fileName;
            try
            {
                reader = new StreamReader(fileName);

                string line = null;

                int lineNumber = 0;

                int width;
                int height;

                int chunkNumber = -1;

                while ((line = reader.ReadLine()) != null)
                {
                    switch(lineNumber % 19)
                    {
                        case 0:
                            String[] chunckSize = line.Split(',');

                            width = int.Parse(chunckSize[0]);
                            height = int.Parse(chunckSize[1]);

                            int[] size = new int[] { width, height };
                            char[,] chunks = new char[width, height];

                            gameChunkSize.Add(size);
                            gameChunks.Add(chunks);

                            chunkNumber++;
                            break;
                        default:
                            int i = 0;
                            foreach(char c in line)
                            {
                                gameChunks[chunkNumber][i, (lineNumber % 19) - 1] = c;
                                i++;
                            }
                            break;
                    }
                    lineNumber++;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            reader.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadNextChunk(int chunkNumber, double elapsedTime, bool startChunk)
        {
            chunkWidth = gameChunks[chunkNumber].GetLength(0) * 40;
            int displacement;
            if (startChunk)
            {
                displacement = 0;
                chunkWidth -= width;
            }
            else
            {
                displacement = width;
            }
            for (int i = 0; i < gameChunks[chunkNumber].GetLength(0); i++)
            { 
                for (int j = 0; j < gameChunks[chunkNumber].GetLength(1); j++)
                {
                    if (gameChunks[chunkNumber][i,j] == 'P')
                    {
                        objects.Add(new Platform(platformTexture, new Rectangle(displacement + (40 * (i)) - 1, (40 * j) - 1, 42, 42), elapsedTime));
                    }
                    else if (gameChunks[chunkNumber][i, j] == 'S')
                    {
                        objects.Add(new Hazard(hazardTexture, new Rectangle(displacement + (40 * (i)), 40 * j, 40, 40), elapsedTime));
                    }
                    else if (gameChunks[chunkNumber][i, j] == 'C')
                    {
                        objects.Add(new Pickup(collectibleTexture, new Rectangle(displacement + (40 * (i)), 40 * j, 40, 40), elapsedTime));
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sb"></param>
        public void Draw(SpriteBatch sb)
        {
            foreach (GameObject o in objects)
            {
                o.Draw(sb);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="player"></param>
        public void Update(GameTime gameTime, Player player, Random rng)
        {
            elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                objects[i].Update(gameTime, player);
                if (objects[i].XPos <= -100)
                {
                    objects.RemoveAt(i);
                }
            }

            chunkWidth -= ScrollingSpeed(elapsedTime);
            if (chunkWidth <= 0)
            {
                LoadNextChunk(rng.Next(1, gameChunks.Count), elapsedTime, false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chunkNumber"></param>
        public void Reset(int chunkNumber)
        {
            objects.Clear();
            elapsedTime = 0;
            LoadNextChunk(chunkNumber, elapsedTime, true);
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

            return (float)Math.Min(speedCoefficient * Math.Sqrt(timeCoefficient * elapsedTime), maxSpeed);
        }
    }
}
