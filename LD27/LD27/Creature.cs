using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    class Creature
    {
        public enum Types {CIVILIAN, PLAYER, SMALL, MEDIUM, LARGE, BEWARE}
        public Vector2 Location { get; set; }
        public Types Type { get; set; }
    }
}
