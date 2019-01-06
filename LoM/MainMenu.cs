using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LoM.Constants;
using LoM.Game;
using LoM.Managers;
using LoM.Serialization;
using LoM.UI;
using LoM.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LoM
{
    public class MainMenu
    {
        private ContentChest _contentChest;
        private InputManager _inputManager;
        private List<UIElement> MenuButtons = new List<UIElement>();
        public Action OnNewGamePressed;
        public Action<World> OnLoadGame;

        private string[] _saves;

        public MainMenu(ContentChest contentChest)
        {
            _contentChest = contentChest;
            _inputManager = new InputManager();

            var buttonSettings = new ElementSettings
            {
                ImagePressed = contentChest.NewGameButtonPressed,
                ImageOff = contentChest.NewGameButtonOff
            };
            var newGameButton = new Button(Screen.Width / 2 - buttonSettings.ImagePressed.Width / 2, Screen.Height / 2, buttonSettings);
            _inputManager.RegisterUIElement(NewGame, newGameButton);


            buttonSettings = new ElementSettings
            {
                ImagePressed = contentChest.LoadGameButtonPressed,
                ImageOff = contentChest.LoadGameButtonOff
            };
            var loadGameButton = new Button(newGameButton.X, newGameButton.GetBounds().Y + newGameButton.GetBounds().Height + 10, buttonSettings);
            _inputManager.RegisterUIElement(LoadGame, loadGameButton);

            MenuButtons.Add(newGameButton);
            MenuButtons.Add(loadGameButton);

            _saves = GetSaves();

        }

        private void NewGame()
        {
            OnNewGamePressed?.Invoke();
        }

        private void LoadGame()
        {
            if (_saves.Length == 0) return;
            var loadWorld = GameLoader.LoadWorld(_saves[0]);
            if (loadWorld == null) return;
            OnLoadGame?.Invoke(loadWorld);
        }
        

        public void Update(float deltaTime)
        {
            _inputManager.Update(deltaTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearClamp);
            foreach (Button button in MenuButtons)
            {
                spriteBatch.Draw(button.Image, button.GetBounds(), Color.White);
            }

            for (var i = 0; i < _saves.Length; i++)
            {
                var font = _contentChest.MainFont;
                spriteBatch.DrawString(font, _saves[i].Split('.')[0], new Vector2(10, 10 + (i * 10) + (i * font.MeasureString(_saves[i].Split('.')[0]).Y)), Color.White);
            }

            var mousePos = _inputManager.GetMousePosition();
            spriteBatch.Draw(_contentChest.Cursor, mousePos, Color.White);

            spriteBatch.End();
        }

        public string[] GetSaves()
        {
            var saveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/LifeOnMars";
            if (Directory.Exists(saveDirectory) == false) return new string[0];
            var files = Directory.GetFiles(saveDirectory, "*.lom");
            return files;
        }

    }
}