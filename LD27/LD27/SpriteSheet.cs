using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    /**
     * FIXME: Combines spritesheet with animationmanager.
     */

    class SpriteSheet 
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
        public float ScaleX { get; set; }
        public float ScaleY { get; set; }

        private string _currentAnimation = string.Empty;

        public float Delay { get; set; }

        private double lastFrameShown;

        private Vector2 _position;
        public Vector2 Position { get { return _position; }
            set {
                _position = value;        
                foreach (var anim in this.Animations) {
                    anim.Position = _position;
                }
            }
        }

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

        public Texture2D Sheet { 
            get 
            {
                return _spriteSheet;
            } 
            set
            {
                _spriteSheet = value;
            } 
        } 

        public TexturedQuad[] Tiles { get; set; }

        public Dictionary<string, Animation> AnimationDefinitions;

        private Dictionary<int, Animation> _animations;
        public List<Animation> Animations
        {
            get
            { return _animations.Values.ToList(); }
        }



        public string Animation { get 
            { return _currentAnimation; 
            } 
            set { 
                _currentAnimation = value;
                this._animations.Clear();                
                this.AddAnimation(_currentAnimation, AnimationDefinitions[_currentAnimation].Position, 0);                
            } 
        }

        //public Dictionary<string, int[]> AnimationIndexes;
        //private PositionedQuad pq;
        private SpriteSheet sheet;
        
        public SpriteSheet(Texture2D texture) {
            this.Initialize();
            this.GenerateTiles();
        }

        public SpriteSheet(Texture2D texture, Vector2 Location) {
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
            this.GenerateTiles();            
        }

        public SpriteSheet(Texture2D texture, 
            GraphicsDevice Device,
            int tilewidth = 64, 
            int tileheight = 64,
            int columns = 8,             
            int scaleX = 1, 
            int scaleY = 1
            ) 
        {
            this.device = Device;
            this.Initialize();            
            this._spriteSheet = texture;
            SetTileSize(tileheight, tilewidth);
                        
            //this.Rescale(this.tileHeight/480);            
            //this.GenerateTiles();
        }


        // Copy constructor
        public SpriteSheet(SpriteSheet sheet)
        {
            //// TODO: Complete member initialization
            this.Initialize();
            this.tileHeight = sheet.tileHeight;
            this.tileWidth = sheet.tileWidth;
            this.columns = sheet.columns;
            this.Current = sheet.Current;
            
            this.tileCount = sheet.tileCount;
            this._currentAnimation = sheet._currentAnimation;
            this.GenerateTiles();
        }

        public void GenerateTiles()
        {
            this.Tiles = new TexturedQuad[tileCount];
            for (var y = 0; y < rows; y++) {
                for (var x = 0; x < columns; x++) {            
                    var tq=new TexturedQuad(){
                        ScaleX = (this.ScaleX / columns/this.ScaleX),
                        ScaleY = (this.ScaleY / rows),
                        Texture = this._spriteSheet
                    };
                    tq.GenerateVerticesWithSubsection(x*tileWidth, y*tileHeight, tileWidth, tileHeight);
                    this.Tiles[x + y * columns] = tq;
                    
                }                
            }
        }        

        public void SetTileSize(int width, int height)
        {
            this.tileHeight = height;
            this.tileWidth = width;
            this.columns = _spriteSheet.Width / width;
            this.rows = _spriteSheet.Height / height;
            this.tileCount = columns * rows;
            this.GenerateTiles();
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

        private Rectangle GetRectFromIndex(int index)
        {
            int xtile = index % columns;
            int ytile = index / columns;
            return new Rectangle(xtile * tileWidth, ytile * tileHeight, tileWidth, tileHeight);
        }

        private void Initialize() {
            this.AnimationDefinitions = new Dictionary<string, Animation>();
            this._animations = new Dictionary<int, Animation>();
            this.Current = 0;
            this.rows = 1; // always 1 row
            this.columns = 1;
            this.tileCount = 64;        
        }

        
        public Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture[] GetPositionedTile(TexturedQuad Tile, Vector2 Position, float scale=1, float angle = 0, float cameraZ = -1f) {                       
            VertexPositionNormalTexture[] outVerts = new VertexPositionNormalTexture[6];
            for (int i = 0; i<Tile.Vertices.Length; i++) {                    
                outVerts[i].TextureCoordinate = Tile.Vertices[i].TextureCoordinate;
                outVerts[i].Normal = Tile.Vertices[i].Normal;
                // FIXME: Make separate version allowing rotation
                /*
                outVerts[i].Position = Vector3.Transform(Tile.Vertices[i].Position, Matrix.CreateRotationX(Rotation.X));
                outVerts[i].Position = Vector3.Transform(outVerts[i].Position, Matrix.CreateRotationY(Rotation.Y));
                outVerts[i].Position = Vector3.Transform(outVerts[i].Position, Matrix.CreateRotationZ(Rotation.Z));                    
                 */
                
                outVerts[i].Position = Tile.Vertices[i].Position;
                if (angle != 0) {
                    outVerts[i].Position = Vector3.Transform(outVerts[i].Position, Matrix.CreateRotationZ(-angle));                    
                }
                if (scale != 1)
                {
                    outVerts[i].Position = Vector3.Transform(outVerts[i].Position, Matrix.CreateScale(scale));
                } 
                outVerts[i].Position = Vector3.Transform(outVerts[i].Position, Matrix.CreateTranslation(new Vector3(Position.X, Position.Y, -1f)));                                    
            }
            return outVerts;
        }


        internal Animation DefineAnimation(string p1, int[] p2, float delay = 0.500f, bool loop = true, float scale = 1, float alpha = 1)
        {
            this.AnimationDefinitions.Add(p1, new Animation() { 
                FrameIndexes = p2, 
                Loop = loop, 
                DelaySeconds = delay, 
                ID=this.AnimationDefinitions.Count, 
                Scale = scale, Opacity = alpha});
            return AnimationDefinitions.Last().Value;
        }

        internal Animation AddAnimation(string p, Vector2 v1, int id = -1, bool reset = false)
        {
            if (-1 == id) { id = this.Animations.Count; }
            // recycle
            if (this._animations.ContainsKey(id))
            {
                if (this._animations[id].ID == this.AnimationDefinitions[p].ID) {
                    this._animations[id].Position = v1;
                } else {
                    this._animations[id] = this.AnimationDefinitions[p].PositionCopy(v1);
                    this._animations[id].CurrentFrameIndex = 0;
                    this._animations[id].Playing = true;
                }
            }
            else
            {
                this._animations.Add(id, this.AnimationDefinitions[p].PositionCopy(v1));
                this._animations[id].Playing = true;
            }
            
            this._currentAnimation = p;
            return this._animations[id];
        }

        internal void PruneUnusedAnimations(IEnumerable<int> enumerable, bool KeepUnlooped= false)
        {
            if (enumerable.IsAny())
            {
                //this._animations = enumerable.ToDictionary((i) => (i), (i) => (this._animations[i]));
                if (!KeepUnlooped)
                {
                    this._animations = this._animations.Where((kv) => (enumerable.Contains(kv.Key))).ToDictionary(x => x.Key, x => x.Value);
                }
                else {
                    this._animations = this._animations.Where((kv) => (enumerable.Contains(kv.Key)||(!kv.Value.Loop && kv.Value.Playing))).ToDictionary(x => x.Key, x => x.Value);
                }
                                
            }
            else 
            {
                this._animations.Clear();
            }
        }
        internal void ClearAnimations() {
            this._animations.Clear();
        }
    }

}
