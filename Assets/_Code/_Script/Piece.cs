using UnityEngine;

namespace _Code._Script
{
    [RequireComponent(typeof(Collider2D), typeof(DragAndDrop))]
    public abstract class Piece : MonoBehaviour
    {
        private string _name;
        [SerializeField] private Player _player;
        private Sprite _sprite;

        protected Piece(Player player, Tile iTileToSpawn)
        {
            _player = player;
            GameManager.Instance.SetPieceAndMoveToParent(this, iTileToSpawn);
        }

        public Vector2[] VectorMovements { get; protected set; }

        public bool bIsFromPile = false;

        public Player Player
        {
            get => _player;
            set => _player = value;
        }
    }
}