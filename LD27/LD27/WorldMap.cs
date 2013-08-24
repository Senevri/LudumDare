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

        public WorldMap(GraphicsDevice device, Microsoft.Xna.Framework.Content.ContentManager content) {
            this.device = device;
            this.content = content;
            var screenw = device.PresentationParameters.BackBufferWidth;
            var screenh = device.PresentationParameters.BackBufferHeight;

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
            Creatures.Add(Player);
            Random random = new Random();
            foreach (var kvpair in map.ObjectGroups) {
                ObjectGroup grp = kvpair.Value;
                foreach (var objkvpair in grp.Objects) {
                    var tiledobj = objkvpair.Value;
                    if (tiledobj.Name.Equals("Start")) {
                        Console.WriteLine("object: {0}, x {1} y {2} width {3} height {4}", tiledobj.Name, tiledobj.X, tiledobj.Y, tiledobj.Width, tiledobj.Height);
                        Player.Location = new Vector2(tiledobj.X + tiledobj.Width/2, tiledobj.Y + tiledobj.Height/2);
                    }
                    else if (tiledobj.Name.Equals("SpawnPortal")) {
                        Portals.Add(new SpawnPortal() { CreatureTypes = new Creature.Types[]{Creature.Types.SMALL, Creature.Types.MEDIUM}, Location = new Vector2(tiledobj.X + tiledobj.Width/2, tiledobj.Y + tiledobj.Height/2), Size = random.Next(1, 100) });                    
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

        public void Update(float time) {
            if (time >= 10) { 
                // each open portal spawns creatures.
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
    }
}
