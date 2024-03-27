using _Code._Script.Sound;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Code._Script.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerTurnText, winnerText;
        [SerializeField] private GameObject victoryScreen;
        [SerializeField] private Slider _musicSlider, _sfxSlider;


        private void Start()
        {
            victoryScreen.SetActive(false);
        }

        public void DisplayPlayerTurnText(string playerName)
        {
            playerTurnText.text = $"Turn : {playerName}";
        }

        public void DisplayVictoryScreen(string winner)
        {
            victoryScreen.SetActive(true);
            winnerText.text = winner;
        }

        public void ToggleMusic()
        {
            AudioManager.Instance.ToggleMusic();
        }
        
        public void ToggleSfx()
        {
            AudioManager.Instance.ToggleSfx();
        }

        public void MusicVolume()
        {
            AudioManager.Instance.MusicVolume(_musicSlider.value);
        }
        
        public void SfxVolume()
        {
            AudioManager.Instance.SfxVolume(_sfxSlider.value);
        }
    
    }
}
