using UnityEngine;

namespace _Code._Script
{
    [RequireComponent(typeof(Collider2D), typeof(DragAndDrop))]
    public abstract class Piece : MonoBehaviour
    {
        private string _name;
        private Player _player;
        private Sprite _sprite;
        
        public bool bIsFromPile = false;

        public Player Player
        {
            get => _player;
            set => _player = value;
        }
    }
}