using UnityEngine;

namespace _Code._Script
{
    [RequireComponent(typeof(Collider2D), typeof(DragAndDrop))]
    public abstract class Piece : MonoBehaviour
    {
        private IPlayer _player;
        private string _name;
        private Sprite _sprite;
        public string _ownerName;

        public float Value { get; set; }

        protected Piece(IPlayer player, Tile iTileToSpawn)
        {
            _player = player;
            _ownerName = player.Name;
            GameManager.Instance.SetPieceAndMoveToParent(this, iTileToSpawn);
        }

        public Vector2[] VectorMovements { get; protected set; }

        public bool bIsFromPile = false;

        public IPlayer Player
        {
            get => _player;
            set => _player = value;
        }

        public void ChangePiecePlayer(IPlayer newPlayer)
        {
            _player = newPlayer;
            transform.Rotate(0, 0, 180f);
        }
    }
}