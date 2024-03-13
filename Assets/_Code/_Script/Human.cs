using UnityEngine;

namespace _Code._Script
{
    public class Human : IPlayer
    {
        public string Name { get; set; }
        public GameObject[] EnemyLastLine { get; set; }

        Tile IPlayer.Play()
        {
            Tile t = null;
            return t;
        }
    }
}