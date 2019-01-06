using System;
using Microsoft.Xna.Framework;

namespace LoM.Game.WorldObjects
{
    public class DoorBehaviour : IBehaviour
    {
        public Action<float> OnStateChange;

        public float OpeningTime = 0.3f;
        private bool _startedOpening;

        public float OpenPercentage;
        public WorldObject Owner;
        public World World => Owner.Tile.World;

        public bool IsPassable()
        {
            return OpenPercentage >= 100;
        }

        public void Update(float deltaTime)
        {
            var changeAmount = deltaTime / OpeningTime * 100;
            
            var characterNear = false;
            foreach (var c in World.Characters)
            {
                if (Owner.Tile != c.TargetTile &&
                    Owner.Tile != c.Tile)
                    continue;

                OpenPercentage += changeAmount;
                characterNear = true;
                break;
            }


            if (!characterNear)
                OpenPercentage -= changeAmount;

            OpenPercentage = MathHelper.Clamp(OpenPercentage, 0, 100);

            if (OpenPercentage <= 0)
                _startedOpening = false;
            else if (OpenPercentage != 0 && !_startedOpening)
            {
                _startedOpening = true;
                Owner.OnChange?.Invoke(Owner);
            }

            OnStateChange?.Invoke(OpenPercentage);
        }

        public IBehaviour Clone(IRenderer renderer)
        {
            var cloned = new DoorBehaviour
            {
                OnStateChange = ((TransitionRenderer) renderer).OnStateChange,
                OpeningTime = OpeningTime
            };

            return cloned;
        }

        public void SetOwner(WorldObject owner)
        {
            Owner = owner;
        }
    }
}