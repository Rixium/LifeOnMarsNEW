using LoM.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LoM.Game.WorldObjects
{
    public class TransitionRenderer : IRenderer
    {
        public ContentChest ContentChest;


        private int _lastState;
        public int CurrentTransition;
        public float CurrentTransitionTime;

        private int _maxTransition;
        public int MaxTransition
        {
            get => _maxTransition;
            set
            {
                PerTransitionTime = TransitionTime / value;
                _maxTransition = value;
            }
        }

        public float PerTransitionTime;

        public string[] TransitionTextures;

        private float _transitionTime;
        public float TransitionTime
        {
            get => _transitionTime;
            set
            {
                PerTransitionTime = value / MaxTransition;
                _transitionTime = value;
            }
        }

        public void Draw(SpriteBatch spriteBatch, WorldObject owner)
        {
            var img = ContentChest.WorldObjects[TransitionTextures[CurrentTransition]];
            spriteBatch.Draw(img, new Vector2(owner.Tile.X * 32, owner.Tile.Y * 32), Color.White);
        }

        public void Update(float deltaTime)
        {
            CurrentTransitionTime += deltaTime;
        }

        public IRenderer Clone()
        {
            return new TransitionRenderer
            {
                TransitionTextures = TransitionTextures,
                MaxTransition = MaxTransition,
                TransitionTime = TransitionTime,
                ContentChest = ContentChest
            };
        }

        public void OnStateChange(int newState)
        {
            if (CurrentTransitionTime < PerTransitionTime) return;
            CurrentTransitionTime = 0;

            if (newState > _lastState)
                CurrentTransition++;
            else CurrentTransition--;

            _lastState = newState;
            CurrentTransition = MathHelper.Clamp(CurrentTransition, 0, MaxTransition);
        }
    }
}