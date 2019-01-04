using LoM.Constants;
using LoM.Game;
using LoM.Game.Build;
using LoM.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LoM.Game.Jobs;
using LoM.Serialization;

namespace LoM.Managers
{
    public class GameManager
    {
        private const int MapHeight = 50;
        private const int MapWidth = 50;
        private const int TileSize = 32;
        
        private List<Tile> _buildTiles = new List<Tile>();

        private bool _dragging;
        private bool _paused;

        private bool _showGrid;

        private int _tileXEndDrag;

        private int _tileXStartDrag;
        private int _tileYEndDrag;
        private int _tileYStartDrag;
        public BuildManager BuildManager;

        public ContentChest ContentChest;
        public InputManager InputManager;
        public JobManager JobManager;
        public RegionManager RegionManager;

        public Action OnJobsComplete;

        public SoundManager SoundManager;
        public UIManager UIManager;

        public World World;

        private readonly StringBuilder _stringBuilder = new StringBuilder(4);
        private bool _isDestroyWorldObjects;

        public Camera Camera { get; private set; }
        public CameraController CameraController;

        public GameManager(ContentChest contentChest)
        {
            ContentChest = contentChest;

            World = new World(MapWidth, MapHeight);
            FinaliseSetup();
        }


        /// <summary>
        /// Passing a world to the game manager should only ever really happen on map load.
        /// This will allow extra steps to be called in order to set up the load correctly.
        /// This includes recalculating the regions.
        /// </summary>
        public GameManager(ContentChest contentChest, World world)
        {
            ContentChest = contentChest;
            World = world;
            FinaliseSetup();
            RecreateWorld();
        }

        /// <summary>
        /// Some callbacks need to be triggered in order to fully recreate the saved world.
        /// This could change, and we could end up storing data about regions, although will increase
        /// save file size.
        /// </summary>
        private void RecreateWorld()
        {
            foreach (var tile in World.Tiles)
            {
                if(tile.WorldObject != null)
                    RegionManager.OnJobComplete(new Job
                    {
                        JobType = JobType.WorldObject,
                        Tile = tile
                    });
            }
            
        }

        /// <summary>
        /// Anything that needs to happen for both world load, and new world should happen in this method.
        /// This guarantees they will always be called.
        /// </summary>
        private void FinaliseSetup()
        {
            InputManager = new InputManager();
            InputManager.RegisterOnKeyPress(Keys.Space, ToggleGrid);

            InputManager.MouseHeld += OnMouseHeld;
            InputManager.RightClick += OnMouseRightClick;
            InputManager.MouseReleased += OnMouseReleased;

            BuildManager = new BuildManager(this, InputManager);
            SoundManager = new SoundManager(this, BuildManager);

            World.OnTileChanged += SoundManager.TileChanged;
            World.OnTileChanged += OnTileChanged;

            UIManager = new UIManager(this, InputManager, BuildManager, SoundManager);
            JobManager = new JobManager(BuildManager);
            JobManager.OnJobsComplete += SoundManager.JobComplete;
            RegionManager = new RegionManager();

            JobManager.OnJobComplete += RegionManager.OnJobComplete;
            World.OnTileChanged += JobManager.OnTileChanged;
            World.OnJobRequest += JobManager.OnJobRequest;

            SetupCamera();

            SoundManager.PlayMainTrack();
        }

        private void OnTileChanged(Tile obj)
        {
            if (obj?.WorldObject?.MovementCost != 0) return;

            foreach (var character in World.Characters)
            {
                character.InvalidatePath();
            }
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

            // TODO NOT SURE ABOUT THIS.
            if (BuildManager.BuildMode == BuildMode.Destroy)
            {
                _isDestroyWorldObjects = tile.WorldObject != null;
            }

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
                JobManager.CreateDestroyJobs(_buildTiles, _isDestroyWorldObjects);
            else if (BuildManager.BuildMode == BuildMode.WorldObject)
                JobManager.CreateBuildJobs(_buildTiles);

            _isDestroyWorldObjects = false;
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
                // TODO CHECK THE OBJECT TYPE FOR THIS. We might want only walls to be "hollow placed", for rooms.
                if (BuildManager.BuildMode == BuildMode.WorldObject)
                    if(x != xStart && x != xEnd && y != yStart && y != yEnd) continue;
                var tile = World.GetTileAt(x, y);

                if (_buildTiles.Count == 0 && tile.WorldObject != null && BuildManager.BuildMode == BuildMode.Destroy)
                    _isDestroyWorldObjects = true;

                if (tile == null || tile.Type != TileType.None && BuildManager.BuildMode == BuildMode.Tile ||
                    (tile.Type == TileType.None && tile.WorldObject == null) && BuildManager.BuildMode == BuildMode.Destroy || (tile.WorldObject != null &&
                    BuildManager.BuildMode == BuildMode.WorldObject)) continue;

                if (BuildManager.BuildMode == BuildMode.WorldObject && tile.Type == TileType.None)
                        continue;

                if (_isDestroyWorldObjects && tile.WorldObject == null) continue; // TODO DUNNO?

                _buildTiles.Add(tile);
            }
        }

        public void Update(float deltaTime)
        {
            InputManager.Update(deltaTime);

            if (_paused) return;
            World.Update(deltaTime);
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

            /*for (var i = renderStartX; i < renderEndX; i++)
            for (var j = renderStartY; j < renderEndY; j++)
            {*/ foreach( var tile in World.Tiles) { 
                //var tile = World.Tiles[i, j];

                if (_showGrid && tile.Type != TileType.None)
                    spriteBatch.Draw(ContentChest.GridSquare, new Vector2(tile.X * TileSize, tile.Y * TileSize),
                        Color.White);

                spriteBatch.Draw(ContentChest.TileTextures[tile.Type],
                new Vector2(tile.X * TileSize, tile.Y * TileSize),
                Color.White);

                if (_showGrid && tile.Type == TileType.None)
                    spriteBatch.Draw(ContentChest.GridSquare, new Vector2(tile.X * TileSize, tile.Y * TileSize),
                        Color.White);

                if (tile.WorldObject != null)
                    objects.Enqueue(tile);
            }

            while (objects.Count > 0)
            {
                var worldObject = objects.Dequeue().WorldObject;

                DrawWorldObject(spriteBatch, worldObject);
            }

            if (_buildTiles != null)
                foreach (var tile in _buildTiles)
                    spriteBatch.Draw(ContentChest.Reticle, new Vector2(tile.X * TileSize, tile.Y * TileSize),
                        Color.White);

            if (JobManager.JobCount() > 0)
                foreach (var job in new List<Job>(JobManager.GetJobs()))
                {
                    if (job.Cancelled) continue;

                    var jobCompletion = (job.JobTime / job.RequiredJobTime);
                    var coverHeight = (int)(jobCompletion * TileSize);
                    
                    var tile = job.Tile;

                    var isAssigned = job.Assigned;

                    if (job.JobType == JobType.Build)
                    {
                        spriteBatch.Draw(ContentChest.TileTextures[TileType.Ground],
                            new Vector2(tile.X * TileSize, tile.Y * TileSize),
                            Color.White * 0.2f);
                        spriteBatch.Draw(ContentChest.Pixel,
                            new Rectangle(tile.X * TileSize, tile.Y * TileSize + TileSize - coverHeight, TileSize, coverHeight),
                            Color.Green * 0.3f);
                    }
                    else if (job.JobType == JobType.WorldObject)
                    {
                        spriteBatch.Draw(ContentChest.Pixel,
                            new Rectangle(tile.X * TileSize, tile.Y * TileSize, TileSize, TileSize),
                            Color.White * 0.2f);
                        spriteBatch.Draw(ContentChest.Pixel,
                            new Rectangle(tile.X * TileSize, tile.Y * TileSize + TileSize - coverHeight, TileSize, coverHeight),
                            Color.Green * 0.3f);
                    }
                    else if (job.JobType == JobType.DestroyWorldObject || job.JobType == JobType.DestroyTile)
                        spriteBatch.Draw(ContentChest.Pixel,
                            new Rectangle(tile.X * TileSize, tile.Y * TileSize + TileSize - coverHeight, TileSize, coverHeight),
                            Color.Red * 0.15f);
/*

                    if (!isAssigned) continue;

                    var sprite = ContentChest.CharacterTypes[job.Assignee.CharacterType];
                    spriteBatch.Draw(sprite,
                        new Vector2(tile.X * 32 + 16 - sprite.Width / 2,
                            tile.Y * 32 + 16 - sprite.Height / 2), Color.White * 0.4f);*/
                }


            var mouseX = Mouse.GetState().X;
            var mouseY = Mouse.GetState().Y;

            var mouseTile = GetTileAtMouse(mouseX, mouseY);

            if (mouseTile != null)
            {
                spriteBatch.Draw(ContentChest.HoverSquare, new Vector2(mouseTile.X * TileSize, mouseTile.Y * TileSize),
                    Color.White);
                if(mouseTile.Region != null && Keyboard.GetState().IsKeyDown(Keys.LeftShift))
                    foreach (var tile in mouseTile.Region.Tiles)
                    {
                        if(mouseTile.Region.SpaceSafe)
                            spriteBatch.Draw(ContentChest.Pixel, new Rectangle(tile.X * TileSize, tile.Y * TileSize, TileSize, TileSize),
                                Color.Green * 0.6f);
                        else
                            spriteBatch.Draw(ContentChest.Pixel, new Rectangle(tile.X * TileSize, tile.Y * TileSize, TileSize, TileSize),
                                Color.Pink * 0.6f);
                    }
            }


            foreach (var c in World.Characters.OrderBy(character => character.Tile.Y))
            {

                DrawCharacter(spriteBatch, c);
            }


            spriteBatch.End();
        }

        private void DrawCharacter(SpriteBatch spriteBatch, Character character)
        {
            float drawX = character.Tile.X * 32;
            float drawY = character.Tile.Y * 32;
            var targetX = drawX;
            var targetY = drawY;

            if (character.TargetTile != null)
            {
                targetX = character.TargetTile.X * 32;
                targetY = character.TargetTile.Y * 32;
            }

            drawX -= (drawX - targetX) * character.MovementPercentage;
            drawY -= (drawY - targetY) * character.MovementPercentage;


            spriteBatch.Draw(ContentChest.CharacterTypes[character.CharacterType], new Rectangle((int)drawX, (int)drawY, TileSize, TileSize), Color.White);

            if (character.Tile.Region?.SpaceSafe == false || character.Tile.Region == null)
            {
                spriteBatch.Draw(ContentChest.Helmet, new Rectangle((int)drawX, (int)drawY, TileSize, TileSize), Color.White);
            }

            var text = character.CharacterType;
            var textWidth = ContentChest.MainFont.MeasureString(text).X;
            spriteBatch.DrawString(ContentChest.MainFont, text, new Vector2(drawX + 16 - textWidth / 2, drawY - 10), Color.White);
        }

        private void DrawWorldObject(SpriteBatch spriteBatch, WorldObject worldObject)
        {
            var objectType = worldObject.ObjectType.ToString();
            var name = $"{objectType}_";

            if (worldObject.MergesWithNeighbors)
            {
                var neighborString = CreateNeighborString(worldObject);
                if(!string.IsNullOrWhiteSpace(neighborString))
                    name = $"{objectType}_{neighborString}";
            }
            spriteBatch.Draw(ContentChest.WorldObjects[name],
                new Vector2(worldObject.Tile.X * TileSize, worldObject.Tile.Y * TileSize),
                Color.White);
        }

        private string CreateNeighborString(WorldObject worldObject)
        {
            _stringBuilder.Clear();

            var tile = worldObject.Tile;
            var tileX = tile.X;
            var tileY = tile.Y;
            var northTile = World.GetTileAt(tileX, tileY - 1);
            var eastTile = World.GetTileAt(tileX + 1, tileY);
            var southTile = World.GetTileAt(tileX, tileY + 1);
            var westTile = World.GetTileAt(tileX - 1, tileY);

            if (northTile?.WorldObject?.ObjectType == worldObject.ObjectType)
                _stringBuilder.Append("N");
            if (eastTile?.WorldObject?.ObjectType == worldObject.ObjectType)
                _stringBuilder.Append("E");
            if (southTile?.WorldObject?.ObjectType == worldObject.ObjectType)
                _stringBuilder.Append("S");
            if (westTile?.WorldObject?.ObjectType == worldObject.ObjectType)
                _stringBuilder.Append("W");

            return _stringBuilder.ToString();
        }

        private void DrawUI(SpriteBatch spriteBatch)
        {
            UIManager.Draw(spriteBatch);
        }

        public void Pause(bool paused)
        {
            _paused = paused;
        }

        public void SaveGame()
        {
            GameSaver.SaveGame(World, "game");
        }

    }
}