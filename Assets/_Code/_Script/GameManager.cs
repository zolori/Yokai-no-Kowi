using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace _Code._Script
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Tile[][] _board;
        [SerializeField] private GameObject Kodama, Tanuki, Koropokkuru, kitsune;

        private List<Piece> _pile;
        public GameObject currSelectedPiece;

        public static GameManager Instance;

        #region Player

        private Player _currPlayer, _player1, _player2;
        public static event Action<Tile> playerDroppedPieceCallback;

        #endregion

        /*        public struct Player
                {
                    public string Name;
                    public List<Piece> Pioche;

                    public Player(string iName = "BOT") : this()
                    {
                        Name = iName;
                    }
                }
        */

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            else
                Instance = this;

            DontDestroyOnLoad(gameObject);
        }

        private void InitGame()
        {
            // TODO: Set prefab piece to position

            Instantiate(kitsune, new Vector3(0, 0, -1), quaternion.identity);
            Instantiate(Koropokkuru, new Vector3(1, 0, -1), quaternion.identity);
            Instantiate(Tanuki, new Vector3(2, 0, -1), quaternion.identity);
            Instantiate(Kodama, new Vector3(1, 1, -1), quaternion.identity);

            Instantiate(kitsune, new Vector3(2, 3, -1), Quaternion.Euler(0, 0, 180));
            Instantiate(Koropokkuru, new Vector3(1, 3, -1), Quaternion.Euler(0, 0, 180));
            Instantiate(Tanuki, new Vector3(0, 3, -1), Quaternion.Euler(0, 0, 180));
            Instantiate(Kodama, new Vector3(1, 2, -1), Quaternion.Euler(0, 0, 180));


            _player1 = new Human();
            _player2 = new Human();

            _player1.Name = "Gontrand";
            _player2.Name = "Didier";

            _currPlayer = _player1;
        }

        private void Start()
        {
            InitGame();
        }

        /// <summary>
        /// CHECK IF THE PLAYER CAN MOVE THE GIVEN PIECE ON THE GIVEN TILE
        /// </summary>
        /// <param name="iPiece"></param>
        /// <param name="iTile"></param>
        /// <returns></returns>
        public bool CanMove(Piece iPiece, Tile iTile)
        {
            Vector2 currVectorMovement =
                CalculateVectorDirection(iPiece.GetComponentInParent<Tile>().transform, iTile.transform);

            if (iPiece.transform.rotation.z != 0)
                currVectorMovement *= -1;
            foreach (var movement in iPiece.VectorMovements)
            {
                if (currVectorMovement == movement)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// TO MOVE A PIECE FROM A TILE TO AN OTHER TILE WHILE DOING SOME CHECK
        /// </summary>
        /// <param name="iMyPiece"></param>
        /// <param name="iNextTile"></param>
        public void Move(Piece iMyPiece, Tile iNextTile)
        {
            if (iNextTile.piece != null && CanMove(iMyPiece, iNextTile) && !iMyPiece.bIsFromPile)
            {
                if (iNextTile.piece.Player.Name != _currPlayer.Name)
                {
                    Eat(iNextTile.piece);
                }
                SetPieceAndMoveToParent(iMyPiece, iNextTile);
                playerDroppedPieceCallback?.Invoke(iNextTile);

                iNextTile.piece = iMyPiece;
            }
            else if (iMyPiece.bIsFromPile && iNextTile.piece == null)
            {
                SetPieceAndMoveToParent(iMyPiece, iNextTile);
                playerDroppedPieceCallback?.Invoke(iNextTile);
            }
            else
            {
                SetPieceAndMoveToParent(iMyPiece, iMyPiece.GetComponentInParent<Tile>());
            }

        }

        /// <summary>
        /// TO ADD THE CURRENT PIECE ON THE TILE INTO THE PLAYER'S PILE
        /// </summary>
        /// <param name="iPiece"></param>
        public void Eat(Piece iPiece)
        {

        }

        /// <summary>
        /// TO PLACE A PIECE TAKEN FROM THE PLAYER'S PILE
        /// </summary>
        public void AirDrop()
        {

        }

        /// <summary>
        /// GET THE POSITION OF A GIVEN TILE
        /// </summary>
        /// <param name="iTile"></param>
        /// <returns></returns>
        public Vector2 GetTilePosition(Tile iTile)
        {
            return iTile.position;
        }

        /// <summary>
        /// TO UPDATE THE PLAYER CURRENTLY PLAYING AT THE END OF THE TURN
        /// </summary>
        private void FinishTurn()
        {
            _currPlayer = _currPlayer.Name == _player1.Name ? _player2 : _player1;
        }

        /// <summary>
        /// SET THE PIECE PARENT AND SET THE PIECE POSITION TO THE PARENT POSITION
        /// </summary>
        /// <param name="iMyPiece"></param>
        /// <param name="iNextTile"></param>
        public void SetPieceAndMoveToParent(Piece iMyPiece, Tile iNextTile)
        {
            var pieceTransform = iMyPiece.transform;
            var tileTransform = iNextTile.gameObject.transform;
            pieceTransform.parent = tileTransform; // Set parent
            pieceTransform.position = tileTransform.position; // Move to parent
            iNextTile.piece = iMyPiece; // Put the piece in the tile
        }

        /// <summary>
        /// CLACULATE THE VECTOR DIRECTION OF WHERE THE PLAYER WANT TO MOVE THE PIECE
        /// </summary>
        /// <param name="iPreviousTile"></param>
        /// <param name="iNextTile"></param>
        /// <returns></returns>
        private Vector2 CalculateVectorDirection(Transform iPreviousTile, Transform iNextTile)
        {
            float x = iNextTile.position.x - iPreviousTile.position.x;
            float y = iNextTile.position.y - iPreviousTile.position.y;

            return new Vector2(x, y);
        }
    }
}
