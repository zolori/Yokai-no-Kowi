using UnityEngine;


namespace _Code._Script
{
    public interface Player
    {
        string Name { get; set; }
        
        GameObject[] EnemyLastLine { get; set; }
        
        public Tile Play();
    }
}

