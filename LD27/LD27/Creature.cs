using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    /*  
     * All moving creatures
     * 
     */
    class Creature
    {
        public enum Types {CIVILIAN, PLAYER, SMALL, MEDIUM, LARGE, BEWARE}
        public Vector2 Location { get; set; }
        public Types Type { get; set; }
        public int Kills { get; set; }
        public int Health { get; set; }
        public int Attack { get; set; }
        public int Range { get; set; }
        public float Speed { get; set; }
        public double Direction { get; set; }


        public delegate void AIScriptCallback(Creature creature, WorldMap worldMap, double random);

        public AIScriptCallback AIScript { get; set; }

       // and lost my train of thought.

        public Creature() {
            this.Speed = 1f;
        }


        public bool Move(Vector2 locationShift, WorldMap worldMap) {
            //worldmap is needed to check if we can move..
            //target location             

            var xpos = Location.X + locationShift.X;
            var ypos = Location.Y + locationShift.Y;
            return IfValidPathThenUpdateLocation(worldMap, xpos+32, ypos+32);
        }

        public bool Move(double angle, double distance, WorldMap worldMap) {
            float xpos = Location.X + (float)(Math.Cos(angle) * distance);
            float ypos = Location.Y + (float)(Math.Sin(angle) * distance);
            return IfValidPathThenUpdateLocation(worldMap, xpos, ypos);
        }

        private bool IfValidPathThenUpdateLocation(WorldMap worldMap, float xpos, float ypos)
        {
            var Target = new Vector2(xpos, ypos);

            if (worldMap.IsValidPath(Location, Target))
            {
                this.Location = Target;
                return true;
            }
            return false;
        }

        internal static void ChargePlayerIfInRange(Creature creature, WorldMap worldMap, double random = 1)
        {
            if (random < 0.6) { return; };
            var distance = worldMap.GetDistance(creature.Location, worldMap.ConvertLocationToPixelPosition(worldMap.Player.Location));
            if (distance < (5 * creature.Range)) {
                creature.Speed = 6;

                creature.Direction = worldMap.GetAngle(creature.Location, worldMap.ConvertLocationToPixelPosition(worldMap.Player.Location));
            }
        }
    }
}
