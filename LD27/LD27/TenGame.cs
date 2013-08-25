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
        Texture2D flatTextures;
        Dictionary<string, Texture2D> Textures;

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
        public static GraphicsDevice GetGraphicsDevice()
        {
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
            Textures = new Dictionary<string, Texture2D>();
            spriteBatch = new SpriteBatch(GraphicsDevice);

            worldMap = new WorldMap(GraphicsDevice, Content);

            engine = new Engine(GraphicsDevice, Content) { WorldMap = worldMap };
            engine.LoadContent();

            flatTextures = Content.Load<Texture2D>("misctiles");
            Textures.Add("losescreen", Content.Load<Texture2D>("endscreen_bad"));


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
            int screenw = GraphicsDevice.Viewport.Width;
            int screenh = GraphicsDevice.Viewport.Height;
            Creature player = worldMap.Player; 

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            KeyboardState kbdState = Keyboard.GetState();

            float xshift = 0;
            float yshift = 0;
            float zshift = 0;
            if (kbdState.IsKeyDown(Keys.Up)) {
                yshift += player.Speed/100f;
                player.Direction = -Math.PI/2;
            }

            if (kbdState.IsKeyDown(Keys.Down))
            {
                yshift -= player.Speed / 100f;
                player.Direction = Math.PI / 2;
            }
            if (kbdState.IsKeyDown(Keys.Left))
            {
                xshift -= player.Speed / 100f;
                player.Direction = Math.PI;
            }

            if (kbdState.IsKeyDown(Keys.Right))
            {
                xshift += player.Speed / 100f;
                player.Direction = 0;
            }


            if (kbdState.IsKeyDown(Keys.PageUp))
            {
                zshift -= 0.05f;
            }

            if (kbdState.IsKeyDown(Keys.PageDown))
            {
                zshift += 0.05f;
            }

            if (kbdState.IsKeyDown(Keys.Z)) 
            {
                var loc = player.GetMoveLocation(AdjustVector2(worldMap.Viewport, screenw / 2, screenh / 2), player.Direction, player.Range);
                worldMap.Player.Set("attacking", 0.5f);
                worldMap.Forces.Add(new Attack()
                {
                    Visual = Force.Visuals.Test,
                    Creator = worldMap.Player,
                    WorldMap = worldMap,
                    Damage = player.Attack,
                    Location = loc,
                    Range = player.Range,
                    
                });
            }

            if (kbdState.IsKeyDown(Keys.X))
            {
                worldMap.Player.Set("attacking", 0.5f);
                worldMap.Forces.Add(new Attack()
                {
                    Visual = Force.Visuals.Test2,
                    Creator = worldMap.Player,
                    WorldMap = worldMap,
                    Damage = 10 * player.Attack,
                    Location = AdjustVector2(worldMap.Viewport, screenw/2, screenh/2),
                    Range = player.Range,
                    Duration = 10
                });
            }

            if (kbdState.IsKeyDown(Keys.Enter)) {
                if (player.Health <= 0)
                {
                    //player.Health = 100;
                    worldMap = new WorldMap(GraphicsDevice, Content);

                }
            }


                float aspect = GraphicsDevice.Viewport.AspectRatio;
            

            float targetX = engine.Camera.X + xshift;
            float targetY = engine.Camera.Y + yshift;
            var xpixels = (targetX/aspect)*screenw/2;
            var ypixels = (targetY) * screenh / 2;


            var worldMapTarget = new Vector2(worldMap.X + xpixels, worldMap.Y - ypixels);

            if (worldMap.IsValidPath(worldMap.Viewport, worldMapTarget, (screenw/2)-32, (screenh/2)-32)) {

                engine.Camera = new Vector3(engine.Camera.X + xshift, engine.Camera.Y + yshift, engine.Camera.Z + zshift);
                engine.Target = new Vector3(engine.Target.X + xshift, engine.Target.Y + yshift, -1);

                worldMap.Viewport = worldMapTarget;

                // keep player  at the center of the screen.
                worldMap.Player.Location = worldMap.Viewport;
            }

            base.Update(gameTime);
            engine.Update(gameTime);            
        }

        public static Vector2 AdjustVector2(Vector2 vector2, int p1, int p2)
        {
            return new Vector2(vector2.X + p1, vector2.Y + p2);
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

            if (worldMap.Player.Health <= 0) {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                spriteBatch.GraphicsDevice.SamplerStates[0].Filter = TextureFilter.Point;
                spriteBatch.Draw(Textures["losescreen"],
                    new Rectangle(0,0,GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), 
                    new Rectangle(0,0,320,200), Color.White);
                spriteBatch.End();
            }
            else
            {
                engine.Draw(GraphicsDevice, gameTime);

                spriteBatch.Begin();
                spriteBatch.Draw(flatTextures,
                    new Rectangle(3, 3, 8, (int)(GraphicsDevice.Viewport.Height - 6) * (int)worldMap.Player.Health / 100), new Rectangle(0, 0, 64, 64), Color.White, 0, Vector2.Zero,
                    SpriteEffects.None, 0);
                spriteBatch.End();
            }
            base.Draw(gameTime);
        }
    }
}
