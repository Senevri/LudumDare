using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    class Force
    {
        public enum Visuals { Test, Test2, Bloody }
        protected bool _isApplied;
        public bool IsApplied { get {
            return _isApplied;
        } }
        public bool Remove { get; set; }

        public Visuals Visual { get; set; }
        public WorldMap WorldMap { get; set; }
        public Vector2 Location { get; set; }
        public Creature Creator { get; set; }
        public delegate void ApplyFunction();
        public ApplyFunction _apply {get; set;} 

        public Force() {
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
        public float Range { get; set; }
        public float Damage { get; set; }
        public float Duration { get; set; }
        private Creature _creature;


        public Attack(Creature creature) {
            _creature = creature;
            _apply = this.Apply;
            Duration = 20;
        }

        public Attack() 
        {
            _creature = null;
            _apply = this.Apply;
            Duration = 20;
        }

        public new void Apply() {
            var self = (this as Attack);
            if (null == _creature) { 
                _creature = WorldMap.Creatures.FirstOrDefault((c)=>((WorldMap.GetDistance(self.Location, c.Location) <= self.Range) && c.ID != Creator.ID));
            }
            if (null == _creature) {
                Duration -= 1; //decay in 20 frames at 40fps.
                if (Duration <= 0) {
                    //self._isApplied = true;
                    self.Remove = true;
                }
                return; 
            }

            if (WorldMap.GetDistance(self.Location, _creature.Location) <= self.Range)
            {
                self._creature.Health -= Damage;
                if (self._creature.Health <= 0)
                {
                    Creator.Kills += 1;
                }
                //self.Remove = true;
            }
            self._isApplied = true;            
        }
    }
}
