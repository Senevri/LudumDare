using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    class Force
    {
        public static int lastID;
        public int ID { get; set; } 
        public enum Visuals { Test, Test2, Bloody, Explosion, Beam }
        public enum Sounds { Default, Flame, Explosion, Beam };
        protected bool _isApplied;
        public bool IsApplied { get {
            return _isApplied;
        } }
        public bool Remove { get; set; }

        public Visuals Visual { get; set; }
        public Sounds Sound { get; set; }
        public WorldMap WorldMap { get; set; }
        public Vector2 Location { get; set; }
        public Creature Creator { get; set; }
        public delegate void ApplyFunction();
        public ApplyFunction _apply {get; set;} 

        public Force() {
            ID = Force.lastID + 1;
            Force.lastID = ID;
            _isApplied = false;
        }

        public void Apply() {
            if (null != _apply) {
                _apply();
            }
                
        }
    }

    class Attack : Force
    {
        public float Speed { get; set; }
        public float Direction { get; set; }
        public float Range { get; set; }
        public float Damage { get; set; }
        public float Duration { get; set; }
        public bool Piercing { get; set; }
        private Creature _creature;


        public Attack(Creature creature) {
            _creature = creature;
            Initialize();
        }

        public Attack() 
        {
            _creature = null;
            Initialize();
             
        }

        public void Initialize() {
            _apply = this.Apply;
            Duration = 20;
            Piercing = false;
        }

        public new void Apply() {
            var self = (this as Attack);
            if (null == _creature) { 
                _creature = WorldMap.Creatures.OrderBy((c)=>(WorldMap.GetDistance(self.Location, c.Location)))
                    .FirstOrDefault((c)=>((WorldMap.GetDistance(self.Location, c.Location) <= self.Range) && c.ID != Creator.ID
                        && !c.Is("dead")));
            }
            Duration -= 1; //decay in 20 frames at 40fps.
            if (Duration <= 0)
            {
                //self._isApplied = true;
                self.Remove = true;
            }
             
            if (null == _creature) {
                this.Location = WorldMap.GetMoveLocation(this.Location, this.Direction, this.Speed);
                return; 
            }

            if (WorldMap.GetDistance(self.Location, _creature.Location) <= self.Range)
            {
                self._creature.Set("hurt");
                self._creature.Health -= Damage;
                self._creature.Set("hurt");
                if (self._creature.Health <= 0 && !(self._creature.Is("dead")))
                {
                    self._creature.Set("dead", 4);
                    Creator.Kills += 1;
                }
                //self.Remove = true;
            }
            if (!Piercing)
            {
                self._isApplied = true;
            }
        }
    }
    class Explosion : Force {
        public float Range { get; set; }
        public float Damage { get; set; }
        public float Duration { get; set; }

        public Explosion() {
            _apply = this.ExplosionFunc;
            this.Visual = Visuals.Explosion;
            this.Sound = Force.Sounds.Explosion;
        }

        public void ExplosionFunc() {
            var self = this as Explosion;
            self.Duration -= 1;
            if (this.IsApplied) {

                return;
            }
            WorldMap.Creatures.ForEach((c) =>
            {
                if ((c.ID != this.Creator.ID) &&
                    (WorldMap.GetDistance(c.Location, this.Location)<=this.Range))
                {
                    c.Health -= this.Damage;
                    if (c.Health <= 0 && !(c.Is("dead"))) {
                        c.Set("dead", 4);
                        Creator.Kills++;
                    }
                }
            });
            self._isApplied = true;
        }
    
    }
}
