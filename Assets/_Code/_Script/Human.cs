using UnityEngine;

namespace _Code._Script
{
    public class Human : Player
    {
        public string Name { get; set; }
        public GameObject[] EnemyLastLine { get; set; }

        Tile Player.Play()
        {
            Tile t = null;
            return t;
        }
    }
}