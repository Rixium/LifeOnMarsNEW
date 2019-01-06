using System.Collections.Generic;
using System.IO;
using LoM.Game;
using LoM.Serialization.Data;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Newtonsoft.Json;

namespace LoM.Util
{
    public class ContentChest
    {

        public Texture2D Cursor;

        public Texture2D BuildButtonOff;

        public Texture2D BuildButtonPressed;

        public SoundEffect BuildSound;

        public Dictionary<string, ItemData> ItemData = new Dictionary<string, ItemData>();

        public Dictionary<string, Texture2D> CharacterTypes = new Dictionary<string, Texture2D>();
        public ContentManager Content;
        public Texture2D DestroyButtonOff;

        public Texture2D DestroyButtonPressed;
        public Texture2D DoorButtonOff;

        public Texture2D DoorButtonPressed;
        public Texture2D GridSquare;

        public Texture2D Helmet;
        public Texture2D HoverSquare;
        public Texture2D LoadGameButtonOff;

        public Texture2D LoadGameButtonPressed;

        public SpriteFont MainFont;
        public Song MainMusic;
        public SoundEffect Ambient;
        public Texture2D NewGameButtonOff;

        public Texture2D NewGameButtonPressed;

        public Texture2D Pause;
        public Texture2D Pixel;
        public Texture2D Play;

        public Texture2D Reticle;
        public Texture2D SaveGameButtonOff;

        public Texture2D SaveGameButtonPressed;
        public SoundEffect SuccessSound;

        public Dictionary<TileType, Texture2D> TileTextures = new Dictionary<TileType, Texture2D>();
        public Texture2D WallButtonOff;

        public Texture2D WallButtonPressed;
        public Dictionary<string, Texture2D> WorldObjects = new Dictionary<string, Texture2D>();
        public Dictionary<string, Texture2D> Items = new Dictionary<string, Texture2D>();

        public ContentChest(ContentManager content)
        {
            Content = content;
        }

        public SoundEffect DoorSound;

        public void Load()
        {
            LoadItemData();

            TileTextures.Add(TileType.Ground, Content.Load<Texture2D>("Tile/ground"));
            TileTextures.Add(TileType.None, Content.Load<Texture2D>("Tile/none"));

            var di = new DirectoryInfo(Content.RootDirectory + "/Objects");
            var files = di.GetFiles("*.xnb");

            // Load in our objects from the correct folder and bind them to the dictionary.
            foreach (var file in files)
            {
                var fileName = file.Name.Split('.')[0];
                WorldObjects.Add(fileName, Content.Load<Texture2D>($"Objects/{fileName}"));
            }

            di = new DirectoryInfo(Content.RootDirectory + "/Characters");
            files = di.GetFiles("*.xnb");

            // Load in our objects from the correct folder and bind them to the dictionary.
            foreach (var file in files)
            {
                var fileName = file.Name.Split('.')[0];
                CharacterTypes.Add(fileName, Content.Load<Texture2D>($"Characters/{fileName}"));
            }

            di = new DirectoryInfo(Content.RootDirectory + "/Items");
            files = di.GetFiles("*.xnb");

            // Load in our objects from the correct folder and bind them to the dictionary.
            foreach (var file in files)
            {
                var fileName = file.Name.Split('.')[0];
                Items.Add(fileName, Content.Load<Texture2D>($"Items/{fileName}"));
            }

            Pixel = Content.Load<Texture2D>("pixel");
            Reticle = Content.Load<Texture2D>("UI/reticle");
            HoverSquare = Content.Load<Texture2D>("UI/hover");
            GridSquare = Content.Load<Texture2D>("UI/grid");

            BuildSound = Content.Load<SoundEffect>("Sounds/build");
            SuccessSound = Content.Load<SoundEffect>("Sounds/success");
            DoorSound = Content.Load<SoundEffect>("Sounds/DoorSound");

            MainMusic = Content.Load<Song>("Music/music");
            Ambient = Content.Load<SoundEffect>("Music/ambient");

            MainFont = Content.Load<SpriteFont>("Fonts/gameFont");

            BuildButtonOff = Content.Load<Texture2D>("UI/Buttons/buildButton_Off");
            BuildButtonPressed = Content.Load<Texture2D>("UI/Buttons/buildButton_Pressed");
            DestroyButtonOff = Content.Load<Texture2D>("UI/Buttons/destroyButton_Off");
            DestroyButtonPressed = Content.Load<Texture2D>("UI/Buttons/destroyButton_Pressed");
            WallButtonOff = Content.Load<Texture2D>("UI/Buttons/buildWall_Off");
            WallButtonPressed = Content.Load<Texture2D>("UI/Buttons/buildWall_Pressed");

            NewGameButtonOff = Content.Load<Texture2D>("UI/Buttons/newGameButton");
            NewGameButtonPressed = Content.Load<Texture2D>("UI/Buttons/newGameButton_Pressed");
            SaveGameButtonOff = Content.Load<Texture2D>("UI/Buttons/saveGameButton");
            SaveGameButtonPressed = Content.Load<Texture2D>("UI/Buttons/saveGameButton_Pressed");
            LoadGameButtonOff = Content.Load<Texture2D>("UI/Buttons/loadGameButton");
            LoadGameButtonPressed = Content.Load<Texture2D>("UI/Buttons/loadGameButton_Pressed");
            DoorButtonOff = Content.Load<Texture2D>("UI/Buttons/doorButton");
            DoorButtonPressed = Content.Load<Texture2D>("UI/Buttons/doorButton_Pressed");

            Pause = Content.Load<Texture2D>("UI/Buttons/pause");
            Play = Content.Load<Texture2D>("UI/Buttons/play");

            Helmet = Content.Load<Texture2D>("Helmet");

            Cursor = Content.Load<Texture2D>("Cursor/Normal");
        }

        private void LoadItemData()
        {
            var itemDataDirectory = "Content\\Data\\Items";
            var itemData = LoadFromFiles<List<ItemData>>(itemDataDirectory);

            foreach (var dataList in itemData)
            {
                foreach (var item in dataList)
                {
                    ItemData.Add(item.Type, item);
                }
            }
        }

        private static IEnumerable<T> LoadFromFiles<T>(string directory)
        {
            if (Directory.Exists(directory) == false) return null;
            var data = new List<T>();
            foreach (var file in Directory.GetFiles(directory))
            {
                var lines = File.ReadAllText(file);
                var worldObject = JsonConvert.DeserializeObject<T>(lines);
                data.Add(worldObject);
            }
            return data.ToArray();
        }

    }
}