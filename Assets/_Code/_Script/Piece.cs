using UnityEngine;

namespace _Code._Script
{
    public abstract class Piece : MonoBehaviour
    {
        private string _name;
        private string _player;
        private Sprite _sprite;
        
        public bool bIsFromPile = false;

        public string Player
        {
            get => _player;
            set => _player = value;
        }
    }
}