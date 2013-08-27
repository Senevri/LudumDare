using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    class Engine : IDisposable
    {
        private Microsoft.Xna.Framework.Graphics.GraphicsDevice GraphicsDevice;
        private Microsoft.Xna.Framework.Matrix viewMatrix;
        private Microsoft.Xna.Framework.Matrix projectionMatrix;
        private Microsoft.Xna.Framework.Matrix worldMatrix;
        private VertexBuffer vertexBuffer;
        
        private Dictionary<string, PositionedQuad> namedQuads;
        private Microsoft.Xna.Framework.Content.ContentManager Content;
        private Dictionary<string, SpriteSheet> sprites;

        private Dictionary<string, SoundEffect> Sounds;

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
            this.textQuads = new List<PositionedQuad>();
            this.sprites = new Dictionary<string, SpriteSheet>();
            this.namedQuads = new Dictionary<string, PositionedQuad>();
            this.Sounds = new Dictionary<string, SoundEffect>();

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
            vertexBuffer = new DynamicVertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, this.namedQuads.Count * 6, BufferUsage.WriteOnly);
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

        public SoundEffectInstance PlaySound(string sound) {
            if (!Sounds.ContainsKey(sound)) { return null; }
            var instance = Sounds[sound].CreateInstance();
            instance.Volume = 0.5f;
            instance.Play();
            return instance;
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
            Textures.Add("misc", Content.Load<Texture2D>("misctiles"));

            Sounds.Add("hurt", Content.Load<SoundEffect>("Hit_Hurt"));
            Sounds.Add("timer", Content.Load<SoundEffect>("Timer"));
            Sounds.Add("attack", Content.Load<SoundEffect>("Laser_Shoot"));



            Effect.Texture = Textures["mapRender"];

            float testScale = 64f / (screenh);
            defaultScale = testScale;

            namedQuads.Add("map", new PositionedQuad(aspect,1f)
            {
                Texture = Textures["mapRender"],
                Position = new Vector2(Camera.X, Camera.Y),
                Show = true
            });
                                    
            var misc = AddSpriteSheet("misc", Textures["misc"], Vector2.Zero, false, false);
            misc.SetTileSize(128, 128);
            misc.ScaleX = 3 * aspect;
            misc.ScaleY = 3f;
            misc.Delay = 0.1f;
            misc.AnimationDefinitions.Add("bigportal", new Animation(){FrameIndexes = Enumerable.Range(4, 4).ToArray()});
            //misc.Animations.Add(misc.AnimationDefinitions.First().Value.Copy());
            
            
            var player = AddSpriteSheet("player", Textures["playerspritesheet"], WorldMap.Player.Location);
            player.DefineAnimation("idle", new int[] { 0, 1 });
            player.DefineAnimation("attack", Enumerable.Range(16, 5).ToArray(), 0.08f, true);
            player.AddAnimation("idle", WorldMap.Viewport, WorldMap.Player.ID);
            
            var tiles = AddSpriteSheet("tiles", Textures["tilesheet"], Vector2.Zero, false, false);
            tiles.DefineAnimation("test", Enumerable.Range(0, 64).ToArray());
            tiles.DefineAnimation("portal", new int[] { 8 });
            //tiles.Animation = "test";
            

            var enemies = AddSpriteSheet("enemies", Textures["youmaspritesheet"], Vector2.Zero, false, false);
            enemies.Delay = 0.5f;
            enemies.DefineAnimation ("small", new int[] { 0, 1});
            enemies.DefineAnimation("medium", new int[] { 8, 9 });
            enemies.DefineAnimation("large", new int[] { 16, 17 });
            enemies.DefineAnimation("itcomes", new int[] { 24, 25, 26 });
            enemies.DefineAnimation("test", Enumerable.Range(0, 64).ToArray());
            //enemies.Animation  ="test";            
            
            var timer = AddSpriteSheet("timer", Textures["bmpFont"], PixelPositionToVector2(screenw / 2, 32));
            
            var sfx = AddSpriteSheet("sfx", Textures["sfx"], Vector2.Zero, false, false);
            sfx.Delay = 0.200f;
            sfx.DefineAnimation("row1", Enumerable.Range(0, 7).ToArray(), 0.250f);
            sfx.DefineAnimation("row2", Enumerable.Range(8,6).ToArray(), 0.200f);
            sfx.DefineAnimation("bloody", Enumerable.Range(16, 5).ToArray(), 0.300f);
            
            //sfx.Animation = "row2";

            
            timer.DefineAnimation("timer", Enumerable.Range(32, 10).ToArray(), 1f);
            System.Diagnostics.Debug.Assert(timer.AnimationDefinitions["timer"].FrameIndexes.Contains(41));            
            timer.Animation = "timer";            
        }
        

        private SpriteSheet AddSpriteSheet(string name, Texture2D texture, Vector2 position, bool animated = true, bool show = true)
        {
            //spritesheet is by default a 8x8 grid, so.... 
            // 
            sprites.Add(name, new SpriteSheet(texture, GraphicsDevice) { 
                //ScaleX = aspect, 
                ScaleX = aspect*defaultScale*8,
                ScaleY = 1*defaultScale*8
            }); // , 64, 64, 8, aspect, 1           
            var sprite = sprites.Values.Last();
            sprite.GenerateTiles();
            //sprite.DefineAnimation("default", new int[] { 0, 1 });
            //sprite.DefineAnimation("test1", new int[] { 9 });
            /*if (show)
            {
                sprite.Animation = sprite.AnimationDefinitions.Keys.First();
            } */         
            return sprite;
        }

        internal Vector2 PixelPositionToVector2(int x, int y) {
            float fx = 0;
            float fy = 0;
            
            fx = (float)(aspect * ((2.0 * (double)x / (double)GraphicsDevice.Viewport.Bounds.Width)));
            fy = (float)(-2.0 * (double)y / (double)GraphicsDevice.Viewport.Bounds.Height);
            return new Vector2(fx-aspect, fy+1);

        }

        float newAngle = 0;

        /*
         * Draw
        */
        internal void Draw(GraphicsDevice GraphicsDevice, Microsoft.Xna.Framework.GameTime gameTime)
        {
            //var ticks = DateTime.Now.Ticks;
            int screenw = GraphicsDevice.Viewport.Bounds.Width;
            int screenh = GraphicsDevice.Viewport.Bounds.Height;            
            GraphicsDevice.SetVertexBuffer(null);
            List<VertexPositionNormalTexture> verts = new List<VertexPositionNormalTexture>();
            
            float seconds = (float)gameTime.TotalGameTime.TotalSeconds - prevSeconds;            
            if (seconds > 10.0f)
            {
                prevSeconds = (float)gameTime.TotalGameTime.TotalSeconds;
            }
            delta = gameTime.TotalGameTime.TotalSeconds;

            namedQuads["map"].Texture = WorldMap.GetMapImage();
              
            foreach (var portal in WorldMap.Portals) {                
                var v1 = PixelPositionToVector2((int)(portal.Location.X-WorldMap.X), (int)(portal.Location.Y-WorldMap.Y));
                if (portal.isOpen) {                    
                    sprites["tiles"].AddAnimation("portal", v1);                     
                }                
            }
            sprites["tiles"].PruneUnusedAnimations(WorldMap.Portals.Select((x)=>(x.ID)));
            
            //renderQuads.AddRange(sprites.Values);
            var player = sprites["player"];
            if (WorldMap.Player.Is("attacking"))
            {
                //WorldMap.Player.Unset("attacking");
                /*f (player.Animation=="attack" && !player.Animations.Last().Playing) // attack doesn't loop
                {
                 
                    player.Animations.Clear();
                    //player.Animation = "idle";
                    player.AddAnimation("attack", WorldMap.Player.Location, WorldMap.Player.ID);
                }
                else
                {
                    player.Animations.Clear();
                    //player.Animation = "attack";
                    player.AddAnimation("attack", WorldMap.Player.Location, WorldMap.Player.ID);
                }*/
                if (player.Animation == "idle")
                {
                    player.Animations.Clear();
                    player.AddAnimation("attack", WorldMap.Player.Location, WorldMap.Player.ID);
                    WorldMap.Player.Unset("attacking");
                }
                
            }
            else 
            {  
                player.Animations.Clear();                
                 player.AddAnimation("idle", WorldMap.Player.Location, WorldMap.Player.ID);
            }
            
            //sprites["player"].PruneUnusedAnimations(new int [] {9999});
            //sprites["player"].PruneUnusedAnimations(new int[] { WorldMap.Player.ID });




            var enemies = sprites["enemies"];
            //enemies.Current = 0;
            //enemies.SetTileToCurrent(GraphicsDevice);            

            foreach (var enemy in WorldMap.Creatures) {
                var enemylocation = PixelPositionToVector2((int)(enemy.Location.X - WorldMap.X), (int)(enemy.Location.Y - WorldMap.Y));
                string type = string.Empty;
                switch (enemy.Type) { 
                    case Creature.Types.PLAYER:
                        // do nothing as player is rendered separately.
                        break;

                    case Creature.Types.SMALL:
                        type = "small";
                        break;
                    case Creature.Types.MEDIUM:
                        type = "medium";
                        break;
                    case Creature.Types.LARGE:
                        type = "large";
                        break;
                    case Creature.Types.BEWARE:
                        type = "itcomes";
                        break;

                    default:
                 
                        break;
                }
                if (!type.Equals(string.Empty))
                {
                    enemies.AddAnimation(type, enemylocation, enemy.ID);
                }                
                
            }
            sprites["enemies"].PruneUnusedAnimations(WorldMap.Creatures.Select((i) => (i.ID)));

            foreach (var loc in WorldMap.Locations) {
                if (WorldMap.EndGame && loc.Type == "EndGame") {
                    var v = PixelPositionToVector2((int)((loc.X - WorldMap.X) + loc.Width/2), (int)((loc.Y - WorldMap.Y) + loc.Height/2));
                    var misc = sprites["misc"];
                    misc.AddAnimation("portal", v);                    
                    }
            }
            sprites["misc"].PruneUnusedAnimations(WorldMap.Locations.Select((i) => (i.ID)));

            var sfx = sprites["sfx"];
            //Console.WriteLine("sfx count: {0}, creature count: {1}", WorldMap.Forces.Count, WorldMap.Creatures.Count);
            foreach (var force in WorldMap.Forces) { 
                var v = PixelPositionToVector2((int)(force.Location.X - WorldMap.X), (int)(force.Location.Y - WorldMap.Y));
                sfx.Delay = 0.250f;
                string type = string.Empty;
                switch (force.Visual) { 
                    case Force.Visuals.Test:
                        type = "row1";
                        break;
                    case Force.Visuals.Test2:
                        type = "row2";
                        break;
                    case Force.Visuals.Bloody:
                        type = "bloody";
                        v = TenGame.AdjustVector2(WorldMap.Viewport, screenw / 2, screenh / 2);
                        break;
                    default:
                        sfx.Animation = string.Empty;
                        break;
                }
            
                sfx.AddAnimation(type, v, force.ID);
                if (force.IsApplied) {
                    force.Remove = true;
                }
            }
            sprites["sfx"].PruneUnusedAnimations(WorldMap.Forces.Select((i) => (i.ID)));
          
            verts.AddRange(namedQuads["map"].Vertices);
            
            foreach (var sprite in sprites.Values) {
                foreach (var anim in sprite.Animations) {
                    var tile = sprite.GetPositionedTile(sprite.Tiles[anim.getNextAllowedIndex((float)gameTime.TotalGameTime.TotalSeconds)], anim.Position);
                    verts.AddRange(tile);
                }
            }
            
            vertexBuffer.SetData(verts.ToArray());

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.SetVertexBuffer(vertexBuffer);                       

            SetupEffect(); 
            // foreach object in some list 
            if (sprites["timer"].Current == 41)
            {
                this.WorldMap.Disaster = true;
                seconds = 10;
                //sprites["timer"].isAnimated = false;
            }            

            foreach (var quad in namedQuads.Values)
            {
                if (quad.Show)
                {
                    RenderVertices(GraphicsDevice, quad.Texture, quad.Vertices);
                }
            }

            foreach (var kvpair in sprites) {                
                var sheet = kvpair.Value;
                //Console.WriteLine("Animations for {0}: {1}", kvpair.Key, sheet.Animations.Count);
                foreach (var anim in sheet.Animations)
                {                    
                    RenderVertices(GraphicsDevice, sheet.Sheet, sheet.GetPositionedTile(sheet.Tiles[anim.CurrentFrame], anim.Position));
                }
            }

            //Console.WriteLine("Engine Draw time: {0}", DateTime.Now.Ticks - ticks);
        }        

        private void RenderVertices(Microsoft.Xna.Framework.Graphics.GraphicsDevice GraphicsDevice,Texture2D texture2D,VertexPositionNormalTexture[] vertices)
        {
 	        Effect.Texture = null;
            Effect.Texture = texture2D;
            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, vertices, 0, 2);
            }
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
            //var ticks = DateTime.Now.Ticks;
            this.viewMatrix = Matrix.CreateLookAt(Camera, Target, Vector3.Up);
            this.worldMatrix = Matrix.CreateWorld(Vector3.Backward, Vector3.Forward, Vector3.Up);
            //this.projectionMatrix = Matrix.CreatePerspective(GraphicsDevice.Viewport.AspectRatio, 1.0f, 0.5f, 100.0f);
            this.projectionMatrix = Matrix.CreateOrthographic(2f*GraphicsDevice.Viewport.AspectRatio, 2f, 0.1f, 100f);

            int screenw = GraphicsDevice.Viewport.Bounds.Width;
            int screenh = GraphicsDevice.Viewport.Bounds.Height;            
                        
            //if (gameTime.TotalGameTime.TotalSeconds - delta > 0.1)
            //{

            //}
            namedQuads["map"].Position = new Vector2(Camera.X, Camera.Y);
           
            sprites["player"].Position = new Vector2(Camera.X, Camera.Y);
            sprites["timer"].Position = new Vector2(Camera.X, Camera.Y + 0.8f);
        
            WorldMap.Player.Location = new Vector2(Camera.X,Camera.Y);
            //positionedQuads.Add(Write("this is a test", 400, 300, new Vector3(0, 0, 0), 0.8f));
            //positionedQuads.Add(Write(string.Format("{0:0}", seconds), screenw/2, 32, new Vector3(0, 0, 0), 0.1f));
            //Effect.Texture = null;
            //positionedQuads[3].Rotation = new Vector3((float)(25 * Math.PI / 180), 0, 0);
            var seconds = (float)gameTime.TotalGameTime.TotalSeconds - prevSeconds;
            WorldMap.Update(seconds, gameTime);
            if (!WorldMap.Disaster) {
                //sprites["timer"].isAnimated = true;
            }
            if (seconds >= 10.0f)
            {
                prevSeconds = (float)gameTime.TotalGameTime.TotalSeconds;
                PlaySound("timer");
            }

            //if (null != vertexBuffer) { vertexBuffer.Dispose(); vertexBuffer = null;  }
            var quadCount = this.namedQuads.Count;
            foreach(var sprite in sprites) {
                quadCount += sprite.Value.Animations.Count;
            }

            vertexBuffer = new DynamicVertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, quadCount * 6, BufferUsage.WriteOnly);

            //Console.WriteLine("Engine Update time: {0}", DateTime.Now.Ticks - ticks);
            if (WorldMap.Player.Is("hurt")) {
                PlaySound("hurt");
                WorldMap.Player.Set("hurt", 0);
            }
            if (WorldMap.Forces.Count > 0) {
                if (null != attackSound)
                {
                    if (attackSound.State == SoundState.Stopped) {                        
                        attackSound.Play();
                    }
                }
                else {
                    attackSound = PlaySound("attack");
                }
            }
        }

        SoundEffectInstance attackSound=null;

        public void Dispose() {
            Dispose(true);
        }

        ~Engine() {
            Dispose(false);
        }

        protected virtual void Dispose(bool Disposing) {
            if (Disposing) {
                this.vertexBuffer.Dispose();
                this.vertexBuffer = null;
            }
        
        }

        internal PositionedQuad Write(string text, int screenx, int screeny, Vector3 rotation, float scale = 1)
        {
            Texture2D tex = Textures["bmpFont"];
            int columns = 8;
            int pixelwidth = 64;
            Color[] data = new Color[tex.Width * tex.Height];
            //useless padding.
            Color[] letters = new Color[(64 * text.Length) * 64];
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

            Texture2D texture = new Texture2D(GraphicsDevice, 64 * text.Length, 64);
            texture.SetData<Color>(letters);

            PositionedQuad pq = new PositionedQuad(scale, scale / text.Length)
            {
                Texture = texture,
                Position = PixelPositionToVector2(screenx, screeny)
            };
            return pq;
            //this.positionedQuads.Add(pq);
        }


    }
}
