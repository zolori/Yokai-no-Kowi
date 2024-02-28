using UnityEngine;

namespace _Code._Script
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private Color baseColor;
        [SerializeField] private Color hoveringColor;
        
        public Vector2 position;
        public Piece piece;

        private void OnMouseEnter()
        {
            // TODO: SET HIGHLIGHT COLOR
        }

        private void OnMouseExit()
        {
            // TODO : REMOVE HIGHLIGHT COLOR
        }
        
        public void DragEnd(Piece iPiece)
        {
            // TODO : CALL THE MOVE FUNCTION FROM THE GAME MANAGER
            GameManager.Instance.Move(iPiece, this);
        }
    }
}
