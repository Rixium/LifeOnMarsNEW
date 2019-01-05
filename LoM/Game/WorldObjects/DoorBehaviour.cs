using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace LoM.Game.WorldObjects
{
    public class DoorBehaviour : IBehaviour
    {

        public WorldObject Owner;
        public World World => Owner.Tile.World;

        public float OpenPercentage;

        public bool IsPassable()
        {
            return OpenPercentage >= 1.0f;
        }

        public void Update(float deltaTime)
        {
            var characterNear = false;
            foreach (var c in World.Characters)
            {
                if (!Owner.Tile.GetNeighbors().Contains(c.Tile) &&
                    Owner.Tile != c.Tile)
                    continue;

                OpenPercentage += 0.1f;
                characterNear = true;
                break;
            }

            if (!characterNear)
                OpenPercentage -= 0.1f;
            
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