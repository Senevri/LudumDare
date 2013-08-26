using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    class SpriteSheet : PositionedQuad    
    {
        private Texture2D _spriteSheet;
        private GraphicsDevice device;
       // private RenderTarget2D renderTarget; // see if we can clone easily.
        public int Current {get; set;}
        private int tileWidth;
        private int tileHeight;
        private int columns;
        private int tileCount;
        private int rows;

        private string _currentAnimation = string.Empty;

        public float Delay { get; set; }

        private double lastFrameShown;

        private Texture2D _texture;
        /*public Texture2D Texture { 
            get {                
                //_spriteSheet = base.Texture;
                //hmhmhm                
                _texture.SetData<Color>(GetRectColors(GetRectFromIndex(Current)));
                return _texture;
            } 
            set {
                _texture = value;
            } 
        
        }*/

        public string Animation { get { return _currentAnimation; } set { _currentAnimation = value; } }

        public Dictionary<string, int[]> AnimationIndexes;
        private PositionedQuad pq;
        private SpriteSheet sheet;
        
        public SpriteSheet(TexturedQuad quad, Vector2 position) : base(quad, position) {
            this.Initialize();
        }

        public SpriteSheet(Texture2D texture, float Scale, float ScaleY, Vector2 Location) : base(new TexturedQuad(Scale, ScaleY), Location) {
            this.Initialize();         
            this._spriteSheet = texture;
            //this.Texture = texture;
            this.columns = 8;
            this.rows = 8;
            if (null != texture)
            {
                this.tileWidth = texture.Width;
                this.tileHeight = texture.Height;
            }
            this.tileCount = 1;
            
            
        }

        public SpriteSheet(Texture2D texture, 
            GraphicsDevice Device,
            int columns = 8, 
            int tilewidth = 64, 
            int tileheight = 64, 
            float scale=0.1f, 
            float scaley=0.1f
            ) : base(new TexturedQuad(scale, scaley), new Vector2()) 
        {
            this.device = Device;
            this.Initialize();
            this.Show = true;
            this._spriteSheet = texture;
            this.tileHeight = tileheight;
            this.tileWidth = tilewidth;
            this.columns = columns;
            this.rows = texture.Height/tileHeight;
            this.tileCount = columns * rows;
            
            this.Delay = 0.5f;
            this.Scale = tilewidth*1.666f / 800f;
            this.ScaleY = tileheight / 480;
            //this.Rescale(this.tileHeight/480);
            this.GenerateVertices();
         
        }

        // Copy constructor
        public SpriteSheet(SpriteSheet sheet) : base(sheet, sheet.Position)
        {
            //// TODO: Complete member initialization
            this.Initialize();
            this.tileHeight = sheet.tileHeight;
            this.tileWidth = sheet.tileWidth;
            this.columns = sheet.columns;
            this.Current = sheet.Current;
            this.Texture = sheet.Texture;
            this.tileCount = sheet.tileCount;
            this._currentAnimation = sheet._currentAnimation;
            this.AnimationIndexes = sheet.AnimationIndexes;
            this.Show = sheet.Show;
            this.isAnimated = sheet.isAnimated;
            this.Scale = sheet.Scale;
            this.ScaleY = sheet.ScaleY;
            this.Position = sheet.Position;
            this.GenerateVertices();
        }

        public void SetTileSize(int width, int height) {
            this.tileHeight = height;
            this.tileWidth = width;
            this.columns = _spriteSheet.Width / width;
            this.rows = _spriteSheet.Height / height;
            this.tileCount = columns * rows;
        }
        
        public Color[] GetRectColors(Rectangle rect) { 
            Color[] sheet = new Color[_spriteSheet.Width * _spriteSheet.Height];
            _spriteSheet.GetData<Color>(sheet);
            Color[] outColor = new Color[tileWidth * tileHeight];
            for (int x = 0; x < rect.Width; x++) {
                for (int y = 0; y < rect.Height; y++)
                {
                    outColor[x + y * rect.Width] = sheet[rect.X + x + ((rect.Y + y) * _spriteSheet.Width)];
                }
            }
            return outColor;
        }

        public Texture2D First(GraphicsDevice device) {
            this.device = device;
            Texture2D tex = new Texture2D(device, tileWidth, tileHeight);
            tex.SetData<Color>(GetRectColors(GetRectFromIndex(0)));
            this.Texture = tex;
            //this.Rescale(this.Scale, this.ScaleY);
            return tex;
        }

        public void SetTileToCurrent(GraphicsDevice device) {
            //var pq = (this as PositionedQuad);
            this.device = device;
            Texture2D currentTile = new Texture2D(device, this.tileWidth, this.tileHeight);
            currentTile.SetData<Color>(GetRectColors(GetRectFromIndex(Current)));
            this.Texture = currentTile;
            //return pq;
        }

        public Texture2D Next(Texture2D prev)
        {
            if (tileCount <= 1) {
                return prev;
            }
            List<int> allowedList = new List<int>();
            if (!this._currentAnimation.Equals(String.Empty))
            {
                allowedList = AnimationIndexes[_currentAnimation].ToList();
            }
            if (Current < this.tileCount - 1 && (allowedList.Count == 0 || allowedList.Contains(Current +1)))
            {
                Current = Current + 1;
            }
            else {
                if (allowedList.Count == 0)
                {
                    Current = 0; 
                }
                else {
                    Current = allowedList.OrderBy((l) => (l)).First();                
                }
            }
            prev.SetData<Color>(GetRectColors(GetRectFromIndex(Current)));
            this.Texture = prev;                
            return prev;
        }

        private Rectangle GetRectFromIndex(int index)
        {
            int xtile = index % columns;
            int ytile = index / columns;
            return new Rectangle(xtile * tileWidth, ytile * tileHeight, tileWidth, tileHeight);
        }

        private void Initialize() {
            this.Current = 0;
            this.rows = 1; // always 1 row
            this.columns = 1;
            this.AnimationIndexes = new Dictionary<string, int[]>();
            this.tileCount = 64;

            /*PresentationParameters pp = device.PresentationParameters;
            renderTarget = new RenderTarget2D(device, 
                64, //pp.BackBufferWidth, 
                64, //pp.BackBufferHeight, 
                true, device.DisplayMode.Format, DepthFormat.Depth24);
             * */
        }

        private void DefaultAnimationIndexing() {            
            for (var y=0 ; y<this.rows; y++) {
                List<int> frames = new List<int>();
                for (var x=0 ; x<this.columns; x++) {
                    frames.Add((y*this.columns) + x);                
                }
                this.AnimationIndexes.Add(String.Format("anim{0}", y), frames.ToArray() );
            }            
        }

        public SpriteSheet Rescale() {
            //Console.WriteLine("sx {0}, sy {1}", this.Scale, this.ScaleY);
            this.GenerateVertices();
            return this;
        }

        internal bool AllowNextFrame(double p)
        {
            if (!isAnimated) {
                return false;
            }

            if (this.lastFrameShown + Delay < p) {
                this.lastFrameShown = p;
                return true;
            }
            return false;
            
        }

        public bool isAnimated { get; set; }

        /*internal RenderTarget2D GetTextureCopy()
        {
            this.device.SetRenderTarget(renderTarget);
            //FIXME from tutorial, check if actually required.
            this.device.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
            this.device.Clear(Color.DarkGray);
            SpriteBatch batch = new SpriteBatch(this.device);
            batch.Begin();
            batch.Draw(this.Texture, new Rectangle(0, 0, this.Texture.Width, this.Texture.Height), new Rectangle(0, 0, this.Texture.Width, this.Texture.Height), Color.White);
            batch.End();
            batch.Dispose();
            this.device.SetRenderTarget(null);
            return renderTarget;
        }*/
    }

}
