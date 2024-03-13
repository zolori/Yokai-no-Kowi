using UnityEngine;


namespace _Code._Script
{
    public interface IPlayer
    {
        string Name { get; set; }
        
        GameObject[] EnemyLastLine { get; set; }
        
        public Tile Play();
    }
}

