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
        public bool Destroyed { get; set; }

        internal List<Creature> SpawnCreatures(int creaturecount = 0, int terrorlevel = 0)
        {
            this.CreatureTypes = new Creature.Types[]{
                    Creature.Types.MEDIUM,
                    };
            if (terrorlevel < 5) {
                this.CreatureTypes = new Creature.Types[]{
                    Creature.Types.SMALL,
                    Creature.Types.MEDIUM,
            
                    };
            }

            if (terrorlevel > 10) { 
                this.CreatureTypes = new Creature.Types[]{                    
                    Creature.Types.LARGE
                };
                this.Size += 4;
            }

            if (terrorlevel > 20) {
                this.CreatureTypes = new Creature.Types[]{
                    Creature.Types.BEWARE                    
                };
                this.Size += 8;
            }


            Random random = new Random();
            var out_list = new List<Creature>();
            for (var i = 0; i < Size; i++) {
                var type = this.CreatureTypes[random.Next(0, this.CreatureTypes.Length - 1)];
                //type = Creature.Types.MEDIUM;
                out_list.Add(new Creature() { 
                    Attack = 1+(int)type,
                    Speed = 9 - (int) type,
                    Location = this.Location, 
                    Type = type,
                    Range = 64+4*(int)type,
                    Health = 2+(int)type*10,
                    ID = i + creaturecount,
                    Direction = random.Next(0, 360) * Math.PI / 180,
                    AIScript = Creature.ChargePlayerIfInRange
                });
            }
            return out_list;
        }
    }
}
