using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace LoM
{
    public class UIManager
    {

        private readonly GameManager _gameManager;
        private readonly BuildManager _buildManager;
        private readonly InputManager _inputManager;

        public List<UIElement> UIElements = new List<UIElement>();


        public UIManager(GameManager gameManager, InputManager inputManager, BuildManager buildManager)
        {
            _gameManager = gameManager;
            _buildManager = buildManager;
            _inputManager = inputManager;
            CreateButtons();
        }

        private void OnMouseClick()
        {
        }

        private void CreateButtons()
        {
            var buttonSettings = new ElementSettings()
            {
                Padding = 10,
                BackgroundColor = Color.White,
                ForegroundColor = Color.Black
            };

            var button = new Button(_gameManager.ContentChest.MainFont, "Build Floor", 10, 10, buttonSettings);
            _inputManager.RegisterUIElement(_buildManager.SetBuildMode, button);
            UIElements.Add(button);

            button = new Button(_gameManager.ContentChest.MainFont, "Clear Mode", 10, button.X + button.GetBounds().Height + 10, buttonSettings);
            _inputManager.RegisterUIElement(_buildManager.ClearMode, button);

            UIElements.Add(button);
        }

        private void OnButtonClick(UIElement element)
        {
            Console.WriteLine("Button Clicked");
        }

    }
}