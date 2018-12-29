using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace LoM
{
    public class GameManager
    {

        public InputManager InputManager;

        private readonly int MapWidth = 100;
        private readonly int MapHeight = 100;
        private readonly int TileSize = 32;

        public Camera Camera { get; private set; }
        
        public ContentChest ContentChest;
        public World World;

        private int _tileXStartDrag;
        private int _tileYStartDrag;

        private int _tileXEndDrag;
        private int _tileYEndDrag;
        private bool _dragging;

        private bool _showGrid;

        private List<Tile> _hoverTiles;

        public GameManager(ContentChest contentChest)
        {
            ContentChest = contentChest;
            World = new World(MapWidth, MapHeight);
            SetupCamera();

            InputManager = new InputManager(this);
            InputManager.RegisterOnKeyDown(Keys.W, MoveCamera);
            InputManager.RegisterOnKeyDown(Keys.S, MoveCamera);
            InputManager.RegisterOnKeyDown(Keys.A, MoveCamera);
            InputManager.RegisterOnKeyDown(Keys.D, MoveCamera);

            InputManager.RegisterOnKeyPress(Keys.Space, ToggleGrid);
            InputManager.MouseHeld += OnMouseHeld;
            InputManager.MouseReleased += OnMouseReleased;

            MediaPlayer.Volume = 0.25f;
            MediaPlayer.Play(ContentChest.MainMusic);
        }

        private void ToggleGrid(Keys key)
        {
            _showGrid = !_showGrid;
        }

        private void OnMouseHeld()
        {
            if (_dragging)
            {
                ContinueDrag();
                return;
            }

            _dragging = true;
            _tileXStartDrag = Mouse.GetState().X;
            _tileYStartDrag = Mouse.GetState().Y;
            var tile = GetTileAtMouse(_tileXStartDrag, _tileYStartDrag);

            if (tile == null) return;

            _tileXStartDrag = tile.X;
            _tileYStartDrag = tile.Y;
        }


        private Tile GetTileAtMouse(int mouseX, int mouseY)
        {
            var tileX = (int)Math.Floor((decimal)(mouseX - Camera.X) / 32);
            var tileY = (int)Math.Floor((decimal)(mouseY - Camera.Y) / 32);
            return GetTileAt(tileX, tileY);
        }

        private void ContinueDrag()
        {
            _tileXEndDrag = Mouse.GetState().X;
            _tileYEndDrag = Mouse.GetState().Y;
            var tile = GetTileAtMouse(_tileXEndDrag, _tileYEndDrag);
            
            if (tile == null) return;

            _tileXEndDrag = tile.X;
            _tileYEndDrag = tile.Y;

            SelectTilesInRange(_tileXStartDrag, _tileYStartDrag, _tileXEndDrag, _tileYEndDrag);
        }

        private void OnMouseReleased()
        {
            if (_dragging)
                EndDrag();
        }

        private void EndDrag()
        {
            _dragging = false;
            ChangeTiles();
            _hoverTiles = null;
        }

        private void ChangeTiles()
        {
            if (_hoverTiles == null || _hoverTiles.Count == 0) return;
            foreach (var tile in _hoverTiles)
            {
                tile.Type = TileType.Ground;
            }

            ContentChest.BuildSound.Play();
        }

        private void SelectTilesInRange(int xStart, int yStart, int xEnd, int yEnd)
        {
            _hoverTiles = new List<Tile>();

            if (xStart > xEnd)
            {
                var temp = xEnd;
                xEnd = xStart;
                xStart = temp;
            }

            if (yStart > yEnd)
            {
                var temp = yEnd;
                yEnd = yStart;
                yStart = temp;
            }

            for (var x = xStart; x <= xEnd; x++)
            {
                for (var y = yStart; y <= yEnd; y++)
                {
                    var tile = GetTileAt(x, y);
                    if (tile == null || tile.Type != TileType.None) continue;
                    _hoverTiles.Add(tile);
                }
            }
        }

        private void MoveCamera(Keys key)
        {
            if (key == Keys.W)
                Camera.Move(0, 10);
            else if (key == Keys.A)
                Camera.Move(10, 0);
            else if (key == Keys.D)
                Camera.Move(-10, 0);
            else if (key == Keys.S) Camera.Move(0, -10);
        }
        
        private void SetupCamera()
        {
            var centerX = -(MapWidth * TileSize) / 2 + Screen.Width / 2;
            var centerY = -(MapHeight * TileSize) / 2 + Screen.Height / 2;
            Camera = new Camera(centerX, centerY);
        }

        public void Update(float deltaTime)
        {
            InputManager.Update(deltaTime);
        }

        public Tile GetTileAt(int x, int y)
        {
            return World.GetTileAt(x, y);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var renderStartX = (-(Camera.X) / TileSize) - 1;
            var renderStartY = (-(Camera.Y) / TileSize) - 1;
            var renderEndX = ((-(Camera.X) + Screen.Width) / TileSize) + 1;
            var renderEndY = ((-(Camera.Y) + Screen.Height) / TileSize) + 1;

            renderStartX = MathHelper.Clamp(renderStartX, 0, MapWidth);
            renderStartY = MathHelper.Clamp(renderStartY, 0, MapHeight);
            renderEndX = MathHelper.Clamp(renderEndX, 0, MapWidth);
            renderEndY = MathHelper.Clamp(renderEndY, 0, MapHeight);

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, Camera.Get());

            for (var i = renderStartX; i < renderEndX; i++)
            for (var j = renderStartY; j < renderEndY; j++)
            {
                var tile = World.Tiles[i, j];
                spriteBatch.Draw(ContentChest.TileTextures[tile.Type], new Vector2(tile.X * 32, tile.Y * 32),
                    Color.White);

                if(_showGrid)
                    spriteBatch.Draw(ContentChest.GridSquare, new Vector2(tile.X * 32, tile.Y * 32),
                        Color.White);
            }

            if(_hoverTiles != null)
                foreach (var tile in _hoverTiles)
                {
                    spriteBatch.Draw(ContentChest.Reticle, new Vector2(tile.X * 32, tile.Y * 32),
                        Color.White);
                }

            var mouseX = Mouse.GetState().X;
            var mouseY = Mouse.GetState().Y;

            var mouseTile = GetTileAtMouse(mouseX, mouseY);

            if (mouseTile != null)
            {
                spriteBatch.Draw(ContentChest.HoverSquare, new Vector2(mouseTile.X * 32, mouseTile.Y * 32),
                    Color.White);
            }

            spriteBatch.End();
        }

    }
}