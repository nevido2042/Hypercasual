using UnityEngine;
using UnityEngine.UI;

namespace Hero
{
    public class VolumeToggle : MonoBehaviour
    {
        public Image buttonImage;
        public Sprite volumeOnSprite;
        public Sprite volumeOffSprite;

        private bool isMuted = false;

        private void Start()
        {
            if (buttonImage == null) buttonImage = GetComponent<Image>();
        }

        public void ToggleVolume()
        {
            isMuted = !isMuted;
            AudioListener.pause = isMuted;

            if (buttonImage != null)
            {
                buttonImage.sprite = isMuted ? volumeOffSprite : volumeOnSprite;
            }
        }
    }
}
