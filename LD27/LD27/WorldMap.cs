using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    class WorldMap
    {
        private Squared.Tiled.Map map;
        private RenderTarget2D renderTarget;
        private GraphicsDevice device;

        public WorldMap(GraphicsDevice device, Microsoft.Xna.Framework.Content.ContentManager content) {
            this.device = device;
            map = Squared.Tiled.Map.Load("Content\\WorldMap.tmx", content);
            renderTarget = new RenderTarget2D(
                device,
                device.PresentationParameters.BackBufferWidth,
                device.PresentationParameters.BackBufferHeight,
                //1024, 
                //1024,
                false,
                device.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
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
            //batch.Dispose();
            device.SetRenderTarget(null);
            return renderTarget;
        }



        public Vector2 Viewport { get; set; }
        public List<Creature> Creatures { get; set; }
        public List<SpawnPortal> Portals { get; set; }
        public Dictionary<string, Rectangle[]> Locations { get; set; }
    }
}
