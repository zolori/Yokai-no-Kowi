using System;
using UnityEngine;

namespace _Code._Script
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private Color baseColor, greenHoveringColor, redHoveringColor;
        
        public Vector2 position;
        public Piece piece;

        private void OnMouseEnter()
        {
            if (GameManager.Instance.currSelectedPiece == null)
                return;

            if (GameManager.Instance.CanMove(GameManager.Instance.currSelectedPiece.GetComponent<Piece>(), this) 
                || GameManager.Instance.currSelectedPiece.GetComponent<Piece>().bIsFromPile)
            {
                GetComponent<SpriteRenderer>().color = greenHoveringColor;
            }
            else
            {
                GetComponent<SpriteRenderer>().color = redHoveringColor;
            }
        }

        private void OnMouseExit()
        {
            GetComponent<SpriteRenderer>().color = baseColor;
        }

        /// <summary>
        /// CALL THE MOVE FUNCTION FROM THE GAME MANAGER
        /// </summary>
        /// <param name="iPiece"></param>
        public void DragEnd(Piece iPiece)
        {
            GameManager.Instance.Move(iPiece, this);
        }
    }
}
