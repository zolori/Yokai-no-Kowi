using System;
using System.Collections.Generic;
using _Code._Script.ChildPieces;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Code._Script
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] board;
        [SerializeField] private GameObject Kodama, Tanuki, Koropokkuru, Kitsune;

        private List<Piece> _pile;
        public GameObject currSelectedPiece;

        public static GameManager Instance;

        #region Player

        private Player _currPlayer, _player1, _player2;
        public static event Action<Tile> PlayerDroppedPieceCallback;

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
            _player1 = new Human();
            _player2 = new Human();

            _player1.Name = "Gontrand";
            _player2.Name = "Didier";
            
            _currPlayer = _player1;
            
            var kitsune1 = Instantiate(Kitsune, Vector3.one, quaternion.identity);
            var koropokurru1 = Instantiate(Koropokkuru,Vector3.one, quaternion.identity);
            var tanuki1 = Instantiate(Tanuki, Vector3.one, quaternion.identity);
            var kodama1 = Instantiate(Kodama, Vector3.one, quaternion.identity);

            var kitsune2 = Instantiate(Kitsune, Vector3.one, Quaternion.Euler(0, 0, 180));
            var koropokurru2 = Instantiate(Koropokkuru, Vector3.one, Quaternion.Euler(0, 0, 180));
            var tanuki2 = Instantiate(Tanuki, Vector3.one, Quaternion.Euler(0, 0, 180));
            var kodama2 = Instantiate(Kodama, Vector3.one, Quaternion.Euler(0, 0, 180));

            kitsune1.GetComponent<Piece>().Player = _player1;
            tanuki1.GetComponent<Piece>().Player = _player1;
            koropokurru1.GetComponent<Piece>().Player = _player1;
            kodama1.GetComponent<Piece>().Player = _player1;
            
            kitsune2.GetComponent<Piece>().Player = _player2;
            tanuki2.GetComponent<Piece>().Player = _player2;
            koropokurru2.GetComponent<Piece>().Player = _player2;
            kodama2.GetComponent<Piece>().Player = _player2;
            
            SetPieceAndMoveToParent(kitsune1.GetComponent<Piece>(), board[0].GetComponent<Tile>());
            SetPieceAndMoveToParent(koropokurru1.GetComponent<Piece>(), board[1].GetComponent<Tile>());
            SetPieceAndMoveToParent(tanuki1.GetComponent<Piece>(), board[2].GetComponent<Tile>());
            SetPieceAndMoveToParent(kodama1.GetComponent<Piece>(), board[4].GetComponent<Tile>());
            
            SetPieceAndMoveToParent(kitsune2.GetComponent<Piece>(), board[11].GetComponent<Tile>());
            SetPieceAndMoveToParent(koropokurru2.GetComponent<Piece>(), board[10].GetComponent<Tile>());
            SetPieceAndMoveToParent(tanuki2.GetComponent<Piece>(), board[9].GetComponent<Tile>());
            SetPieceAndMoveToParent(kodama2.GetComponent<Piece>(), board[7].GetComponent<Tile>());

        }

        private void Start()
        {
            InitGame();
        }

        /// <summary>
        /// CHECK IF THE PLAYER CAN MOVE THE GIVEN PIECE ON THE GIVEN TILE
        /// </summary>
        /// <param name="iMyPiece"></param>
        /// <param name="iNextTile"></param>
        /// <returns></returns>
        public bool CanMove(Piece iMyPiece, Tile iNextTile)
        {
            

            if (iMyPiece.Player == _currPlayer)
            {
                if (iNextTile.piece != null)
                {
                    if (iNextTile.piece.Player == _currPlayer)
                        return false;
                }
                
                Vector2 currVectorMovement =
                    CalculateVectorDirection(iMyPiece.GetComponentInParent<Tile>().transform, iNextTile.transform);

                if (iMyPiece.transform.rotation.z != 0)
                    currVectorMovement *= -1;
                foreach (var movement in iMyPiece.VectorMovements)
                {
                    if (currVectorMovement == movement)
                    {
                        Debug.Log(currVectorMovement);
                        return true;
                    }
                }
                Debug.Log(currVectorMovement);
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
            if (CanMove(iMyPiece, iNextTile) && !iMyPiece.bIsFromPile)
            {
                if (iNextTile.piece != null)
                {
                    if (iNextTile.piece.Player.Name != _currPlayer.Name)
                        Eat(iNextTile.piece);
                }

                SetPieceAndMoveToParent(iMyPiece, iNextTile);
                PlayerDroppedPieceCallback?.Invoke(iNextTile);
                FinishTurn();

                iNextTile.piece = iMyPiece;
            }
            else if (iMyPiece.bIsFromPile && iNextTile.piece == null)
            {
                SetPieceAndMoveToParent(iMyPiece, iNextTile);
                PlayerDroppedPieceCallback?.Invoke(iNextTile);
                FinishTurn();
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
        private void Eat(Piece iPiece)
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
