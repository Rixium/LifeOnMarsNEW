﻿using LoM.Constants;
using LoM.Game;
using LoM.Managers;
using LoM.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LoM
{
    public class Main : Microsoft.Xna.Framework.Game
    {
        public ContentChest ContentChest;
        private GameManager _gameManager;
        private MainMenu _mainMenu;
        private GraphicsDeviceManager _graphics;

        private float _lastUpdate;
        private SpriteBatch _spriteBatch;

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = Screen.Width,
                PreferredBackBufferHeight = Screen.Height
            };

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            Window.Title = "Life on Mars";
            IsMouseVisible = true;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            ContentChest = new ContentChest(Content);
            ContentChest.Load();

            _mainMenu = new MainMenu(ContentChest);
            _mainMenu.OnNewGamePressed += NewGame;
            _mainMenu.OnLoadGame += LoadGame;

            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        private void NewGame()
        {
            _gameManager = new GameManager(ContentChest);
            _mainMenu = null;
        }

        private void LoadGame(World world)
        {
            _gameManager = new GameManager(ContentChest, world);
            _mainMenu = null;
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            var deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
            _mainMenu?.Update(deltaTime);
            _gameManager?.Update(deltaTime * 5);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(132, 56, 56));

            GraphicsDevice.Clear(Color.Black);

            // TODO SCREEN CLASS TO HOLD AN INSTANCE OF GAME OR SCREEN.
            if(_gameManager != null)
                _gameManager?.Draw(_spriteBatch);
            else
                _mainMenu.Draw(_spriteBatch);

            base.Draw(gameTime);
        }
    }
}