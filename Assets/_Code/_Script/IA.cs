using UnityEngine;



namespace _Code._Script
{
    public class IA : Player
    {
        public string Name { get; set; }
        public GameObject[] EnemyLastLine { get; set; }

        Tile Player.Play()
        {
            // Calcul du meilleur coup
            // Bouger sur la case
            Tile t = null;
            return t;
        }
    }
}