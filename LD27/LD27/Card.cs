using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    class Card
    {
        public enum Types { Bomb, Sign, Heal }
        public Types Type { get; set; }
        public delegate void CardFunction(WorldMap worldMap, Creature creature);
        public CardFunction Apply { get; set; }

        public static void BombFunc(WorldMap worldMap, Creature creature) {
            worldMap.Forces.Add(new Explosion() { 
                WorldMap = worldMap, 
                Creator = creature, Duration = 40, 
                Location = creature.Location,
                Damage = 100, Range = 64*3});
        }

        public static void SignFunc(WorldMap worldMap, Creature creature)
        {
            Vector2 v;
            v = creature.Location;
            if (worldMap.Portals.Count > 0)
            {
                var portal = worldMap.Portals.OrderBy((p) => (worldMap.GetDistance(v.X, v.Y, p.Location.X, p.Location.Y))).First();
                portal.isOpen = false;
                portal.Destroyed = true;
            }
        }

        public static void HealFunc(WorldMap worldMap, Creature creature)
        {
            creature.Health = 100;
        }

        internal static Card CreateCard(Types type)
        {
            var card = new Card() { Type = type };
            switch (type) {
                case Types.Bomb:
                    card.Apply = Card.BombFunc;
                    break;
                case Types.Heal:
                    card.Apply = Card.HealFunc;
                    break;
                case Types.Sign:
                    card.Apply = Card.SignFunc;
                    break;

            }

            return card;

        }
    }
}
