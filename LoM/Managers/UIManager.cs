using System;
using System.Collections.Generic;
using LoM.Constants;
using LoM.Game;
using LoM.Game.Build;
using LoM.UI;
using LoM.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LoM.Managers
{
    public class UIManager
    {

        private ContentChest ContentChest => _gameManager.ContentChest;

        private readonly GameManager _gameManager;
        private readonly BuildManager _buildManager;
        private readonly InputManager _inputManager;
        private readonly SoundManager _soundManager;

        public List<UIElement> UIElements = new List<UIElement>();

        private Tile _mouseOverTile;


        public UIManager(GameManager gameManager, InputManager inputManager, BuildManager buildManager, SoundManager soundManager)
        {
            _gameManager = gameManager;
            _buildManager = buildManager;
            _inputManager = inputManager;
            _soundManager = soundManager;
            CreateButtons();
        }

        private void OnMouseClick()
        {
        }

        private void CreateButtons()
        {
            var buttonSettings = new ElementSettings()
            {
                ImagePressed = _gameManager.ContentChest.BuildButtonPressed,
                ImageOff = _gameManager.ContentChest.BuildButtonOff
            };

            var button = new Button(10, 10, buttonSettings);
            _inputManager.RegisterUIElement(_buildManager.SetBuildMode, button);
            UIElements.Add(button);

            button.OnClick += _soundManager.OnButtonClick;

            buttonSettings = new ElementSettings()
            {
                ImagePressed = _gameManager.ContentChest.DestroyButtonPressed,
                ImageOff = _gameManager.ContentChest.DestroyButtonOff
            };

            button = new Button(10, button.Y + button.GetBounds().Height + 10, buttonSettings);
            _inputManager.RegisterUIElement(_buildManager.SetDestroyMode, button);
            button.OnClick += _soundManager.OnButtonClick;
            UIElements.Add(button);

            buttonSettings = new ElementSettings()
            {
                ImagePressed = _gameManager.ContentChest.WallButtonPressed,
                ImageOff = _gameManager.ContentChest.WallButtonOff
            };

            button = new Button(10, button.Y + button.GetBounds().Height + 10, buttonSettings);
            _inputManager.RegisterUIElement(() => { _buildManager.SetBuildObject("Wall"); }, button);
            button.OnClick += _soundManager.OnButtonClick;
            UIElements.Add(button);

            buttonSettings = new ElementSettings()
            {
                ImagePressed = _gameManager.ContentChest.DoorButtonPressed,
                ImageOff = _gameManager.ContentChest.DoorButtonOff
            };

            button = new Button(10, button.Y + button.GetBounds().Height + 10, buttonSettings);
            _inputManager.RegisterUIElement(() => { _buildManager.SetBuildObject("Door"); }, button);
            button.OnClick += _soundManager.OnButtonClick;
            UIElements.Add(button);


            buttonSettings = new ElementSettings()
            {
                ImagePressed = _gameManager.ContentChest.DoorButtonPressed,
                ImageOff = _gameManager.ContentChest.DoorButtonOff
            };

            button = new Button(10, button.Y + button.GetBounds().Height + 10, buttonSettings);
            _inputManager.RegisterUIElement(() => { _buildManager.SetBuildObject("Stockpile"); }, button);
            button.OnClick += _soundManager.OnButtonClick;
            UIElements.Add(button);

            buttonSettings = new ElementSettings()
            {
                ImagePressed = _gameManager.ContentChest.Play,
                ImageOff = _gameManager.ContentChest.Play
            };

            button = new Button(Screen.Width - buttonSettings.ImagePressed.Width - 10, Screen.Height - 10 - buttonSettings.ImagePressed.Height, buttonSettings);
            _inputManager.RegisterUIElement(() => { _gameManager.Pause(false); }, button);
            button.OnClick += _soundManager.OnButtonClick;
            UIElements.Add(button);


            buttonSettings = new ElementSettings()
            {
                ImagePressed = _gameManager.ContentChest.Pause,
                ImageOff = _gameManager.ContentChest.Pause
            };

            button = new Button(button.X, button.Y - 10 - buttonSettings.ImagePressed.Height, buttonSettings);
            _inputManager.RegisterUIElement(() => { _gameManager.Pause(true); }, button);
            button.OnClick += _soundManager.OnButtonClick;
            UIElements.Add(button);

            buttonSettings = new ElementSettings()
            {
                ImagePressed = _gameManager.ContentChest.SaveGameButtonPressed,
                ImageOff = _gameManager.ContentChest.SaveGameButtonOff
            };

            button = new Button(Screen.Width - buttonSettings.ImagePressed.Width - 10, 10, buttonSettings);
            _inputManager.RegisterUIElement(() => { _gameManager.SaveGame(); }, button);
            button.OnClick += _soundManager.OnButtonClick;
            UIElements.Add(button);

        }

        private void OnButtonClick(UIElement element)
        {
            Console.WriteLine("Button Clicked");
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
/*

            if (_activeJobs.Count != 0)
            {
                var jobString = $"{ _activeJobs.Count } Jobs";
                var measurements = ContentChest.MainFont.MeasureString(jobString);

                spriteBatch.DrawString(ContentChest.MainFont, jobString, new Vector2(Screen.Width - 10 - measurements.X, 10),
                    Color.White);
            }
*/

            foreach (var element in UIElements)
            {
                if (element.Type() != ElementType.Button) continue;

                var button = (Button)element;
                spriteBatch.Draw(button.Image, button.GetPosition(), Color.White);
            }

            if (_gameManager.SelectedCharacter != null)
            {
                spriteBatch.DrawString(ContentChest.MainFont, $"Selected: {_gameManager.SelectedCharacter.CharacterType}",
                    new Vector2(10, Screen.Height - 10 - ContentChest.MainFont.MeasureString($"Selected: {_gameManager.SelectedCharacter.CharacterType}").Y),
                    Color.White);
            }

            var mousePos = _inputManager.GetMousePosition();

            if (_mouseOverTile != null &&
                _mouseOverTile.ItemStack != null)
            {
                var itemData = ContentChest.ItemData[_mouseOverTile.ItemStack.Item.Type];
                var str = $"{itemData.Name} x{_mouseOverTile.ItemStack.Amount}";
                var strWidth = (int) ContentChest.MainFont.MeasureString(str).X;
                var strHeight = (int) ContentChest.MainFont.MeasureString(str).Y;

                spriteBatch.Draw(ContentChest.Pixel,
                    new Rectangle((int) mousePos.X + 10, (int) mousePos.Y + 10, 20 + strWidth, 20 + strHeight),
                    Color.White);
                spriteBatch.DrawString(ContentChest.MainFont, str, new Vector2(mousePos.X + 20, mousePos.Y + 20),
                    Color.Black);
            }

            spriteBatch.Draw(ContentChest.Cursor, mousePos, Color.White);

            spriteBatch.End();
        }

        public void OnMouseMoved(Vector2 mousePosition)
        {
            _mouseOverTile = _gameManager.GetTileAtMouse(mousePosition);
        }
    }
}