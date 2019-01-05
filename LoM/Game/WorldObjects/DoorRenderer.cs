using LoM.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LoM.Game.WorldObjects
{

    public class DoorRenderer : IRenderer
    {

        public WorldObject Owner { get; set; }
        public DoorBehaviour DoorBehaviour => (DoorBehaviour) Owner.Behaviour;

        public void Draw(SpriteBatch spriteBatch, ContentChest contentChest)
        {
            var drawTile = Owner.Tile;

            var selectedImage = GetImage(contentChest);

            spriteBatch.Draw(selectedImage, new Vector2(drawTile.X * 32, drawTile.Y * 32), Color.White);
        }

        private Texture2D GetImage(ContentChest contentChest)
        {
            var openPercentage = DoorBehaviour.OpenPercentage;


            // TODO : This needs to be accessed another way, perhaps we can initialise the renderer with a set of images depending on whether it is animated.
            if (openPercentage < 0.1f)
                return contentChest.WorldObjects["Door"];
            if (openPercentage < 0.25f)
                return contentChest.WorldObjects["Door_1"];
            if (openPercentage < 0.4f)
                return contentChest.WorldObjects["Door_2"];
            if (openPercentage < 0.55f)
                return contentChest.WorldObjects["Door_3"];
            if (openPercentage < 0.7f)
                return contentChest.WorldObjects["Door_4"];
            if (openPercentage < 0.85f)
                return contentChest.WorldObjects["Door_5"];

            return contentChest.WorldObjects["Door_6"];
        }

        public IRenderer Clone()
        {
            return new DoorRenderer();
        }

        public void SetOwner(WorldObject owner)
        {
            Owner = owner;
        }

    }

}