using System;
using System.Collections.Generic;
using LoM.Constants;
using LoM.Game;
using LoM.Game.Build;
using LoM.Game.Job;
using LoM.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LoM.Managers
{
    public class GameManager
    {
        private const int MapHeight = 32;
        private const int MapWidth = 32;
        private const int TileSize = 32;
        
        private List<Tile> _buildTiles = new List<Tile>();

        private bool _dragging;

        private bool _showGrid;

        private int _tileXEndDrag;

        private int _tileXStartDrag;
        private int _tileYEndDrag;
        private int _tileYStartDrag;
        public BuildManager BuildManager;

        public ContentChest ContentChest;
        public InputManager InputManager;
        public JobManager JobManager;

        public Action OnJobsComplete;

        public SoundManager SoundManager;
        public UIManager UIManager;

        public World World;

        public Camera Camera { get; private set; }
        public CameraController CameraController;

        public GameManager(ContentChest contentChest)
        {
            ContentChest = contentChest;

            World = new World(MapWidth, MapHeight);

            InputManager = new InputManager(this);
            InputManager.RegisterOnKeyPress(Keys.Space, ToggleGrid);

            InputManager.MouseHeld += OnMouseHeld;
            InputManager.RightClick += OnMouseRightClick;
            InputManager.MouseReleased += OnMouseReleased;

            BuildManager = new BuildManager(this, InputManager);
            SoundManager = new SoundManager(this, BuildManager);

            World.OnTileChanged += SoundManager.TileChanged;

            UIManager = new UIManager(this, InputManager, BuildManager, SoundManager);
            JobManager = new JobManager(BuildManager);
            JobManager.OnJobsComplete += SoundManager.JobComplete;
            
            SetupCamera();

            SoundManager.PlayMainTrack();
        }

        private void SetupCamera()
        {
            var centerX = MapWidth * TileSize / 2;
            var centerY = MapHeight * TileSize / 2;
            Camera = new Camera(centerX, centerY);
            CameraController = new CameraController(Camera);
            CameraController.SetupKeys(InputManager);
        }

        private void ToggleGrid(Keys key)
        {
            _showGrid = !_showGrid;
        }

        private void OnMouseHeld()
        {
            if (BuildManager.GetMode() == BuildMode.Tile ||
                BuildManager.GetMode() == BuildMode.Destroy ||
                BuildManager.GetMode() == BuildMode.WorldObject)
                BuildTiles();
        }
        
        private void OnMouseRightClick()
        {
            var tile = GetTileAtMouse(Mouse.GetState().X, Mouse.GetState().Y);
            if (tile == null) return;
            JobManager.CancelTileJob(tile);
        }

        private void BuildTiles()
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

        private Tile GetTileAtMouse(float mouseX, float mouseY)
        {
            var worldPosition = Camera.ScreenToWorld(new Vector2(mouseX, mouseY));
            mouseX = worldPosition.X;
            mouseY = worldPosition.Y;

            var tileX = (int) Math.Floor((decimal) mouseX) / TileSize;
            var tileY = (int) Math.Floor((decimal) mouseY) / TileSize;

            return World.GetTileAt(tileX, tileY);
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

            if(BuildManager.BuildMode == BuildMode.Tile)
                JobManager.CreateTileJobs(_buildTiles);
            else if (BuildManager.BuildMode == BuildMode.Destroy)
                JobManager.CreateDestroyJobs(_buildTiles);
            else if (BuildManager.BuildMode == BuildMode.WorldObject)
                JobManager.CreateBuildJobs(_buildTiles);

            _buildTiles = null;
        }

        private void SelectTilesInRange(int xStart, int yStart, int xEnd, int yEnd)
        {
            _buildTiles = new List<Tile>();

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
            for (var y = yStart; y <= yEnd; y++)
            {
                var tile = World.GetTileAt(x, y);
                if (tile == null || tile.Type != TileType.None && BuildManager.BuildMode == BuildMode.Tile ||
                    tile.Type == TileType.None && BuildManager.BuildMode == BuildMode.Destroy || tile.WorldObject != null &&
                    BuildManager.BuildMode == BuildMode.WorldObject) continue;
                _buildTiles.Add(tile);
            }
        }

        public void Update(float deltaTime)
        {
            InputManager.Update(deltaTime);
            JobManager.Update(deltaTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawWorld(spriteBatch);
            DrawUI(spriteBatch);
        }

        private void DrawWorld(SpriteBatch spriteBatch)
        {
            var objects = new Queue<Tile>();

            var renderStartX = (Camera.X - Screen.Height) / TileSize - 1;
            var renderStartY = (Camera.Y - Screen.Width) / TileSize - 1;
            var renderEndX = (Camera.X + Screen.Width) / TileSize + 1;
            var renderEndY = (Camera.Y + Screen.Height) / TileSize + 1;

            renderStartX = MathHelper.Clamp(renderStartX, 0, MapWidth);
            renderStartY = MathHelper.Clamp(renderStartY, 0, MapHeight);
            renderEndX = MathHelper.Clamp(renderEndX, 0, MapWidth);
            renderEndY = MathHelper.Clamp(renderEndY, 0, MapHeight);

            spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null, null, Camera.Get());

            for (var i = renderStartX; i < renderEndX; i++)
            for (var j = renderStartY; j < renderEndY; j++)
            {
                var tile = World.Tiles[i, j];
                spriteBatch.Draw(ContentChest.TileTextures[tile.Type],
                    new Vector2(tile.X * TileSize, tile.Y * TileSize),
                    Color.White);

                if(tile.WorldObject != null)
                    objects.Enqueue(tile);

                if (_showGrid)
                    spriteBatch.Draw(ContentChest.GridSquare, new Vector2(tile.X * TileSize, tile.Y * TileSize),
                        Color.White);
            }

            while (objects.Count > 0)
            {
                var worldObject = objects.Dequeue().WorldObject;

                spriteBatch.Draw(ContentChest.WorldObjects[worldObject.ObjectType],
                    new Vector2(worldObject.Tile.X * TileSize, worldObject.Tile.Y * TileSize),
                    Color.White);
            }

            if (_buildTiles != null)
                foreach (var tile in _buildTiles)
                    spriteBatch.Draw(ContentChest.Reticle, new Vector2(tile.X * TileSize, tile.Y * TileSize),
                        Color.White);

            if (JobManager.JobCount() > 0)
                foreach (var job in new List<Job>(JobManager.GetJobs()))
                {
                    if (job.Cancelled) continue;

                    var tile = job.Tile;
                    if (job.JobType == JobType.Build)
                        spriteBatch.Draw(ContentChest.TileTextures[TileType.Ground],
                            new Vector2(tile.X * TileSize, tile.Y * TileSize),
                            Color.White * 0.5f);
                    else if (job.JobType == JobType.Destroy)
                        spriteBatch.Draw(ContentChest.Pixel,
                            new Rectangle(tile.X * TileSize, tile.Y * TileSize, TileSize, TileSize),
                            Color.Red * 0.15f);
                }


            var mouseX = Mouse.GetState().X;
            var mouseY = Mouse.GetState().Y;

            var mouseTile = GetTileAtMouse(mouseX, mouseY);

            if (mouseTile != null)
                spriteBatch.Draw(ContentChest.HoverSquare, new Vector2(mouseTile.X * TileSize, mouseTile.Y * TileSize),
                    Color.White);

            spriteBatch.End();
        }

        private void DrawUI(SpriteBatch spriteBatch)
        {
            UIManager.Draw(spriteBatch);
        }

    }
}