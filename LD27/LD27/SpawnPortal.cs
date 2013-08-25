using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    class SpawnPortal
    {
        public Vector2 Location { get; set; }
        public Creature.Types[] CreatureTypes { get; set;}
        public float Size { get; set; }
        public bool isOpen { get; set; }

        internal List<Creature> SpawnCreatures()
        {
            Random random = new Random();
            var out_list = new List<Creature>();
            for (var i = 0; i < Size; i++) { 
                out_list.Add(new Creature() { 
                    Attack = 1,
                    Speed = 1,
                    Location = this.Location, 
                    Type = this.CreatureTypes.ElementAt(random.Next(0, this.CreatureTypes.Length - 1)),
                    Range = 64,
                    Health = 2,
                    Direction = random.Next(0, 360) * Math.PI / 180,
                    AIScript = Creature.ChargePlayerIfInRange
                });
            }
            return out_list;
        }
    }
}
