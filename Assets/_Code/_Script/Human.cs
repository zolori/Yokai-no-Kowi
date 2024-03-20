using System;
using System.Collections.Generic;
using System.Linq;
using _Code._Script.Event;
using UnityEngine;

namespace _Code._Script
{
    public class Human : IPlayer
    {
        public Human(int id, string name, GameObject[] enemyLastLine)
        {
            Id = id;
            Name = name;
            EnemyLastLine = enemyLastLine;
            LastThreeMove = new List<Vector2>();
            GameManager.Instance.OnPieceMovedEventHandler += SetLastMovement;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool BSameThreeLastMove { get; set; }
        public List<Piece> PossessedPieces { get; set; }
        public List<Vector2> LastThreeMove { get; set; }
        public GameObject[] EnemyLastLine { get; set; }

        Tile IPlayer.Play()
        {
            Tile t = null;
            return t;
        }
        
        /// <summary>
        /// RECORD THE LAST THREE MOVEMENTS DONE BY THE PLAYER
        /// </summary>
        /// <param name="receiver">The current player playing to assign the right list</param>
        /// <param name="e"></param>
        public void SetLastMovement(object receiver, EventPlayerMovement e)
        {
            /*if (receiver.Equals(this))
            {
                if (LastThreeMove.Count < 2)
                {
                    LastThreeMove.Insert(0, e.VectorMovement);
                }
                else
                {
                    LastThreeMove.RemoveAt(2);
                    LastThreeMove.Insert(0, e.VectorMovement);
                }
            }*/
        }

        public void CompareThreeLastMove()
        {
            // TODO: compare the last three move
            // TODO: If yes : BSameLastThreeMove = true
            // TODO: If no : pass
        }
    }
}