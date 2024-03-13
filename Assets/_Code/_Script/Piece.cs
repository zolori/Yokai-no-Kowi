using UnityEngine;

namespace _Code._Script
{
    [RequireComponent(typeof(Collider2D), typeof(DragAndDrop))]
    public abstract class Piece : MonoBehaviour
    {
        [SerializeField] private IPlayer _player;
        public int TileNumberInPile { get; set; }
        private string _name;
        private Sprite _sprite;


        protected Piece(IPlayer player, Tile iTileToSpawn)
        {
            _player = player;
            GameManager.Instance.SetPieceAndMoveToParent(this, iTileToSpawn);
        }

        public Vector2[] VectorMovements { get; protected set; }

        public bool bIsFromPile = false;

        public IPlayer Player
        {
            get => _player;
            set => _player = value;
        }

        public void changePlayer(IPlayer newPlayer)
        {
            _player = newPlayer;
            transform.Rotate(0, 0, 180f);
        }
    }
}