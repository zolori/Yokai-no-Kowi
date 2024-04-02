using System.Collections.Generic;
using _Code._Script.Event;
using UnityEngine;


namespace _Code._Script
{
    public interface IPlayer
    {
        int Id { get; set; }
        string Name { get; set; }

        bool BSameThreeLastMove { get; set; }
        
        List<Piece> PossessedPieces { get; set; }
        
        List<Vector2> LastThreeMove { get; set; }
        
        GameObject[] EnemyLastLine { get; set; }

        public bool isPlaying { get; set; }

        void SetLastMovement(object receiver, EventPlayerMovement e);

        void CompareThreeLastMove();
    }
}

