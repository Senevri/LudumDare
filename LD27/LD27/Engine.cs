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

        internal void LoadContent()
        {
            int screenw = GraphicsDevice.Viewport.Bounds.Width;
            int screenh = GraphicsDevice.Viewport.Bounds.Height;
            Textures.Add("test", Content.Load<Texture2D>("testTexture"));
            foreach (var texture in Textures) { 
                //Textures[texture.Key].
            }
            // "1" = viewport height.
            positionedQuads.Add(new PositionedQuad(new TexturedQuad(64f / screenw) { Texture = Textures["test"] }, PixelPositionToVector2(32, 32)) { Rotation = new Vector3(0, 0, (float)(Math.PI / 4)) });
            positionedQuads.Add(new PositionedQuad(new TexturedQuad(64f / screenw) { Texture = Textures["test"] }, new Vector2(0.5f, 0)) { Rotation = new Vector3(0,0,(float)(-Math.PI / 4)) });
            positionedQuads.Add(new PositionedQuad(new TexturedQuad(64f/screenw) { Texture = Textures["test"] }, new Vector2(0.5f, 0.5f)));
            positionedQuads.Add(new PositionedQuad(new TexturedQuad(64f / screenw) { Texture = Textures["test"] }, PixelPositionToVector2(screenw - 32, screenh - 32)) { Rotation = new Vector3(0, 0, (float)(-Math.PI / 8)) });
            //positionedQuads[3].Rotation = new Vector3((float)(25 * Math.PI / 180), 0, 0);

            vertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, this.positionedQuads.Count * 6, BufferUsage.WriteOnly);
        }

        internal Vector2 PixelPositionToVector2(int x, int y) {
            float fx = 0;
            float fy = 0;
            float aspect = GraphicsDevice.Viewport.AspectRatio;
            fx = (float)(aspect * ((2.0 * (double)x / (double)GraphicsDevice.Viewport.Bounds.Width)));
            fy = (float)(-2.0 * (double)y / (double)GraphicsDevice.Viewport.Bounds.Height);
            return new Vector2(fx-aspect, fy+1);

        }

       

        internal void Draw(GraphicsDevice GraphicsDevice, Microsoft.Xna.Framework.GameTime gameTime)
        {
            SetupEffect();
            List<VertexPositionNormalTexture> verts = new List<VertexPositionNormalTexture>();
            int i = 0;
            foreach (var quad in positionedQuads) {

                /*float newAngle = (float)(quad.Rotation.X + Math.PI / 180);
                if (quad.Rotation.X >= Math.PI * 2)
                {
                    newAngle = 0;
                }
                positionedQuads[i].Rotation = new Vector3(newAngle, 0, 0);
                i++;*/
                verts.AddRange(quad.Vertices);
            }

            // DO NOT MESS WITH THE VERTEX BUFFER!
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
                    GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, quad.Vertices, 0, 3);
                    
                }
                //endforeach
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

        internal void Update(GameTime gameTime)
        {
            this.viewMatrix = Matrix.CreateLookAt(Camera, Target, Vector3.Up);
            this.worldMatrix = Matrix.CreateWorld(Vector3.Backward, Vector3.Forward, Vector3.Up);
            this.projectionMatrix = Matrix.CreatePerspective(GraphicsDevice.Viewport.AspectRatio, 1.0f, 0.5f, 1.0f);
        }
        
    }
}
