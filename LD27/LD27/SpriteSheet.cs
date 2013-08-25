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
        public int Current {get; set;}
        private int tileWidth;
        private int tileHeight;
        private int columns;
        private int tileCount;
        private int rows;

        private string _currentAnimation = string.Empty;

        public float Delay { get; set; }

        private double lastFrameShown;

        public string Animation { get { return _currentAnimation; } set { _currentAnimation = value; } }

        public Dictionary<string, int[]> AnimationIndexes;
        private PositionedQuad pq;
        private SpriteSheet sheet;
        
        public SpriteSheet(TexturedQuad quad, Vector2 position) : base(quad, position) {
            this.Initialize();
        }

        public SpriteSheet(Texture2D texture, float Scale, float ScaleY, Vector2 Location) : base(new TexturedQuad(Scale, ScaleY), Location) {
            this._spriteSheet = texture;
            this.Texture = texture;
            this.columns = 1;
            this.rows = 1;
            if (null != texture)
            {
                this.tileWidth = texture.Width;
                this.tileHeight = texture.Height;
            }
            this.tileCount = 1;
            
            this.Initialize();         
        }

        public SpriteSheet(Texture2D texture, int columns = 8, int tilewidth = 64, int tileheight = 64, float scale=0.1f, float scaley=0.1f) : base(new TexturedQuad(scale, scaley), new Vector2()) {
            this.Show = true;
            this._spriteSheet = texture;
            this.tileHeight = tileheight;
            this.tileWidth = tilewidth;
            this.columns = columns;
            this.rows = texture.Height/tileHeight;
            this.tileCount = columns * rows;
            this.Initialize();
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

        /*public SpriteSheet(PositionedQuad pq) : base(pq)
        {

            this.Texture = pq.Texture;
            this.Effect = pq.Effect;
            this.Rotation = pq.Rotation;
            this.Scale = pq.Scale;
            this.ScaleY = pq.ScaleY;
            this.Position = pq.Position;
            this.Vertices = pq.Vertices;
            this.columns = 1;
            this.rows = 1;
            this.tileWidth = pq.Texture.Width;
            this.tileHeight = pq.Texture.Height;
            this.tileCount = 1;
            this.Initialize();
        }*/

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
            Texture2D tex = new Texture2D(device, tileWidth, tileHeight);
            tex.SetData<Color>(GetRectColors(GetRectFromIndex(0)));
            this.Texture = tex;
            //this.Rescale(this.Scale, this.ScaleY);
            return tex;
        }

        public void SetTileToCurrent(GraphicsDevice device) {
            //var pq = (this as PositionedQuad);
            Texture2D currentTile = new Texture2D(device, this.tileWidth, this.tileHeight);
            currentTile.SetData<Color>(GetRectColors(GetRectFromIndex(Current)));
            this.Texture = currentTile;
            //return pq;
        }

        public Texture2D Next(Texture2D prev)
        {
            if (tileCount <= 1) {
                return Texture;
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
            this.rows = 0;
            this.AnimationIndexes = new Dictionary<string, int[]>();
        }

        private void DefaultAnimationIndexing() {
            ;
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
    }

}
