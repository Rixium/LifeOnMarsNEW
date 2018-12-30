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

        public SpriteFont MainFont;

        public Texture2D Reticle;
        public Texture2D HoverSquare;
        public Texture2D Pixel;
        public Texture2D GridSquare;

        public SoundEffect BuildSound;
        public SoundEffect SuccessSound;
        public Song MainMusic;

        public Dictionary<TileType, Texture2D> TileTextures = new Dictionary<TileType, Texture2D>();

        public Texture2D BuildButtonPressed;
        public Texture2D BuildButtonOff;

        public Texture2D DestroyButtonPressed;
        public Texture2D DestroyButtonOff;


        public ContentChest(ContentManager content)
        {
            _content = content;
        }

        public void Load()
        {
            TileTextures.Add(TileType.Ground, _content.Load<Texture2D>("Tile/ground"));
            TileTextures.Add(TileType.None, _content.Load<Texture2D>("Tile/none"));


            Pixel = _content.Load<Texture2D>("pixel");
            Reticle = _content.Load<Texture2D>("UI/reticle");
            HoverSquare = _content.Load<Texture2D>("UI/hover");
            GridSquare = _content.Load<Texture2D>("UI/grid");

            BuildSound = _content.Load<SoundEffect>("Sounds/build");
            SuccessSound = _content.Load<SoundEffect>("Sounds/success");

            MainMusic = _content.Load<Song>("Music/music");

            MainFont = _content.Load<SpriteFont>("Fonts/gameFont");

            BuildButtonOff = _content.Load<Texture2D>("UI/Buttons/buildButton_Off");
            BuildButtonPressed = _content.Load<Texture2D>("UI/Buttons/buildButton_Pressed");
            DestroyButtonOff = _content.Load<Texture2D>("UI/Buttons/destroyButton_Off");
            DestroyButtonPressed = _content.Load<Texture2D>("UI/Buttons/destroyButton_Pressed");

        }
    }
}
