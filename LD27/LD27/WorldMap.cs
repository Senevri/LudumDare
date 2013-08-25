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
    class WorldMap
    {
        public Vector2 Viewport { get; set; }
        public List<Creature> Creatures { get; set; }        
        public Creature Player { get; set; }

        public List<SpawnPortal> Portals { get; set; }
        public Dictionary<string, Rectangle[]> Locations { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        private Squared.Tiled.Map map;
        private RenderTarget2D renderTarget;
        private GraphicsDevice device;
        private ContentManager content;
        private double previousUpdateTotalSeconds;
        private int screenw;
        private int screenh;



        public WorldMap(GraphicsDevice device, Microsoft.Xna.Framework.Content.ContentManager content) {
            this.device = device;
            this.content = content;
            screenw = device.PresentationParameters.BackBufferWidth;
            screenh = device.PresentationParameters.BackBufferHeight;
            Portals = new List<SpawnPortal>();

            map = Squared.Tiled.Map.Load("Content\\WorldMap.tmx", content);
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

            Player = new Creature() { Type = Creature.Types.PLAYER, Location = new Vector2(0, 0) };
            //Creatures.Add(Player);
            Random random = new Random();
            foreach (var grp in map.ObjectGroups) {
                //ObjectGroup grp = kvpair.Value;
                foreach (var tiledobj in grp.Objects) {
                    if (tiledobj.Name.Equals("Start")) {
                        Console.WriteLine("object: {0}, x {1} y {2} width {3} height {4}", tiledobj.Name, tiledobj.X, tiledobj.Y, tiledobj.Width, tiledobj.Height);
                        Player.Location = new Vector2(tiledobj.X + tiledobj.Width/2, tiledobj.Y + tiledobj.Height/2);
                    }
                    else if (tiledobj.Name.Equals("SpawnPortal")) {
                        Portals.Add(new SpawnPortal() { 
                            CreatureTypes = new Creature.Types[]{Creature.Types.SMALL, Creature.Types.MEDIUM}, 
                            Location = new Vector2(tiledobj.X + tiledobj.Width/2, tiledobj.Y + tiledobj.Height/2), 
                            Size = random.Next(1, 4),
                            isOpen = false
                        });                    
                    }
                    else if (tiledobj.Name.Equals("Location")) { 
                        if (tiledobj.Properties.ContainsKey("LocationType")) {
                            // TODO: do something.
                        }
                    }
                }
            }

            this.Viewport = new Vector2(Player.Location.X - (screenw / 2), Player.Location.Y - (screenh / 2));
            X = this.Viewport.X;
            Y = this.Viewport.Y;

        }

        /* 
         * World Update Function
         */
        public void Update(float time, GameTime gameTime) {
            if (previousUpdateTotalSeconds == 0) { previousUpdateTotalSeconds = gameTime.TotalGameTime.TotalSeconds; }
            Random random = new Random();
            double timeDelta = gameTime.TotalGameTime.TotalSeconds - previousUpdateTotalSeconds;
            if (time >= 10 && timeDelta > 0.025) { 
                // each open portal spawns creatures.
                time = 0;
                
                int openingPortalIndex = random.Next(0, this.Portals.Count-1);
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
                foreach (var creature in this.Creatures) {
                    creature.Direction = random.Next(0, 360) * Math.PI / 180;
                }

                foreach (var portal in this.Portals) {
                    if (portal.isOpen)
                    {
                        Creatures.AddRange(portal.SpawnCreatures());

                    }
                }
            }
            if (timeDelta > 0.025) {//40fps 
                foreach (var creature in this.Creatures) {
                    if (creature.Type != Creature.Types.CIVILIAN) { 
                        //move creature; pause just before deadline
                        if (time < 9.5) {
                            if (!creature.Move(creature.Direction, creature.Speed, this)) {
                                creature.Direction = creature.Direction + (random.Next(0,2) -1) * Math.PI / 8;

                                // no multiple loops in circle
                                if (creature.Direction > Math.PI * 2) {
                                    creature.Direction = 0;
                                }
                                else if (creature.Direction < 0) {
                                    creature.Direction += 2 * Math.PI;
                                }
                            }
                        }
                    }
                    if (creature.AIScript != null) {
                        creature.AIScript(creature, this, (random.Next(0, 100)/100.0));
                    }
                }
                previousUpdateTotalSeconds = gameTime.TotalGameTime.TotalSeconds;
            }            
        }


        public RenderTarget2D GetMapImage() {
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
                if (position.X < 0 || position.Y < 0 || position.X > map.TileWidth || position.Y > map.TileHeight) {
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
    }
}
