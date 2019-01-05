using System;
using Microsoft.Xna.Framework;

namespace LoM.Game.WorldObjects
{
    public class DoorBehaviour : IBehaviour
    {
        public Action<int> OnStateChange;

        public float OpeningTime = 0.3f;

        public float OpenPercentage;
        public WorldObject Owner;
        public World World => Owner.Tile.World;

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

            
            OnStateChange?.Invoke((int) (OpenPercentage * 100));
        }

        public IBehaviour Clone(IRenderer renderer)
        {
            var cloned = new DoorBehaviour
            {
                OnStateChange = ((TransitionRenderer) renderer).OnStateChange
            };

            return cloned;
        }

        public void SetOwner(WorldObject owner)
        {
            Owner = owner;
        }
    }
}