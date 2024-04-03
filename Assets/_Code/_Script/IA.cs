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
            LastThreeMove = new List<Vector2>();
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

        public float MinMax(int depth, bool maximizingPlayer)
        {
            IPlayer opponent = _gameManager.getPlayerThatsNotHisTurn();
            KeyValuePair<Piece, Vector2> _bestMove = GameManager.Instance.bestMove;

            if (depth == 0 || _gameManager.CheckWin() != -2)
            {
                return _gameManager.EvaluateBoard();
            }

            if (maximizingPlayer)
            {
<<<<<<< HEAD
=======
                Debug.Log("*********IA");
>>>>>>> parent of e77495a (WIP async)
                float maxEval = int.MinValue;
                foreach (var moves in _gameManager.GetLegalMoves(this))
                {
                    foreach (Vector2 move in moves.Value)
                    {
                        KeyValuePair<Piece, Vector2> mouvement = new KeyValuePair<Piece, Vector2>(moves.Key, move);
<<<<<<< HEAD
                        _bestMove = new KeyValuePair<Piece, Vector2>(mouvement.Key, mouvement.Value);
                        await _gameManager.ApplyMove(mouvement, this);
                        float eval = await MinMax(depth - 1, false);
                        maxEval = Math.Max(maxEval, eval);
                        Debug.Log("MINMAX***  Piece : " + _bestMove.Key + " , déplacement : " + _bestMove.Value + ", et le coup vaut : " + eval);
                        await _gameManager.UndoMove(mouvement);
=======
                        _gameManager.ApplyMove(mouvement, this);
                        float eval = MinMax(depth - 1, false);
                        maxEval = Math.Max(maxEval, eval);
                        _gameManager.UndoMove(mouvement);
>>>>>>> parent of e77495a (WIP async)
                    }
                }
                return maxEval;
            }
            else
            {
                float minEval = int.MaxValue;
                foreach (var moves in _gameManager.GetLegalMoves(opponent))
                {
                    foreach(Vector2 move in moves.Value)
                    {
                        KeyValuePair<Piece, Vector2> mouvement = new KeyValuePair<Piece, Vector2>(moves.Key, move);
<<<<<<< HEAD
                        await _gameManager.ApplyMove(mouvement, opponent);
                        float eval = await MinMax(depth - 1, true);
=======
                        _gameManager.ApplyMove(mouvement, opponent);
                        float eval = MinMax(depth - 1, true);
>>>>>>> parent of e77495a (WIP async)
                        minEval = Math.Min(minEval, eval);
                        await _gameManager.UndoMove(mouvement);
                    }
                }
                return minEval;
            }
        }
    }
}