using System;
using System.Collections.Generic;
using System.Linq;
using _Code._Script.Event;
using TMPro;
using UnityEngine;

namespace _Code._Script
{
    public class IA : IPlayer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool BSameThreeLastMove { get; set; }
        public Dictionary<int, Piece> PossessedPieces { get; set; }
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
            PossessedPieces = new Dictionary<int, Piece> { };
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

        /// <summary>
        /// Algo MinMax
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="maximizingPlayer"></param>
        /// <param name="bestMove"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public float MinMax(int depth, bool maximizingPlayer, ref KeyValuePair<Piece, KeyValuePair<Vector2, int>> bestMove, ref int node)
        {
            node++;
            IPlayer opponent = _gameManager.getPlayerThatsNotHisTurn();

            if (depth == 0 || _gameManager.CheckWin() != -2)
            {
                node--;
                return _gameManager.EvaluateBoard();
            }

            if (maximizingPlayer)
            {
                float maxEval = int.MinValue;

                Dictionary<Piece, List<Vector2>> clearedLegalMoves = ClearGetLegalMoves(_gameManager.GetLegalMoves(this));
                foreach (var moves in clearedLegalMoves)
                {
                    foreach (Vector2 move in moves.Value)
                    {
                        KeyValuePair<Vector2, int> pair = new KeyValuePair<Vector2, int>(move, node);
                        KeyValuePair<Piece, KeyValuePair<Vector2, int>> mouvement = new KeyValuePair<Piece, KeyValuePair<Vector2, int>>(moves.Key, pair);
                        KeyValuePair<Piece, Vector2> truc = new KeyValuePair<Piece, Vector2>(mouvement.Key, mouvement.Value.Key);

                        _gameManager.ApplyMove(truc, this);
                        float eval = MinMax(depth - 1, false, ref bestMove, ref node);
                        maxEval = Math.Max(maxEval, eval);
                        bestMove = new KeyValuePair<Piece, KeyValuePair<Vector2, int>>(mouvement.Key, mouvement.Value);
                        //Debug.Log("MINMAX***  Piece : " + bestMove.Key + " , déplacement : " + bestMove.Value + ", et le coup vaut : " + eval);
                        _gameManager.UndoMove(truc);
                    }
                }
                node--;
                return maxEval;
            }
            else
            {
                float minEval = int.MaxValue;
                foreach (var moves in _gameManager.GetLegalMoves(opponent))
                {
                    foreach (Vector2 move in moves.Value)
                    {
                        KeyValuePair<Vector2, int> pair = new KeyValuePair<Vector2, int>(move, node);
                        KeyValuePair<Piece, KeyValuePair<Vector2, int>> mouvement = new KeyValuePair<Piece, KeyValuePair<Vector2, int>>(moves.Key, pair);
                        KeyValuePair<Piece, Vector2> truc = new KeyValuePair<Piece, Vector2>(mouvement.Key, mouvement.Value.Key);

                        _gameManager.ApplyMove(truc, opponent);
                        float eval = MinMax(depth - 1, true, ref bestMove, ref node);
                        minEval = Math.Min(minEval, eval);
                        _gameManager.UndoMove(truc);
                    }
                }
                node--;
                return minEval;
            }
        }

        private Dictionary<Piece, List<Vector2>> ClearGetLegalMoves(Dictionary<Piece, List<Vector2>> moves)
        {
            Dictionary<Piece, List<Vector2>> newLegalMoves = new Dictionary<Piece, List<Vector2>>();

            foreach (var move in moves)
            {
                if (move.Key == null)
                    continue;

                List<Vector2> tmp = new List<Vector2>();
                foreach (var mouvement in move.Value)
                {
                    Tile nextTile = _gameManager.GetTileToMove(move.Key, mouvement);
                    var t = nextTile.Position;
                    if (!(t.x < 0 || t.x > 2 || t.y < 0 || t.y > 3))
                        tmp.Add(mouvement);
                }
                if (tmp.Count > 0)
                    newLegalMoves.Add(move.Key, tmp);
            }

            return newLegalMoves;
        }
    }
}