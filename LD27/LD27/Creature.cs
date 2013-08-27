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
        public static int lastID;
        public enum Types {CIVILIAN, PLAYER, SMALL, MEDIUM, LARGE, BEWARE}
        public Vector2 Location { get; set; }
        public Types Type { get; set; }
        public int Kills { get; set; }
        public float Health { get; set; }
        public float Attack { get; set; }
        public float Range { get; set; }
        public float Speed { get; set; }
        public double Direction { get; set; }
        public int ID { get; set; }

        private Dictionary<string, float> _properties;


        public delegate void AIScriptCallback(Creature creature, WorldMap worldMap, double random);

        public AIScriptCallback AIScript { get; set; }

       // and lost my train of thought.

        public Creature() {
            this.ID = lastID + 1;
            Creature.lastID = this.ID;
            this.Speed = 1f;
            this._properties = new Dictionary<string, float>();
        }

        public bool Is(string s) {
            return (this._properties.ContainsKey(s) && this._properties[s] > 0);
        }

        public float Set(string s, float f=1) {
            var f_old = 0f;
            if (this._properties.ContainsKey(s)) { 
                f_old =  this._properties[s];
            }
            this._properties[s] = f;
            return f_old;
        }

        public void Unset(string s) {
            this._properties.Remove(s);
        }

        public float Get(string s, float? newval = null) {
            if (this._properties.ContainsKey(s))
            {
                var outval = this._properties[s];
                this._properties[s] = (null == newval)? outval: (float)newval;
                return outval;
            }
            return 0;
        }
         


        public bool Move(Vector2 locationShift, WorldMap worldMap) {
            //worldmap is needed to check if we can move..
            //target location             

            var xpos = Location.X + locationShift.X;
            var ypos = Location.Y + locationShift.Y;
            return IfValidPathThenUpdateLocation(worldMap, xpos+32, ypos+32);
        }

        public Vector2 GetMoveLocation(float x, float y, double angle, double distance) {
            float xpos = x + (float)(Math.Cos(angle) * distance);
            float ypos = y + (float)(Math.Sin(angle) * distance);
            return new Vector2(xpos, ypos);
        
        }
        public Vector2 GetMoveLocation(Vector2 v, double angle, double distance)
        {
            return WorldMap.GetMoveLocation(v, angle, distance);
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
            if (creature.Is("changingDirections")) {
                Random rnd = new Random();
                creature.MoveScript(rnd, worldMap);
                creature.Set("changingDirections", 0);
            }

            if (creature.Is("charging"))
            {
                creature.Speed = creature.Get("charging", 0);
            }
            else {
                //creature.Set("charging", creature.Speed);
            }            

            if (random < 0.975) { return; };
            creature.Set("charging", creature.Speed);
            var distance = worldMap.GetDistance(creature.Location, worldMap.ConvertLocationToPixelPosition(worldMap.Player.Location));
            if (distance < (5 * creature.Range)) {
                creature.Speed = 12-(int)creature.Type;

                creature.Direction = worldMap.GetAngle(creature.Location, worldMap.ConvertLocationToPixelPosition(worldMap.Player.Location));
                if (distance < creature.Range) {
                    worldMap.Forces.Add(
                            new Attack(worldMap.Player){ 
                                Creator = creature, 
                                Location = worldMap.Player.Location, 
                                Duration = 15, 
                                Damage = creature.Attack, 
                                Range = creature.Range, 
                                WorldMap = worldMap, 
                                Visual = Force.Visuals.Bloody
                            }
                        );
                }
            }
        }

        internal static void CreateAttack(Creature creature, WorldMap worldMap, double random = 1) { 
            worldMap.Forces.Add(new Attack(){ 
                Creator = creature,
                Damage = creature.Attack, 
                Range  = creature.Range, 
                Location = creature.GetMoveLocation(creature.Location.X, creature.Location.Y, creature.Direction, 32f), 
                WorldMap = worldMap 
            });
        }
        internal static void CreateAttackIfInRange(Creature creature, WorldMap worldMap, double random = 1)
        {
            var creatures = worldMap.Creatures.Where((c) => ((worldMap.GetDistance(creature.Location, c.Location) <= creature.Range) && c.ID != creature.ID)).Take(5).ToList();
            for(int i=0; i<creatures.Count; i++)
            {
                var enemy = creatures[i];
                worldMap.Forces.Add(new Attack(enemy)
                {
                    Creator = creature,
                    Damage = creature.Attack,
                    Range = creature.Range,
                    Location = creature.GetMoveLocation(creature.Location.X, creature.Location.Y, creature.Direction, 32f),
                    WorldMap = worldMap
                });
            }

        }

        public void MoveScript(Random random, WorldMap worldMap)
        {
            var creature = this;
            if (!creature.Move(creature.Direction, creature.Speed, worldMap))
            {
                creature.Direction = creature.Direction + (random.Next(0, 2) - 1) * Math.PI / 8;
            }
            // no multiple loops in circle
            if (creature.Direction > Math.PI * 2)
            {
                creature.Direction = 0;
            }
            else if (creature.Direction < 0)
            {
                creature.Direction += 2 * Math.PI;
            }
            
        }
 
    }
}
