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
            worldMap.Forces.Add(new Explosion() { WorldMap = worldMap, 
                Creator = creature, 
                Location = TenGame.AdjustVector2(worldMap.Viewport, TenGame.screenw/2, TenGame.screenh/2), 
                Damage = 100, Range = 64*10});
        }

        public static void SignFunc(WorldMap worldMap, Creature creature)
        {
            
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
