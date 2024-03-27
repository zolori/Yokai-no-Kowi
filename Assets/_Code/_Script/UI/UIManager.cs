
using _Code._Script.Enums;
using _Code._Script.Event;
using _Code._Script.Sound;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Code._Script.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerTurnText, winnerText;
        [SerializeField] private GameObject victoryScreen, drawScreen;
        [SerializeField] private Slider musicSlider, sfxSlider;


        private void Start()
        {
            InitUI();
        }

        public void InitUI()
        {
            victoryScreen.SetActive(false);
            drawScreen.SetActive(false);
            playerTurnText.gameObject.SetActive(true);
        }
        
        private void OnEnable()
        {
            GameManager.Instance.GameOverEventHandler += DisplayGameOverScreen;
        }

        public void DisplayPlayerTurnText(string playerName)
        {
            playerTurnText.text = $"Turn : {playerName}";
        }

        public void DisplayGameOverScreen(object sender, EventGameOver e)
        {
            Time.timeScale = 0;
            switch (e.CurrState)
            {
                case EGameOverState.None:
                    
                    break;
                case EGameOverState.Draw:
                    drawScreen.SetActive(true);
                    playerTurnText.gameObject.SetActive(false);
                    break;
                case EGameOverState.Victory:
                    victoryScreen.SetActive(true);
                    winnerText.text = e.Winner;
                    playerTurnText.gameObject.SetActive(false);
                    break;
            }
        }

        public void RestartGame()
        {
            GameManager.Instance.InitGame();
        }
        
        public void BackToMainMenu()
        {
            SceneManager.LoadScene(0);
        }
        
        #region Sounds
        
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
            AudioManager.Instance.MusicVolume(musicSlider.value);
        }
        
        public void SfxVolume()
        {
            AudioManager.Instance.SfxVolume(sfxSlider.value);
        }
        
        #endregion

    
    }
}
