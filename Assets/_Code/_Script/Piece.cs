using UnityEngine;
using UnityEngine.Serialization;

namespace _Code._Script
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class Piece : MonoBehaviour
    {
        private int _id;
        private IPlayer _player;
        private string _name;
        private Sprite _sprite;
        public string ownerName;

        public float Value { get; set; }

        protected Piece(IPlayer player, Tile iTileToSpawn, int iId)
        {
            _player = player;
            ownerName = player.Name;
            GameManager.Instance.SetPieceAndMoveToParent(this, iTileToSpawn);
            _id = iId;
        }

        public Vector2[] VectorMovements { get; protected set; }

        public bool bIsFromPile = false;

        public int ID
        {
            get => _id;
            set => _id = value;
        }
        
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