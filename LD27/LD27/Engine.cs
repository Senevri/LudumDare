using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    class Engine
    {
        private Microsoft.Xna.Framework.Graphics.GraphicsDevice GraphicsDevice;
        private Microsoft.Xna.Framework.Matrix viewMatrix;
        private Microsoft.Xna.Framework.Matrix projectionMatrix;
        private Microsoft.Xna.Framework.Matrix worldMatrix;
        private VertexBuffer vertexBuffer;


        private List<PositionedQuad> storedQuads;

        private Dictionary<string, PositionedQuad> namedQuads;
        private List<PositionedQuad> renderQuads;
        private Microsoft.Xna.Framework.Content.ContentManager Content;
        private List<SpriteSheet> sprites;

        public Vector3 Camera { get; set; }

        public Vector3 Target { get; set; }

        public Dictionary<string, Texture2D> Textures { get; set; }

        public BasicEffect Effect { get; set; }

        public WorldMap WorldMap { get; set; }

        private float aspect;

        public Engine(Microsoft.Xna.Framework.Graphics.GraphicsDevice GraphicsDevice)
        {
            // TODO: Complete member initialization
            this.GraphicsDevice = GraphicsDevice;
            this.Initialize();
        }

        public Engine(Microsoft.Xna.Framework.Graphics.GraphicsDevice GraphicsDevice, Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            this.GraphicsDevice = GraphicsDevice;
            this.Content = Content;
            this.Initialize();
        }

        private List<PositionedQuad> textQuads;

        private void Initialize()
        {            
            this.renderQuads = new List<PositionedQuad>();
            this.storedQuads = new List<PositionedQuad>();
            this.textQuads = new List<PositionedQuad>();
            this.sprites = new List<SpriteSheet>();
            this.namedQuads = new Dictionary<string, PositionedQuad>();

            aspect = GraphicsDevice.Viewport.AspectRatio;

            CreateEffect();

            RasterizerState state = new RasterizerState();
            state.CullMode = CullMode.None;
            state.FillMode = FillMode.Solid;
            GraphicsDevice.RasterizerState = state;
            DepthStencilState depthBufferState = new DepthStencilState();
            depthBufferState.DepthBufferEnable = true;
            GraphicsDevice.DepthStencilState = depthBufferState;
            
            
            Textures = new Dictionary<string, Texture2D>();
            Camera = new Vector3(0, 0, 1);
            Target = new Vector3(0, 0, -1);
        }

        public Vector2 GetScreenUpperLeft() {
            //FIXME : not fully implemented
            return new Vector2(Camera.X, Camera.Y);
        }

        private void CreateEffect()
        {
            Effect = new BasicEffect(this.GraphicsDevice);
            Effect.EnableDefaultLighting();
            Effect.DirectionalLight0.Enabled = false;
            Effect.DirectionalLight1.Enabled = false;
            Effect.DirectionalLight2.Enabled = false;
        }

        private float  defaultScale = 64f / 480f;

        /**
         * LoadContent
         */
        internal void LoadContent()
        {
            //SetupEffect();            
            int screenw = GraphicsDevice.Viewport.Bounds.Width;
            int screenh = GraphicsDevice.Viewport.Bounds.Height;
            Textures.Add("test", Content.Load<Texture2D>("testTexture"));
            Textures.Add("bmpFont", Content.Load<Texture2D>("bmpFont"));
            Textures.Add("mapRender", WorldMap.GetMapImage());
            Textures.Add("playerspritesheet", Content.Load<Texture2D>("playerspritesheet"));
            Textures.Add("youmaspritesheet", Content.Load<Texture2D>("youmaspritesheet"));
            Textures.Add("tilesheet", Content.Load<Texture2D>("tilesheet"));
            Textures.Add("sfx", Content.Load<Texture2D>("specialeffects"));
            Effect.Texture = Textures["mapRender"];

            float testScale = 64f / (screenh);
            defaultScale = testScale;

            namedQuads.Add("map", new PositionedQuad(new TexturedQuad(aspect, 1)
            {
                Texture = Textures["mapRender"]
            }, new Vector2(Camera.X, Camera.Y)));

            

                //Write(string.Format("{0:0}", 0), screenw / 2, 32, new Vector3(0, 0, 0), 0.1f));

            storedQuads.Add(namedQuads["map"]);
            /*
            storedQuads.Add(new PositionedQuad(new TexturedQuad(defaultScale) { Texture = Textures["test"] }, PixelPositionToVector2(32, 32)));
            storedQuads.Add(new PositionedQuad(new TexturedQuad(defaultScale) { Texture = Textures["test"] }, PixelPositionToVector2(96, 32)));
            storedQuads.Add(new PositionedQuad(new TexturedQuad(defaultScale) { Texture = Textures["test"] }, PixelPositionToVector2(160, 32)));
            storedQuads.Add(new PositionedQuad(new TexturedQuad(defaultScale) { Texture = Textures["test"] }, PixelPositionToVector2(224, 32)));
            storedQuads.Add(new PositionedQuad(new TexturedQuad(defaultScale) { Texture = Textures["test"] }, PixelPositionToVector2(288, 32)) { Rotation = new Vector3(0, 0, (float)(Math.PI / 4)) });
            storedQuads.Add(new PositionedQuad(new TexturedQuad(defaultScale) { Texture = Textures["test"] }, new Vector2(0.5f, 0)) { Rotation = new Vector3(0, 0, (float)(-Math.PI / 4)) });
            storedQuads.Add(new PositionedQuad(new TexturedQuad(defaultScale) { Texture = Textures["test"] }, new Vector2(0.5f, 0.5f)));
            storedQuads.Add(new PositionedQuad(new TexturedQuad(defaultScale) { Texture = Textures["test"] }, PixelPositionToVector2(screenw - 32, screenh - 32)) { Rotation = new Vector3(0, 0, (float)(-Math.PI / 8)) });
             */
            //storedQuads.Add(namedQuads["timer"]);


            var player = AddSpriteSheet(Textures["playerspritesheet"], WorldMap.Player.Location);
            player.AnimationIndexes.Add("attack", Enumerable.Range(16, 5).ToArray());
            namedQuads.Add("player", player);

            namedQuads.Add("tiles", AddSpriteSheet(Textures["tilesheet"], Vector2.Zero, false, false));

            var enemies = AddSpriteSheet(Textures["youmaspritesheet"], Vector2.Zero, false, false);
            enemies.Delay = 0.5f;
            enemies.AnimationIndexes.Add("small", new int[] { 0, 1});
            enemies.AnimationIndexes.Add("medium", new int[] { 8, 9 });
            enemies.AnimationIndexes.Add("large", new int[] { 16, 17 });
            enemies.AnimationIndexes.Add("itcomes", new int[] { 24, 25, 26 });
            
            

            namedQuads.Add("enemies", enemies);

            var timer = AddSpriteSheet(Textures["bmpFont"], PixelPositionToVector2(screenw / 2, 32));
            namedQuads.Add("timer", timer);

            var sfx = AddSpriteSheet(Textures["sfx"], Vector2.Zero, false, false);
            sfx.Delay = 0.250f;
            sfx.AnimationIndexes.Add("row1", new int[] { 0, 1, 2 });
            sfx.AnimationIndexes.Add("row2", new int[] { 8, 9 });
            sfx.AnimationIndexes.Add("bloody", Enumerable.Range(16, 5).ToArray());
            namedQuads.Add("sfx", sfx);

            // TODO: animation generator
            // set texture to first frame;
            //timer.First(GraphicsDevice);
            
            timer.Current = 42;
            timer.SetTileToCurrent(GraphicsDevice);            
            timer.AnimationIndexes["timer"] = Enumerable.Range(32, 10).ToArray();
            System.Diagnostics.Debug.Assert(timer.AnimationIndexes["timer"].Contains(41));
            timer.Animation = "timer";
            timer.Delay = 1f;
            

            //storedQuads.AddRange(sprites);
        }
        

        private SpriteSheet AddSpriteSheet(Texture2D texture, Vector2 position, bool animated = true, bool show = true)
        {
            //spritesheet is by default a 8x8 grid, so.... 
            // 
            sprites.Add(new SpriteSheet(texture, GraphicsDevice, 8, 64, 64, aspect / 12.5f, 1 / 8f) { Show = show });
            sprites.Last().Texture = null;
            sprites.Last().isAnimated = animated;
            sprites.Last().AnimationIndexes.Add("test", new int[] { 0, 1 });
            sprites.Last().AnimationIndexes.Add("test1", new int[] { 9 });
            sprites.Last().Animation = "test";
            sprites.Last().Position = position;
            
            sprites.Last().First(GraphicsDevice);

            return sprites.Last();
        }

        internal Vector2 PixelPositionToVector2(int x, int y) {
            float fx = 0;
            float fy = 0;
            
            fx = (float)(aspect * ((2.0 * (double)x / (double)GraphicsDevice.Viewport.Bounds.Width)));
            fy = (float)(-2.0 * (double)y / (double)GraphicsDevice.Viewport.Bounds.Height);
            return new Vector2(fx-aspect, fy+1);

        }

        float newAngle = 0;

        internal void Draw(GraphicsDevice GraphicsDevice, Microsoft.Xna.Framework.GameTime gameTime)
        {
            int screenw = GraphicsDevice.Viewport.Bounds.Width;
            int screenh = GraphicsDevice.Viewport.Bounds.Height;            
            GraphicsDevice.SetVertexBuffer(null);
            List<VertexPositionNormalTexture> verts = new List<VertexPositionNormalTexture>();
            
            float seconds = (float)gameTime.TotalGameTime.TotalSeconds - prevSeconds;            

            //renderQuads.Add((namedQuads["timer"] as SpriteSheet));

            if (seconds > 10.0f)
            {
                prevSeconds = (float)gameTime.TotalGameTime.TotalSeconds;
            }
            delta = gameTime.TotalGameTime.TotalSeconds;

            var tiles = (namedQuads["tiles"] as SpriteSheet);
            tiles.Current = 8;
            tiles.SetTileToCurrent(GraphicsDevice);
            
            // get correct tile
            
            foreach (var portal in WorldMap.Portals) {
                
                var v1 = PixelPositionToVector2((int)(portal.Location.X-WorldMap.X), (int)(portal.Location.Y-WorldMap.Y));
                if (portal.isOpen) {
                    renderQuads.Add(new PositionedQuad(
                        new TexturedQuad(defaultScale) { Texture = tiles.Texture }, v1 ) { Show = true });                                
                }
            }

            renderQuads.AddRange(sprites);
            if (WorldMap.Player.Is("attacking")) {
                var player = (namedQuads["player"] as SpriteSheet);
                if (player.Animation.Equals("attack") && player.Current == player.AnimationIndexes["attack"].Last())
                {
                    WorldMap.Player.Set("attacking", 0);
                    player.Animation = "test";
                    player.Delay = 0.5f;
                }
                else {
                    player.Animation = "attack";
                    player.Delay = 0.05f;
                    //player.Delay = WorldMap.Player.Get("attacking");
                }
            }

            

            var enemies = (namedQuads["enemies"] as SpriteSheet);
            //enemies.Current = 0;
            //enemies.SetTileToCurrent(GraphicsDevice);            

            foreach (var enemy in WorldMap.Creatures) {
                var v1 = PixelPositionToVector2((int)(enemy.Location.X - WorldMap.X), (int)(enemy.Location.Y - WorldMap.Y));
                switch (enemy.Type) { 
                    case Creature.Types.PLAYER:
                        // do nothing as player is rendered separately.
                        break;

                    case Creature.Types.SMALL:
                        enemies.Animation = "small";
                        break;
                    case Creature.Types.MEDIUM:
                        enemies.Animation = "medium";
                        break;
                    case Creature.Types.LARGE:
                        enemies.Animation = "large";
                        break;
                    case Creature.Types.BEWARE:
                        enemies.Animation = "itcomes";
                        break;

                    default:
                 
                        break;
                }
                enemies.isAnimated = true;
                if (enemies.AllowNextFrame(gameTime.TotalGameTime.TotalSeconds))
                {
                    enemies.Next(enemies.Texture);
                }
                else
                {
                    enemies.SetTileToCurrent(GraphicsDevice);
                }
                /*Color[] bits = new Color[enemies.Texture.Width * enemies.Texture.Height];
                 *  var copy = new Texture2D(GraphicsDevice, enemies.Texture.Width, enemies.Texture.Height);
                 *  enemies.Texture.GetData<Color>(bits);                 
                 *  copy.SetData<Color>(bits);
                */
                renderQuads.Add(new PositionedQuad(
                 new TexturedQuad(defaultScale) { Texture = enemies.Texture }, v1) { Show = true });                                
            }

            var sfx = (namedQuads["sfx"] as SpriteSheet);
            sfx.isAnimated = true;
            //Console.WriteLine("sfx count: {0}, creature count: {1}", WorldMap.Forces.Count, WorldMap.Creatures.Count);
            foreach (var force in WorldMap.Forces) { 
                var v = PixelPositionToVector2((int)(force.Location.X - WorldMap.X), (int)(force.Location.Y - WorldMap.Y));
                sfx.Delay = 0.250f;
                switch (force.Visual) { 
                    case Force.Visuals.Test:
                        sfx.Animation = "row1";
                        break;
                    case Force.Visuals.Test2:
                        sfx.Animation = "row2";
                        break;
                    case Force.Visuals.Bloody:
                        sfx.Animation = "bloody";
                        sfx.Delay = 0.100f;
                        sfx.Position = TenGame.AdjustVector2(WorldMap.Viewport, screenw / 2, screenh / 2);
                        break;
                    default:
                        sfx.Animation = string.Empty;
                        break;
                }
                if (sfx.AllowNextFrame(gameTime.TotalGameTime.TotalSeconds))
                {
                    sfx.Next(sfx.Texture);
                }
                else
                {
                    sfx.SetTileToCurrent(GraphicsDevice);
                }
                renderQuads.Add(new PositionedQuad(
                        new TexturedQuad(defaultScale) { Texture = sfx.Texture }, v) { Show = true });                
                if (force.IsApplied) {
                    force.Remove = true;
                }
            }


            int i = 0;
            for (i=0; i<renderQuads.Count; i++) {
                var quad = renderQuads[i];                
                /*if (i == 1)
                {
                    newAngle = (float)(newAngle + (Math.PI / (180 * 4)));
                    if (newAngle >= Math.PI * 2)
                    {
                        newAngle = 0;
                    }
                    renderQuads[i].Rotation = new Vector3(0, newAngle, 0);
                }*/

                verts.AddRange(quad.Vertices);
            }

            //vertexBuffer.SetData(positionedQuads.SelectMany((q) => (q.Vertices)).ToArray());            
            vertexBuffer.SetData(verts.ToArray());

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.SetVertexBuffer(vertexBuffer);                       

            SetupEffect(); 
            // foreach object in some list 
            if ((namedQuads["timer"] as SpriteSheet).Current == 41)
            {
                //this.WorldMap.Disaster = true;
                seconds = 10;
            }
            foreach (var quad in renderQuads)
            {
                if (quad.Show)
                {
                    Effect.Texture = null;
                    //Effect.TextureEnabled = false;                                  
                    Effect.Texture = quad.Texture;
                    foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, quad.Vertices, 0, 2);
                    }
                    //endforeach
                    
                    var sheet = quad as SpriteSheet;
                    if (null != sheet && sheet.AllowNextFrame(gameTime.TotalGameTime.TotalSeconds))
                    {
                        sheet.Next(sheet.Texture);
                    }                    
                }
            }
            
            
        }

        internal PositionedQuad Write(string text, int screenx, int screeny, Vector3 rotation, float scale = 1) {
            Texture2D tex = Textures["bmpFont"];
            int columns = 8;
            int pixelwidth = 64;
            Color[] data = new Color[tex.Width * tex.Height];
            //useless padding.
            Color[] letters = new Color[(64*text.Length)*64];
            tex.GetData<Color>(data);
            int letterindex = 0;
            foreach (char letter in text.ToLower())
            {
                int gid = 0;
                if (Char.IsNumber(letter))
                {
                    gid = 31 + (int)Char.GetNumericValue(letter);
                }
                else if (letter >= 'a')
                {
                    gid = letter - 'a';
                }
                else if (letter.Equals(' '))
                {
                    gid = 63;
                }
                else 
                { //period
                    gid = 30;
                }

                //int xmod = (gid % columns) * pixelwidth;
                //int ymod = (int)(Math.Floor((float)gid / columns)) * (tex.Width);
                for (int y = 0; y < 64; y++)
                {
                    for (int x = 0; x < pixelwidth; x++)
                    {
                        //int dataindex = x + xmod + ((int)y * ymod);
                        //int dataindex = x + xmod + (y * tex.Width) + (ymod * 64);
                        int dataindex = x + (gid % columns) * pixelwidth + (y * tex.Width) + ((int)(Math.Floor((float)gid / columns)) * (tex.Width) * 64);
                        int stringTextureIndex = (y * (64 * text.Length)) + (64 * letterindex) + x;
                        letters[stringTextureIndex] = data[dataindex];
                    }
                 
                }
                letterindex++;             
            }

            Texture2D texture = new Texture2D(GraphicsDevice, 64*text.Length, 64);
            texture.SetData<Color>(letters);

            PositionedQuad pq = new PositionedQuad(new TexturedQuad(scale, scale/text.Length) { 
                Texture = texture}, 
                PixelPositionToVector2(screenx, screeny));
            return pq;
            //this.positionedQuads.Add(pq);
        }

        private void SetupEffect()
        {
            Effect.View = viewMatrix;
            Effect.Projection = projectionMatrix;
            Effect.World = worldMatrix;
            Effect.AmbientLightColor = Color.White.ToVector3();
            Effect.TextureEnabled = true;
        }

        float prevSeconds = 0;
        double delta = 0;
        /**
         *  Update function
         *  
         */
        internal void Update(GameTime gameTime)
        {
            this.viewMatrix = Matrix.CreateLookAt(Camera, Target, Vector3.Up);
            this.worldMatrix = Matrix.CreateWorld(Vector3.Backward, Vector3.Forward, Vector3.Up);
            //this.projectionMatrix = Matrix.CreatePerspective(GraphicsDevice.Viewport.AspectRatio, 1.0f, 0.5f, 100.0f);
            this.projectionMatrix = Matrix.CreateOrthographic(2f*GraphicsDevice.Viewport.AspectRatio, 2f, 0.1f, 100f);

            int screenw = GraphicsDevice.Viewport.Bounds.Width;
            int screenh = GraphicsDevice.Viewport.Bounds.Height;            
            

            renderQuads.Clear();                        
            
            //WorldMap.GetMapImage().GetData<Color>(mapdata);
            //Textures["mapRender"].SetData<Color>(mapdata);
            //var maptexture = Textures["mapRender"];
            var texture = WorldMap.GetMapImage();
            Effect.Texture = texture;
            PositionedQuad[] quads = new PositionedQuad[storedQuads.Count];
            storedQuads.CopyTo(quads);
            renderQuads = quads.ToList();
            //if (gameTime.TotalGameTime.TotalSeconds - delta > 0.1)
            //{

            //}
            namedQuads["map"].Texture = WorldMap.GetMapImage();
            namedQuads["map"].Position = new Vector2(Camera.X, Camera.Y);
            namedQuads["player"].Position = new Vector2(Camera.X, Camera.Y);
            namedQuads["timer"].Position = new Vector2(Camera.X, Camera.Y + 0.8f);
        
            WorldMap.Player.Location = new Vector2(Camera.X,Camera.Y);
            //positionedQuads.Add(Write("this is a test", 400, 300, new Vector3(0, 0, 0), 0.8f));
            //positionedQuads.Add(Write(string.Format("{0:0}", seconds), screenw/2, 32, new Vector3(0, 0, 0), 0.1f));
            //Effect.Texture = null;
            //positionedQuads[3].Rotation = new Vector3((float)(25 * Math.PI / 180), 0, 0);
            var seconds = (float)gameTime.TotalGameTime.TotalSeconds - prevSeconds;
            WorldMap.Update(seconds, gameTime);                        
            if (seconds >= 10.0f)
            {
                prevSeconds = (float)gameTime.TotalGameTime.TotalSeconds;
            }
            
            vertexBuffer = new DynamicVertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, this.renderQuads.Count * 6, BufferUsage.WriteOnly);
            

        }

    }
}
