using System;
using LoM.Constants;
using LoM.Managers;
using LoM.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LoM
{
    public class Main : Microsoft.Xna.Framework.Game
    {
        private GameManager _gameManager;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private float _lastUpdate;

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
            var contentChest = new ContentChest(Content);
            contentChest.Load();

            _gameManager = new GameManager(contentChest);

            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            var deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
            _gameManager.Update(deltaTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(132, 56, 56));

            GraphicsDevice.Clear(Color.Black);
            _gameManager.Draw(_spriteBatch);
            base.Draw(gameTime);
        }
    }
}