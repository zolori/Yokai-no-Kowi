using System;
using System.Collections.Generic;
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

        private GameManager _gameManager;

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
        }

        Tile IPlayer.Play()
        {
            // TODO: Choose a piece to move
            GameManager.Instance.FinishTurn();
            Tile t = null;
            return t;
        }

        public void SetLastMovement(object receiver, EventPlayerMovement e)
        {
            throw new NotImplementedException();
        }

        public void CompareThreeLastMove()
        {
            throw new NotImplementedException();
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

        /// <summary>
        /// GET A REFERENCE TO THE TILE WHERE THE IA TRY TO MOVE THE PIECE
        /// </summary>
        /// <param name="iPieceToMove"></param>
        /// <param name="iVectorMovement"></param>
        /// <returns>The reference to the tile hit by the raycast</returns>
        private Tile GetTileToMove(Piece iPieceToMove, Vector2 iVectorMovement)
        {
            var currPieceTile = iPieceToMove.GetComponentInParent<Transform>().position;
            if(iPieceToMove.Player == GameManager.Instance.Player2)
                iVectorMovement *= -1;
            
            var xPos = currPieceTile.x + iVectorMovement.x;
            var yPos = currPieceTile.y + iVectorMovement.y;
            var originPos = new Vector2(xPos, yPos);

            // Start a raycast with the current piece + movement vector as origin
            RaycastHit2D[] hits = Physics2D.RaycastAll(originPos, Vector2.zero);

            // Filter the result to get the first object with Tile script component on it, ignore other gameobject
            foreach (RaycastHit2D hit in hits)
            {
                GameObject dropArea = hit.collider.gameObject;
                Debug.Log("IA | GameObject hit par le raycast : " + dropArea.name);

                if (dropArea.GetComponent<Tile>())
                {
                    return dropArea.GetComponent<Tile>();
                }
            }
            return null;
        }

        private int MinMax(int depth, bool maximizingPlayer)
        {
            if (depth == 0 || _gameManager.CheckWin() != 0)
            {
                return _gameManager.EvaluateBoard();
            }

            if (maximizingPlayer)
            {
                int maxEval = int.MinValue;
                foreach (var move in _gameManager.GetLegalMoves(1))
                {
                    _gameManager.ApplyMove(move, 1);
                    int eval = MinMax(depth - 1, false);
                    maxEval = Math.Max(maxEval, eval);
                    _gameManager.UndoMove(move);  // Assume this method undoes a move
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                foreach (var move in _gameManager.GetLegalMoves(-1))
                {
                    _gameManager.ApplyMove(move, -1);
                    int eval = MinMax(depth - 1, true);
                    minEval = Math.Min(minEval, eval);
                    _gameManager.UndoMove(move);  // Assume this method undoes a move
                }
                return minEval;
            }
        }
    }
}