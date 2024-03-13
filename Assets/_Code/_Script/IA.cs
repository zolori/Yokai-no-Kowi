using UnityEngine;



namespace _Code._Script
{
    public class IA : IPlayer
    {
        public string Name { get; set; }
        public GameObject[] EnemyLastLine { get; set; }

        Tile IPlayer.Play()
        {
            // Calcul du meilleur coup
            // Bouger sur la case
            Tile t = null;
            return t;
        }
    }
}