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
            _inputManager.RegisterUIElement(() => { _buildManager.SetBuildObject(ObjectType.Wall); }, button);
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

            spriteBatch.End();
        }

    }
}