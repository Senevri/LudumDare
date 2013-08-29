using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Squared.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    class WorldMap : IDisposable
    {
        public Vector2 Viewport { get; set; }
        public List<Creature> Creatures { get; set; }        
        public Creature Player { get; set; }

        public List<SpawnPortal> Portals { get; set; }
        public List<Location> Locations { get; set; }
        public List<Force> Forces { get; set; } 
        public float X { get; set; }
        public float Y { get; set; }
        public int TerrorLevel { get; set; }
        public bool Disaster { get; set; }

        private Squared.Tiled.Map map;
        private RenderTarget2D renderTarget;
        private GraphicsDevice device;
        private ContentManager content;
        private double previousUpdateTotalSeconds;
        private int screenw;
        private int screenh;
        private int totalKills = 0;
        public bool EndGame { get; set; }


        public WorldMap(GraphicsDevice device, Microsoft.Xna.Framework.Content.ContentManager content) {
            this.device = device;
            this.content = content;
            screenw = device.PresentationParameters.BackBufferWidth;
            screenh = device.PresentationParameters.BackBufferHeight;
            EndGame = false;
            Portals = new List<SpawnPortal>();

            renderTarget = new RenderTarget2D(
                device,
                screenw,
                screenh,
                //1024, 
                //1024,
                false,
                device.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);

            this.Creatures = new List<Creature>();
            this.Forces = new List<Force>();
            this.Locations = new List<Location>();
            
            
            Player = new Creature() { 
                Type = Creature.Types.PLAYER, 
                Location = new Vector2(0, 0), 
                Health = 100, 
                Attack = 10, 
                ID = 9999, 
                Range  =32, 
                Speed = 7                
            };
            Player.AddCard(Card.Types.Heal);
            Player.AddCard(Card.Types.Bomb);
            Player.AddCard(Card.Types.Heal);
            Player.AddCard(Card.Types.Bomb);


            //Player.AIScript = Creature.CreateAttackIfInRange;
            Creatures.Add(Player);
            LoadMap("WorldMap.tmx");
            EndGame = true;
            
        }

        public bool Loading { get; set;  }

        public void LoadMap(string mapname)
        {
            this.Loading = true;
            map = Squared.Tiled.Map.Load(String.Format("Content\\{0}", mapname), content);

            Random random = new Random();
            foreach (var grp in map.ObjectGroups)
            {
                //ObjectGroup grp = kvpair.Value;
                foreach (var tiledobj in grp.Objects)
                {
                    if (tiledobj.Name.Equals("Start"))
                    {
                        Console.WriteLine("object: {0}, x {1} y {2} width {3} height {4}", tiledobj.Name, tiledobj.X, tiledobj.Y, tiledobj.Width, tiledobj.Height);
                        Player.Location = new Vector2(tiledobj.X + tiledobj.Width / 2, tiledobj.Y + tiledobj.Height / 2);
                    }
                    else if (tiledobj.Name.Equals("SpawnPortal"))
                    {
                        Portals.Add(new SpawnPortal()
                        {
                            CreatureTypes = new Creature.Types[] { Creature.Types.SMALL, Creature.Types.MEDIUM },
                            Location = new Vector2(tiledobj.X + tiledobj.Width / 2, tiledobj.Y + tiledobj.Height / 2),
                            Size = random.Next(1, 6),
                            isOpen = false
                        });
                    }
                    else if (tiledobj.Name.Equals("Location"))
                    {
                        if (tiledobj.Properties.ContainsKey("LocationType"))
                        {
                            Locations.Add(new Location()
                            {
                                X = tiledobj.X,
                                Y = tiledobj.Y,
                                Type = tiledobj.Properties["LocationType"],
                                Width = tiledobj.Width,
                                Height = tiledobj.Height,
                            });
                        }
                    }
                }
            }

            this.Viewport = new Vector2(Player.Location.X - (screenw / 2), Player.Location.Y - (screenh / 2));
            X = this.Viewport.X;
            Y = this.Viewport.Y;
            this.Loading = false;
        }

        /* 
         * World Update Function
         */
        public void Update(float time, GameTime gameTime) {
            //var ticks = DateTime.Now.Ticks;
            if (previousUpdateTotalSeconds == 0) { previousUpdateTotalSeconds = gameTime.TotalGameTime.TotalSeconds; }
            Random random = new Random();
            double timeDelta = gameTime.TotalGameTime.TotalSeconds - previousUpdateTotalSeconds;
            if ((time >= 10) && timeDelta > 0.025) {
                TenSecondUpdate(time, random);
                this.Disaster = false;
            }

            if (timeDelta > 0.025) {//40fps 
                foreach (var creature in this.Creatures) {
                    if (creature.Type != Creature.Types.CIVILIAN && creature.Type != Creature.Types.PLAYER) { 
                        //move creature; pause just before deadline
                        if (time < 9.5||this.Disaster) {                            
                            creature.Set("changingDirections");                            
                        }
                    } 
                    if (creature.AIScript != null) {
                        if (creature.Type == Creature.Types.PLAYER) {
                            Console.WriteLine("playerscript");
                        }
                        creature.AIScript(creature, this, (random.Next(0, 100)/100.0));
                    }
                }
                foreach (var force in this.Forces.Where((f) => (!f.IsApplied))) {
                    force.Apply();                    
                }

                this.Forces = this.Forces.Where((f) => (!f.Remove)).ToList();
                this.Creatures = this.Creatures.Where((c) => (c.Health > 0)).ToList();
                this.Portals = this.Portals.Where((p) => (!p.Destroyed)).ToList();

                previousUpdateTotalSeconds = gameTime.TotalGameTime.TotalSeconds;
                if (this.EndGame) {
                    /*if (32 < Math.Abs(this.Viewport.X - this.X) && (32 < Math.Abs(this.Viewport.Y - this.Y))) {
                        this.Forces.Clear();
                        this.Creatures.Clear();
                        this.Portals.Clear();
                        this.Locations.Clear();
                        LoadMap("BossFight.tmx");                                                
                    } */
                }

            }
            if (TerrorLevel >= 100) {
                Player.Health = 0;
                Player.Set("TerrorDeath");
                //System.Diagnostics.Debugger.Break();
            }
            if (totalKills >= 100) {
                
                EndGame = true;
            }
            //Console.WriteLine("WorldMap Update time: {0}", DateTime.Now.Ticks - ticks);
        }

        private void TenSecondUpdate(float time, Random random)
        {
           
            TerrorLevel += Creatures.Count;
            // each open portal spawns creatures.                
            Console.WriteLine("Time: {2}, Player Kills: {0} | Terrorlevel: {1}", Player.Kills, TerrorLevel, time);
            //spawn cards
            SpawnCards(Player.Kills);

            totalKills += Player.Kills;
            Player.Kills = 0;

            int openingPortalIndex = random.Next(0, this.Portals.Count - 1);
            this.Portals[openingPortalIndex].isOpen = true;
            var p = this.Portals[openingPortalIndex];
            Console.WriteLine("DOOOOM! at {0}, {1}", p.Location.X, p.Location.Y);


            /*foreach (var grp in map.ObjectGroups)
            {
                if (grp.Name.Equals("hidden")) {
                //ObjectGroup grp = kvpair.Value;
                    foreach (var tiledobj in grp.Objects)
                    {
                        tiledobj.
                    }
                }
            }*/

            // new direction;
            foreach (var creature in this.Creatures)
            {
                creature.Direction = random.Next(0, 360) * Math.PI / 180;
            }

            foreach (var portal in this.Portals)
            {
                if (portal.isOpen)
                {
                    TerrorLevel += 1;
                    if (Creatures.Count < 100)
                    {
                        Creatures.AddRange(portal.SpawnCreatures(Creatures.Count, TerrorLevel));
                        portal.Size += 0.1f;
                    }
                    else
                    {
                        portal.Size += 1f;
                    }
                }
            }

            time = 0;
            
        }


        // FIXME: Who is the correct owner? 
        private void SpawnCards(int p)
        {
            if (p > 5) {
                Player.AddCard(Card.Types.Bomb);                
            }
            if (p > 10) {
                Player.AddCard(Card.Types.Heal);                            
            }
            if (p > 15) {
                Player.AddCard(Card.Types.Sign);                            
            }
        }

        


        public RenderTarget2D GetMapImage() {
            //var ticks = DateTime.Now.Ticks;
            device.SetRenderTarget(renderTarget);
            //FIXME from tutorial, check if actually required.
            device.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
            device.Clear(Color.DarkGray);
            SpriteBatch batch = new SpriteBatch(device);
            batch.Begin();
            map.Draw(batch, new Rectangle(0,0,800,480), Viewport);
            batch.End();
            batch.Dispose();
            device.SetRenderTarget(null);
            //Console.WriteLine("WorldMap Draw time: {0}", DateTime.Now.Ticks - ticks);
            return renderTarget;
        }

        
        internal void ResetMapTexture()
        {
            foreach (var ts in this.map.Tilesets.Values) {
                ts.Texture = content.Load<Texture2D>(ts.Name);
                Console.WriteLine("{0} : {1} {2}", ts.Name, ts.Texture.Width, ts.Texture.Height);
                map.Tilesets[ts.Name].Texture = ts.Texture;
            }
        }

        internal bool IsValidPath(Vector2 Location, Vector2 Target, float shiftx = 0, float shifty = 0)
        {
            //Console.WriteLine("IsValidPath? from {0} {1} to {2} {3}", Location.X, Location.Y, Target.X, Target.Y);
                // for now, just test the target.
            foreach (var layer in this.map.Layers.Values) {                
                Point position = new Point();
                position.X = (int)Math.Round((Target.X+shiftx)/map.TileWidth);
                position.Y = (int)Math.Round((Target.Y+shifty)/map.TileHeight);
                //get tile position in reference to Target location (in pixels) 
                if (position.X < 0 || position.Y < 0 || position.X > map.Width || position.Y > map.Height) {
                    return false;
                }
                int tile = layer.GetTile(position.X, position.Y);
                if (tile.Equals(1)) {
                    return true;
                }                                                    
            }
            return false;
            
        }

        internal float GetDistance(float x1, float y1, float x2, float y2) {

            // optimization for kicks. I could do an eight/16-directional simplification too, if necessary, I guess.
            if (Math.Abs(x2 - x1) <= 1) return y2 - y1;
            if (Math.Abs(y2 - y1) <= 1) return x2 - x1;

            float distance = (float)Math.Sqrt(Math.Pow(x2-x1, 2) + Math.Pow(y2-y1, 2));
            return distance;
        }

        internal float GetDistance(Vector2 vector21, Vector2 vector22)
        {
            return GetDistance(vector21.X, vector21.Y, vector22.X, vector22.Y);
        }

        internal double GetAngle(Vector2 source, Vector2 destination)
        {
            return Math.Atan2(destination.Y - source.Y, destination.X - source.X);
        }

        internal Vector2 ConvertLocationToPixelPosition(Vector2 source)
        {
            var aspect = device.Viewport.AspectRatio;
            Vector2 Out = new Vector2(
                ((screenw/2)/aspect)*(1+source.X) + this.X+128+32, // see if we're off by 2x tilewidth FIXME!
                (screenh/2)*(1-source.Y) + this.Y
                );
            return Out;
        }

        public static Vector2 GetMoveLocation(Vector2 vector, double angle, double distance)
        {
            // optimizations begin
            if (distance<1) return vector;
            if (angle == 0) {
                return new Vector2(vector.X + (float)distance, vector.Y);
            }
            if (angle == Math.PI)
            {
                return new Vector2(vector.X - (float)distance, vector.Y);
            }

            if (angle == -Math.PI/2)
            {
                return new Vector2(vector.X, vector.Y - (float)distance);
            }
            if (angle == Math.PI / 2)
            {
                return new Vector2(vector.X, vector.Y + (float)distance);
            }
            //optimizations end;

            float xpos = vector.X + (float)(Math.Cos(angle) * distance);
            float ypos = vector.Y + (float)(Math.Sin(angle) * distance);
            return new Vector2(xpos, ypos);

        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
            
        }
        ~WorldMap() {
            Dispose(false);
        }
        protected virtual void Dispose(bool Disposing) {
            if (Disposing)
            {
                this.renderTarget.Dispose();
                this.renderTarget = null;
            }
        }
    }
}
