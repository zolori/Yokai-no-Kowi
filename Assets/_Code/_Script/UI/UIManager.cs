using TMPro;
using UnityEngine;

namespace _Code._Script.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerTurnText, winnerText;
        [SerializeField] private GameObject victoryScreen;


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
    
    }
}
