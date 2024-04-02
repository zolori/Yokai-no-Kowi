using System;
using System.Collections.Generic;
using System.Linq;
using _Code._Script.Event;
using UnityEngine;

namespace _Code._Script
{
    public class IA : IPlayer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool BSameThreeLastMove { get; set; }
        public List<Piece> PossessedPieces { get; set; }
        public List<Vector2> LastThreeMove { get; set; }
        public GameObject[] EnemyLastLine { get; set; }
        public bool isPlaying { get; set; }

        private GameManager _gameManager = GameManager.Instance;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="enemyLastLine"></param>
        public IA(int id, string name, GameObject[] enemyLastLine)
        {
            Id = id;
            Name = name;
            EnemyLastLine = enemyLastLine;
            GameManager.Instance.OnPieceMovedEventHandler += SetLastMovement;
            isPlaying = false;
            PossessedPieces = new List<Piece>();
        }

        public void SetLastMovement(object receiver, EventPlayerMovement e)
        {
            if (receiver.Equals(this))
            {
                if (LastThreeMove.Count < 3)
                {
                    LastThreeMove.Insert(0, e.VectorMovement);
                }
                else
                {
                    LastThreeMove.RemoveAt(2);
                    LastThreeMove.Insert(0, e.VectorMovement);
                }

                if (LastThreeMove.Count == 3)
                    CompareThreeLastMove();
            }
        }

        public void CompareThreeLastMove()
        {
            var vectorToTest1 = LastThreeMove.ElementAt(0);
            var vectorToTest2 = LastThreeMove.ElementAt(1);
            var vectorToTest3 = LastThreeMove.ElementAt(2);
            vectorToTest2 *= -1;

            if (vectorToTest1 == vectorToTest3 && vectorToTest1 == vectorToTest2)
                BSameThreeLastMove = true;
            else
                BSameThreeLastMove = false;
        }

        protected virtual Piece ChooseAPieceToMove()
        {
            // TODO: Pick a random piece from the possessed pieces list
            // TODO: And try to check the better move with it

            throw new NotImplementedException();
        }

        protected virtual Vector2 MoveToDo(Piece iPieceToMove)
        {
            // TODO: Pick a random movement according to the piece
            // TODO: Or Air Drop if the piece is in the pile

            throw new NotImplementedException();
        }

        public float MinMax(int depth, bool maximizingPlayer)
        {
            Debug.Log("Minmax --> depth : " + depth + " + max : " + maximizingPlayer);

            IPlayer opponent = _gameManager.getPlayerThatsNotHisTurn();

            if (depth == 0 || _gameManager.CheckWin() != -2)
            {
                Debug.Log("feuille ou GameOver");
                return _gameManager.EvaluateBoard();
            }

            if (maximizingPlayer)
            {
                float maxEval = int.MinValue;
                foreach (var move in _gameManager.GetLegalMoves(this))
                {
                    _gameManager.ApplyMove(move, this);
                    float eval = MinMax(depth - 1, false);
                    maxEval = Math.Max(maxEval, eval);
                    _gameManager.UndoMove(move);
                }
                return maxEval;
            }
            else
            {
                float minEval = int.MaxValue;
                foreach (var move in _gameManager.GetLegalMoves(opponent))
                {
                    _gameManager.ApplyMove(move, opponent);
                    float eval = MinMax(depth - 1, true);
                    minEval = Math.Min(minEval, eval);
                    _gameManager.UndoMove(move);
                }
                return minEval;
            }
        }
    }
}