using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace LoM
{
    public class ContentChest
    {

        private ContentManager _content;

        public Texture2D Reticle;
        public Texture2D HoverSquare;
        public Texture2D GridSquare;

        public SoundEffect BuildSound;
        public Song MainMusic;

        public Dictionary<TileType, Texture2D> TileTextures = new Dictionary<TileType, Texture2D>();


        public ContentChest(ContentManager content)
        {
            _content = content;
        }

        public void Load()
        {
            TileTextures.Add(TileType.Ground, _content.Load<Texture2D>("Tile/ground"));
            TileTextures.Add(TileType.None, _content.Load<Texture2D>("Tile/none"));


            Reticle = _content.Load<Texture2D>("UI/reticle");
            HoverSquare = _content.Load<Texture2D>("UI/hover");
            GridSquare = _content.Load<Texture2D>("UI/grid");

            BuildSound = _content.Load<SoundEffect>("Sounds/build");
            MainMusic = _content.Load<Song>("Music/music");
        }
    }
}
