using LoM.Game;
using LoM.Game.Build;
using LoM.UI;
using LoM.Util;
using Microsoft.Xna.Framework.Media;

namespace LoM.Managers
{
    public class SoundManager
    {

        private readonly GameManager _gameManager;

        private ContentChest ContentChest => _gameManager.ContentChest;

        public SoundManager(GameManager gameManager, BuildManager buildManager)
        {
            _gameManager = gameManager;
            gameManager.OnJobsComplete += JobComplete;
        }

        public void JobComplete()
        {
            ContentChest.SuccessSound.Play();
        }

        public void TileChanged(Tile tile)
        {
            ContentChest.BuildSound.Play();
        }

        public void UIInteraction()
        {
            ContentChest.BuildSound.Play();
        }

        public void OnButtonClick(UIElement element)
        {
            ContentChest.BuildSound.Play();
        }

        public void PlayMainTrack()
        {
            MediaPlayer.Volume = 0.1f;
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(ContentChest.MainMusic);
        }

    }
}