using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Code._Script.UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private GameObject mainMenu, pvpMenu;
        [SerializeField] private TextMeshProUGUI player1InputField, player2InputField;


        public void PlayPVP()
        {
            PlayerPrefs.SetInt("GameMode", 1);
            PlayerPrefs.Save();

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        public void PlayPVM()
        {
            PlayerPrefs.SetInt("GameMode", 2);
            PlayerPrefs.Save();

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }


        public void QuitGame()
        {
            Application.Quit();
        }

        public void ShowPvpMenu()
        {
            mainMenu.SetActive(false);
            pvpMenu.SetActive(true);
        }
        
        public void BackButton()
        {
            pvpMenu.SetActive(false);
            mainMenu.SetActive(true);
        }


        
        // NOT WORKING, MUST BE FIX
        /*public void OnPlayerNameChanged(int pPlayerindex)
        {
            if(pPlayerindex == 1)
                GameManager.Instance.Player1.Name = player1InputField.text;

            if (pPlayerindex == 2)
                GameManager.Instance.Player2.Name = player2InputField.text;
        }*/
        
    }
}
