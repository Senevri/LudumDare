#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
#endregion

namespace LD27
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class TenGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Engine engine;
        WorldMap worldMap;
        private static GraphicsDevice _graphicsDevice;

        public TenGame()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;
            TenGame._graphicsDevice = GraphicsDevice;
        }


        //FIXME ugly
        public static GraphicsDevice GetGraphicsDevice() {
            return TenGame._graphicsDevice;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            worldMap = new WorldMap(GraphicsDevice, Content);

            engine = new Engine(GraphicsDevice, Content) { WorldMap = worldMap};
            engine.LoadContent();

           
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            KeyboardState kbdState = Keyboard.GetState();

            float xshift = 0;
            float yshift = 0;
            float zshift = 0;
            if (kbdState.IsKeyDown(Keys.Up)) {
                yshift += 0.1f;
            }

            if (kbdState.IsKeyDown(Keys.Down))
            {
                yshift -= 0.1f;
            }
            if (kbdState.IsKeyDown(Keys.Left))
            {
                xshift -= 0.1f;
            }

            if (kbdState.IsKeyDown(Keys.Right))
            {
                xshift += 0.1f;
            }


            if (kbdState.IsKeyDown(Keys.PageUp))
            {
                zshift -= 0.05f;
            }

            if (kbdState.IsKeyDown(Keys.PageDown))
            {
                zshift += 0.05f;
            }


            //worldMap.Viewport = new Vector2(worldMap.Viewport.X + xshift, worldMap.Viewport.Y + yshift);

            engine.Camera = new Vector3(engine.Camera.X + xshift, engine.Camera.Y + yshift, engine.Camera.Z + zshift);
            engine.Target = new Vector3(engine.Target.X + xshift, engine.Target.Y + yshift, -1);
            int screenw = GraphicsDevice.Viewport.Width;
            int screenh = GraphicsDevice.Viewport.Height;
            float aspect = GraphicsDevice.Viewport.AspectRatio;

            // TODO: Add your update logic here            
            base.Update(gameTime);
            engine.Update(gameTime);
            //engine.MoveQuad("timer", xshift, yshift, zshift);

            var xpixels = (engine.Camera.X/aspect)*screenw/2;
            var ypixels = (engine.Camera.Y) * screenh / 2;
            
            worldMap.Viewport = new Vector2(worldMap.X + xpixels, worldMap.Y - ypixels);

            // keep player  at the center of the screen.
            worldMap.Player.Location = worldMap.Viewport;


        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

            int fps = 1000 / gameTime.ElapsedGameTime.Milliseconds;
            this.Window.Title = string.Format("LD27 (FPS: {0})", fps);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            engine.Draw(GraphicsDevice, gameTime);

            base.Draw(gameTime);
        }
    }
}
