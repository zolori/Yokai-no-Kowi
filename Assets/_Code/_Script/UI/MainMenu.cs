using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Code._Script.UI
{
    public class MainMenu : MonoBehaviour
    {
       
        public void PlayGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
        
        public void BackToMainMenu()
        {
            SceneManager.LoadScene(0);
        }
    }
}
