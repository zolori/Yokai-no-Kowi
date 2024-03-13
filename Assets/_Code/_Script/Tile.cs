using _Code._Script.Event;
using UnityEngine;

namespace _Code._Script
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private Color baseColor, greenHoveringColor, redHoveringColor;

        private Vector2 Position { get; set; }

        public Piece Piece { get; set; }

        private void Start()
        {
            Position = transform.position;
        }

        private void OnEnable()
        {
            GameManager.Instance.OnTilePieceChangeEventHandler += SetPieceOnTile;
            Debug.Log(GameManager.Instance);
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnTilePieceChangeEventHandler -= SetPieceOnTile;
        }

        private void SetPieceOnTile(object receiver, EventTilePieceChange e)
        {
            if(receiver.Equals(this))
                Piece = e.NewPiece;
        }
        
        public void SetHoveringColor()
        {
            if (GameManager.Instance.CanMove(GameManager.Instance.CurrSelectedPiece.GetComponent<Piece>(), this) ||
                    GameManager.Instance.CanAirDrop(GameManager.Instance.CurrSelectedPiece.GetComponent<Piece>(), this))
            {
                GetComponent<SpriteRenderer>().color = greenHoveringColor;
                Debug.Log("Green hovering");
            }
            else
            {
                GetComponent<SpriteRenderer>().color = redHoveringColor;
                Debug.Log("Red hovering");

            }
        }
        
        public void SetBaseColor()
        {
            GetComponent<SpriteRenderer>().color = baseColor;
        }

    }
}
