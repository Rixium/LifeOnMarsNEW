using System;
using System.Collections.Generic;
using LoM.UI;
using LoM.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LoM.Managers
{
    public class InputManager
    {
        private readonly GameManager _gameManager;
        private readonly Camera _camera;

        private KeyboardState _currentKeyState;
        private KeyboardState _lastKeyState;
        public Action<Keys> OnKeyPressed;
        public Dictionary<Keys, ActionMap> ActionMapBindings = new Dictionary<Keys, ActionMap>();

        private MouseState _lastMouseState;
        private MouseState _currentMouseState;
        private float _holdTime; // Holds the total number of milliseconds that the mouse button has been held down for.
        private const float RequiredHoldTime = 0.1f; // Represents how long the mouse button needs to be held down to trigger an onHold event.


        public Action MouseHeld;
        public Action MouseReleased;
        public Action MouseClick;
        public Action RightClick;

        private Dictionary<UIElement, Action> _elementMap = new Dictionary<UIElement, Action>();
        

        public class ActionMap
        {
            public Action<Keys> OnKeyPress;
            public Action<Keys> OnKeyDown;
            public Action<Keys> OnKeyUp;
        }


        public InputManager(GameManager gameManager)
        {
            _gameManager = gameManager;
            _camera = gameManager.Camera;
        }


        public void RegisterOnKeyDown(Keys key, Action<Keys> controlAction)
        {
            if(ActionMapBindings.ContainsKey(key) == false)
                ActionMapBindings.Add(key, new ActionMap());

            var action = ActionMapBindings[key];
            action.OnKeyDown += controlAction;
        }

        public void RegisterOnKeyPress(Keys key, Action<Keys> controlAction)
        {
            if (ActionMapBindings.ContainsKey(key) == false)
                ActionMapBindings.Add(key, new ActionMap());

            var action = ActionMapBindings[key];
            action.OnKeyPress += controlAction;
        }

        public void UnRegisterOnKeyDown(Keys key, Action<Keys> controlAction)
        {
            if (ActionMapBindings.ContainsKey(key) == false)
                return;

            var action = ActionMapBindings[key];
            action.OnKeyDown -= controlAction;
        }

        public void Update(float deltaTime)
        {
            ManageMouse(deltaTime);
            ManageKeyboard();
        }

        private void ManageKeyboard()
        {
            _lastKeyState = _currentKeyState;
            _currentKeyState = Keyboard.GetState();

            foreach (var key in _lastKeyState.GetPressedKeys())
            {
                if (ActionMapBindings.ContainsKey(key) == false)
                    continue;

                var action = ActionMapBindings[key];

                if (_currentKeyState[key] == KeyState.Up)
                    action?.OnKeyPress?.Invoke(key);
                else action?.OnKeyDown?.Invoke(key);
            }
        }
        
        private void ManageMouse(float deltaTime)
        {
            _lastMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();

            if (_currentMouseState.LeftButton == ButtonState.Pressed)
                OnMouseLeftDown(deltaTime);
            else if (_lastMouseState.LeftButton == ButtonState.Pressed)
                OnMouseReleased(deltaTime);

            if (_currentMouseState.RightButton == ButtonState.Pressed)
                OnMouseRightDown(deltaTime);
            else if (_lastMouseState.RightButton == ButtonState.Pressed)
                OnMouseRightReleased(deltaTime);

        }

        private void OnMouseRightReleased(float deltaTime)
        {
            RightClick?.Invoke();
        }

        private void OnMouseRightDown(float deltaTime)
        {
            RightClick?.Invoke();
        }

        private void OnMouseReleased(float deltaTime)
        {
            if (_holdTime < RequiredHoldTime)
            {
                OnMouseClick();
            }

            foreach (var keySet in _elementMap)
            {
                var element = keySet.Key;
                element.Release();
            }

            MouseReleased?.Invoke();

            _holdTime = 0;
        }

        private void OnMouseClick()
        {
            var mouseState = _currentMouseState;
            var mouse = new Rectangle(mouseState.X, mouseState.Y, 1, 1);

            foreach (var keySet in _elementMap)
            {
                var element = keySet.Key;

                if (!element.CollidesWith(mouse)) continue;
                
                element.Click();
                keySet.Value.Invoke();
                return;
            }

            MouseClick?.Invoke();
        }

        private void OnMouseLeftDown(float deltaTime)
        {
            if (_lastMouseState.LeftButton != ButtonState.Pressed) return;
            _holdTime += deltaTime;
            if(_holdTime > RequiredHoldTime)
                OnMouseHeld();
        }

        private void OnMouseHeld()
        {
            var mouseState = _currentMouseState;
            var mouse = new Rectangle(mouseState.X, mouseState.Y, 1, 1);

            foreach (var keySet in _elementMap)
            {
                var element = keySet.Key;

                if (!element.CollidesWith(mouse)) continue;

                element.Click();
                keySet.Value.Invoke();
                return;
            }

            MouseHeld?.Invoke();
        }

        public void RegisterUIElement(Action onElementClick, UIElement element)
        {
            _elementMap.Add(element, onElementClick);
        }

    }
}