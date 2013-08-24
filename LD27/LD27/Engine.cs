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
        
        private List<PositionedQuad> positionedQuads;
        private Microsoft.Xna.Framework.Content.ContentManager Content;

        public Vector3 Camera { get; set; }

        public Vector3 Target { get; set; }

        public Dictionary<string, Texture2D> Textures { get; set; }

        public BasicEffect Effect { get; set; }

        public WorldMap WorldMap { get; set; }

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

        private void Initialize()
        {            
            this.positionedQuads = new List<PositionedQuad>();            
            Effect = new BasicEffect(this.GraphicsDevice);
            Effect.EnableDefaultLighting();
            Effect.DirectionalLight0.Enabled = false;
            Effect.DirectionalLight1.Enabled = false;
            Effect.DirectionalLight2.Enabled = false;

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

        private float  defaultScale = 64f / 480f;

        internal void LoadContent()
        {
            int screenw = GraphicsDevice.Viewport.Bounds.Width;
            int screenh = GraphicsDevice.Viewport.Bounds.Height;
            Textures.Add("test", Content.Load<Texture2D>("testTexture"));
            Textures.Add("bmpFont", Content.Load<Texture2D>("bmpFont"));
            Textures.Add("mapRender", new Texture2D(GraphicsDevice, screenw, screenh));
            Color[] mapdata = new Color[screenw * screenh];
            WorldMap.GetMapImage().GetData<Color>(mapdata);
            Textures["mapRender"].SetData<Color>(mapdata);

            foreach (var texture in Textures) { 
                //Textures[texture.Key].
            }
            // "1" = viewport height.
            float testScale = 64f / (screenh);
            defaultScale = testScale;
            
        }

        internal Vector2 PixelPositionToVector2(int x, int y) {
            float fx = 0;
            float fy = 0;
            float aspect = GraphicsDevice.Viewport.AspectRatio;
            fx = (float)(aspect * ((2.0 * (double)x / (double)GraphicsDevice.Viewport.Bounds.Width)));
            fy = (float)(-2.0 * (double)y / (double)GraphicsDevice.Viewport.Bounds.Height);
            return new Vector2(fx-aspect, fy+1);

        }

        float newAngle = 0;

        internal void Draw(GraphicsDevice GraphicsDevice, Microsoft.Xna.Framework.GameTime gameTime)
        {
            SetupEffect();
            List<VertexPositionNormalTexture> verts = new List<VertexPositionNormalTexture>();


            
            int i = 0;
            for (i=0; i<positionedQuads.Count; i++) {
                var quad = positionedQuads[i];
                newAngle = (float)(newAngle + (Math.PI / (180 * 4)));
                if (newAngle >= Math.PI * 2)
                {
                    newAngle = 0;
                }
                if (i == 1)
                {
                    positionedQuads[i].Rotation = new Vector3(0, newAngle, 0);
                }
                verts.AddRange(quad.Vertices);
            }

            //vertexBuffer.SetData(positionedQuads.SelectMany((q) => (q.Vertices)).ToArray());
            GraphicsDevice.SetVertexBuffer(null);
            vertexBuffer.SetData(verts.ToArray());

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.SetVertexBuffer(vertexBuffer);
                    

            // foreach object in some list 
            foreach (var quad in positionedQuads)
            {                
                Effect.Texture = quad.Texture;                
                foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();                    
                    GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, quad.Vertices, 0, 2);
                    
                }
                //endforeach
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

                int xmod = (gid % columns) * pixelwidth;
                int ymod = (int)(Math.Floor((float)gid / columns)) * (tex.Width);
                for (int y = 0; y < 64; y++)
                {
                    for (int x = 0; x < pixelwidth; x++)
                    {
                        //int dataindex = x + xmod + ((int)y * ymod);
                        int dataindex = x + xmod + (y * tex.Width) + (ymod * 64);
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
        internal void Update(GameTime gameTime)
        {
            this.viewMatrix = Matrix.CreateLookAt(Camera, Target, Vector3.Up);
            this.worldMatrix = Matrix.CreateWorld(Vector3.Backward, Vector3.Forward, Vector3.Up);
            //this.projectionMatrix = Matrix.CreatePerspective(GraphicsDevice.Viewport.AspectRatio, 1.0f, 0.5f, 100.0f);
            this.projectionMatrix = Matrix.CreateOrthographic(2f*GraphicsDevice.Viewport.AspectRatio, 2f, 0.1f, 100f);

            var aspect = GraphicsDevice.Viewport.AspectRatio;
            int screenw = GraphicsDevice.Viewport.Bounds.Width;
            int screenh = GraphicsDevice.Viewport.Bounds.Height;
            float seconds = (float)gameTime.TotalGameTime.TotalSeconds - prevSeconds;
            

            positionedQuads.Clear();
            /*
            positionedQuads.Add(new PositionedQuad(new TexturedQuad(defaultScale) { Texture = Textures["test"] }, PixelPositionToVector2(32, 32)));
            positionedQuads.Add(new PositionedQuad(new TexturedQuad(defaultScale) { Texture = Textures["test"] }, PixelPositionToVector2(96, 32)));
            positionedQuads.Add(new PositionedQuad(new TexturedQuad(defaultScale) { Texture = Textures["test"] }, PixelPositionToVector2(160, 32)));
            positionedQuads.Add(new PositionedQuad(new TexturedQuad(defaultScale) { Texture = Textures["test"] }, PixelPositionToVector2(224, 32)));
            positionedQuads.Add(new PositionedQuad(new TexturedQuad(defaultScale) { Texture = Textures["test"] }, PixelPositionToVector2(288, 32)) { Rotation = new Vector3(0, 0, (float)(Math.PI / 4)) });
            positionedQuads.Add(new PositionedQuad(new TexturedQuad(defaultScale) { Texture = Textures["test"] }, new Vector2(0.5f, 0)) { Rotation = new Vector3(0, 0, (float)(-Math.PI / 4)) });
            positionedQuads.Add(new PositionedQuad(new TexturedQuad(defaultScale) { Texture = Textures["test"] }, new Vector2(0.5f, 0.5f)));
            positionedQuads.Add(new PositionedQuad(new TexturedQuad(defaultScale) { Texture = Textures["test"] }, PixelPositionToVector2(screenw - 32, screenh - 32)) { Rotation = new Vector3(0, 0, (float)(-Math.PI / 8)) });
             * */
            Color[] mapdata = new Color[screenw * screenh];
            //WorldMap.GetMapImage().GetData<Color>(mapdata);
            //Textures["mapRender"].SetData<Color>(mapdata);
            var maptexture = Textures["mapRender"];
            positionedQuads.Add(new PositionedQuad(new TexturedQuad(aspect, 1) { Texture = maptexture }, PixelPositionToVector2(maptexture.Width/2, maptexture.Height/2)));
            positionedQuads.Add(Write("this is a test", 400, 300, new Vector3(0, 0, 0), 0.8f));
            positionedQuads.Add(Write(string.Format("{0:0}", seconds), screenw/2, 32, new Vector3(0, 0, 0), 0.1f));
            //positionedQuads[3].Rotation = new Vector3((float)(25 * Math.PI / 180), 0, 0);

            if (seconds > 10.9)
            {
                prevSeconds = (float)gameTime.TotalGameTime.TotalSeconds;
            }

            vertexBuffer = new DynamicVertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, this.positionedQuads.Count * 6, BufferUsage.WriteOnly);
            

        }
        
    }
}
