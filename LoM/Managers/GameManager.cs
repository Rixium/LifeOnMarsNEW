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
using Microsoft.Xna.Framework.Media;

namespace LoM.Managers
{
    public class GameManager
    {

        private const int MapHeight = 32;
        private const int MapWidth = 32;
        private const int TileSize = 32;
        private const float ZoomSpeed = 0.02f;
        private const int CameraSpeed = 5;

        private bool _dragging;

        private List<Job> _activeJobs = new List<Job>();

        private bool _showGrid;

        private int _tileXEndDrag;

        private int _tileXStartDrag;
        private int _tileYEndDrag;
        private int _tileYStartDrag;

        public ContentChest ContentChest;

        public SoundManager SoundManager;
        public InputManager InputManager;
        public UIManager UIManager;
        public BuildManager BuildManager;

        public World World;
        
        private List<Tile> _buildTiles = new List<Tile>();

        public Action OnJobsComplete;
        public Action<Tile> OnTileChanged;

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
            InputManager.RegisterOnKeyDown(Keys.Up, MoveCamera);
            InputManager.RegisterOnKeyDown(Keys.Down, MoveCamera);
            InputManager.RegisterOnKeyDown(Keys.Left, MoveCamera);
            InputManager.RegisterOnKeyDown(Keys.Right, MoveCamera);
            InputManager.RegisterOnKeyDown(Keys.OemPeriod, MoveCamera);
            InputManager.RegisterOnKeyDown(Keys.OemComma, MoveCamera);
            InputManager.RegisterOnKeyPress(Keys.Space, ToggleGrid);

            InputManager.MouseHeld += OnMouseHeld;
            InputManager.RightClick += OnMouseRightClick;
            InputManager.MouseReleased += OnMouseReleased;

            BuildManager = new BuildManager(this, InputManager);
            SoundManager = new SoundManager(this, BuildManager);
            UIManager = new UIManager(this, InputManager, BuildManager, SoundManager);

            MediaPlayer.Volume = 0.25f;
            //MediaPlayer.Play(ContentChest.MainMusic);
        }

        public Camera Camera { get; private set; }

        private void ToggleGrid(Keys key)
        {
            _showGrid = !_showGrid;
        }

        private void OnMouseHeld()
        {
            if (BuildManager.GetMode() == BuildMode.Tile ||
                BuildManager.GetMode() == BuildMode.Destroy)
                BuildTiles();
        }

        private void DestroyTiles()
        {

        }

        private void OnMouseRightClick()
        {
            var tile = GetTileAtMouse(Mouse.GetState().X, Mouse.GetState().Y);
            if (tile == null) return;
            CancelTileJob(tile);
        }

        private void CancelTileJob(Tile tile)
        {
            foreach (var job in _activeJobs)
            {
                if (job.Tile != tile) continue;

                job.Cancel();
                _activeJobs.Remove(job);
                return;
            }
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

            var tileX = (int) Math.Floor((decimal) (mouseX)) / TileSize;
            var tileY = (int) Math.Floor((decimal) (mouseY)) / TileSize;
            
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
            CreateJobs();
            _buildTiles = null;
        }

        private void CreateJobs()
        {
            if (_buildTiles == null || _buildTiles.Count == 0) return;

            foreach (var tile in _buildTiles)
            {
                var jobType = JobType.Build;

                if (BuildManager.GetMode() == BuildMode.Destroy)
                    jobType = JobType.Destroy;

                var job = new Job
                {
                    JobType = jobType,
                    RequiredJobTime = 0.2f,
                    Tile = tile,
                    OnJobComplete = JobComplete
                };

                _activeJobs.Add(job);
            }
        }

        private void JobComplete(Job job)
        {
            var jobTile = job.Tile;
            _activeJobs.Remove(job);

            if (job.Cancelled) return;

            if(job.JobType == JobType.Build)
                jobTile.Type = TileType.Ground;
            else if (job.JobType == JobType.Destroy)
                jobTile.Type = TileType.None;

            OnTileChanged?.Invoke(jobTile);

            if (_activeJobs.Count == 0)
                OnJobsComplete?.Invoke();
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
                var tile = GetTileAt(x, y);
                if (tile == null || tile.Type != TileType.None && BuildManager.BuildMode == BuildMode.Tile ||
                tile.Type == TileType.None && BuildManager.BuildMode == BuildMode.Destroy) continue;
                _buildTiles.Add(tile);
            }
        }

        private void MoveCamera(Keys key)
        {
            if (key == Keys.W || key == Keys.Up)
                Camera.Move(0, -CameraSpeed);
            else if (key == Keys.A || key == Keys.Left)
                Camera.Move(-CameraSpeed, 0);
            else if (key == Keys.D || key == Keys.Right)
                Camera.Move(CameraSpeed, 0);
            else if (key == Keys.S || key == Keys.Down) Camera.Move(0, CameraSpeed);
            else if (key == Keys.OemPeriod) Camera.Zoom(ZoomSpeed);
            else if (key == Keys.OemComma) Camera.Zoom(-ZoomSpeed);
        }

        private void SetupCamera()
        {
            var centerX = (MapWidth * TileSize) / 2;
            var centerY = (MapHeight * TileSize) / 2;
            Camera = new Camera(centerX, centerY);
        }

        public void Update(float deltaTime)
        {
            InputManager.Update(deltaTime);

            if (_activeJobs.Count != 0)
                DoJob(deltaTime);
        }

        private void DoJob(float deltaTime)
        {
            var currentJob = _activeJobs[0];
            currentJob.DoWork(deltaTime);

            if (currentJob.Cancelled)
                currentJob.OnJobComplete(currentJob);
        }

        public Tile GetTileAt(int x, int y)
        {
            return World.GetTileAt(x, y);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawWorld(spriteBatch);
            DrawUI(spriteBatch);
        }

        private void DrawWorld(SpriteBatch spriteBatch)
        {

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
                spriteBatch.Draw(ContentChest.TileTextures[tile.Type], new Vector2(tile.X * TileSize, tile.Y * TileSize),
                    Color.White);

                if (_showGrid)
                    spriteBatch.Draw(ContentChest.GridSquare, new Vector2(tile.X * TileSize, tile.Y * TileSize),
                        Color.White);
            }

            if (_buildTiles != null)
                foreach (var tile in _buildTiles)
                    spriteBatch.Draw(ContentChest.Reticle, new Vector2(tile.X * TileSize, tile.Y * TileSize),
                        Color.White);

            if (_activeJobs.Count > 0)
            {
                foreach (var job in new List<Job>(_activeJobs))
                {
                    if (job.Cancelled) continue;

                    var tile = job.Tile;
                    if(job.JobType == JobType.Build)
                        spriteBatch.Draw(ContentChest.TileTextures[TileType.Ground], new Vector2(tile.X * TileSize, tile.Y * TileSize),
                            Color.White * 0.5f);
                    else if (job.JobType == JobType.Destroy)
                        spriteBatch.Draw(ContentChest.Pixel, new Rectangle(tile.X * TileSize, tile.Y * TileSize, TileSize, TileSize),
                            Color.Red * 0.15f);
                }
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