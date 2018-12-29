namespace LoM
{
    public class SoundManager
    {

        private readonly GameManager _gameManager;

        private ContentChest ContentChest => _gameManager.ContentChest;

        public SoundManager(GameManager gameManager, BuildManager buildManager)
        {
            _gameManager = gameManager;

            buildManager.OnBuildModeChange += UIInteraction;
            gameManager.OnTileChanged += TileChanged;
            gameManager.OnJobsComplete += JobComplete;
        }

        private void JobComplete()
        {
            ContentChest.SuccessSound.Play();
        }

        private void TileChanged(Tile tile)
        {
            ContentChest.BuildSound.Play();
        }

        private void UIInteraction()
        {
            ContentChest.BuildSound.Play();
        }

    }
}