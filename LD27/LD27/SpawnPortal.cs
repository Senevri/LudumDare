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
    }
}
