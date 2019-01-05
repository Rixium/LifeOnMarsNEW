using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace LoM.Game.WorldObjects
{
    public class DoorBehaviour : IBehaviour
    {

        public float OpeningTime = 0.5f;
        public WorldObject Owner;
        public World World => Owner.Tile.World;

        private bool shown = false;

        public float OpenPercentage;

        public bool IsPassable()
        {
            return OpenPercentage >= 1.0f;
        }

        public void Update(float deltaTime)
        {
            var changeAmount = deltaTime / OpeningTime;

            var characterNear = false;
            foreach (var c in World.Characters)
            {
                if (Owner.Tile != c.TargetTile)
                    continue;

                OpenPercentage += changeAmount;
                characterNear = true;
                break;
            }



            if (!characterNear)
                OpenPercentage -= changeAmount;
            
            OpenPercentage = MathHelper.Clamp(OpenPercentage, 0, 1);
        }

        public IBehaviour Clone()
        {
            return new DoorBehaviour();
        }

        public void SetOwner(WorldObject owner)
        {
            Owner = owner;
        }

    }
}