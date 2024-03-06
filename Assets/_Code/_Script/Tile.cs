using System;
using UnityEngine;

namespace _Code._Script
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private Color baseColor, greenHoveringColor, redHoveringColor;
        
        public Vector2 position;
        public Piece piece;

        private void Start()
        {
            position = transform.position;
        }

        public void SetHoveringColor()
        {
            Debug.Log("Tile Enter "+ position.x+", "+ position.y);
            if (GameManager.Instance.currSelectedPiece == null || 
                GameManager.Instance.currSelectedPiece.GetComponentInParent<Tile>() == this)
                return;
            
            if (GameManager.Instance.CanMove(GameManager.Instance.currSelectedPiece.GetComponent<Piece>(), this) 
                || GameManager.Instance.currSelectedPiece.GetComponent<Piece>().bIsFromPile)
            {
                GetComponent<SpriteRenderer>().color = greenHoveringColor;
            }
            else
            {
                Debug.Log("help");
                GetComponent<SpriteRenderer>().color = redHoveringColor;
            }
        }
        
        public void SetBaseColor()
        {
            GetComponent<SpriteRenderer>().color = baseColor;
        }

        /// <summary>
        /// CALL THE MOVE FUNCTION FROM THE GAME MANAGER
        /// </summary>
        /// <param name="iPiece"></param>
        public void DragEnd(Piece iPiece)
        {
            GameManager.Instance.currSelectedPiece = null;
        }
    }
}
