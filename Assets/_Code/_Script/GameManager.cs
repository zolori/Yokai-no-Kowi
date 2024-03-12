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
        [SerializeField] private GameObject[] board, _pileJ1, _pileJ2;
        [SerializeField] private GameObject Kodama, Tanuki, Koropokkuru, Kitsune;

        public GameObject currSelectedPiece;
        public int nextEmptyTileInPileJ1 = 0, nextEmptyTileInPileJ2 = 0;

        #region Player

        private Player _currPlayer, _player1, _player2;
        private int nbOfPlayer = 0;

        #endregion

        public static GameManager Instance;

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

            var kitsune1 = Instantiate(Kitsune, Vector3.zero, quaternion.identity);
            var koropokurru1 = Instantiate(Koropokkuru, Vector3.zero, quaternion.identity);
            var tanuki1 = Instantiate(Tanuki, Vector3.zero, quaternion.identity);
            var kodama1 = Instantiate(Kodama, Vector3.zero, quaternion.identity);

            var kitsune2 = Instantiate(Kitsune, Vector3.zero, Quaternion.Euler(0, 0, 180));
            var koropokurru2 = Instantiate(Koropokkuru, Vector3.zero, Quaternion.Euler(0, 0, 180));
            var tanuki2 = Instantiate(Tanuki, Vector3.zero, Quaternion.Euler(0, 0, 180));
            var kodama2 = Instantiate(Kodama, Vector3.zero, Quaternion.Euler(0, 0, 180));

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
            // For classic move
            if (CanMove(iMyPiece, iNextTile) && !iMyPiece.bIsFromPile)
            {
                if (iNextTile.piece != null)
                {
                    if (iNextTile.piece.Player.Name != _currPlayer.Name)
                    {
                        Eat(iNextTile.piece);
                    }
                }

                iMyPiece.GetComponentInParent<Tile>().piece = null;
                SetPieceAndMoveToParent(iMyPiece, iNextTile);
                FinishTurn();

                iNextTile.piece = iMyPiece;
            }
            // For Air Drop
            else if (iMyPiece.bIsFromPile && iNextTile.piece == null && iMyPiece.Player == _currPlayer)
            {
                AirDrop(iMyPiece);
                SetPieceAndMoveToParent(iMyPiece, iNextTile);
                FinishTurn();
            }
            // Move back the piece to the tile where it belongs
            else
            {
                SetPieceAndMoveToParent(iMyPiece, iMyPiece.GetComponentInParent<Tile>());
            }
            Debug.Log("Current player : "+ _currPlayer.Name);

        }

        /// <summary>
        /// TO ADD THE CURRENT PIECE ON THE TILE INTO THE PLAYER'S PILE
        /// </summary>
        /// <param name="iPiece"></param>
        private void Eat(Piece iPiece)
        {
            iPiece.bIsFromPile = true;
            iPiece.changePlayer(iPiece.Player == _player1 ? _player2 : _player1);
            SetPieceAndMoveToParent(iPiece, ChooseGoodParent(_currPlayer == _player1 ? _pileJ1 : _pileJ2, iPiece));

            if (iPiece.GetComponent<Koropokkuru>())
            {
                // TODO: Call the victory screen
            }
        }

        private Tile ChooseGoodParent(GameObject[] iPile, Piece iPiece)
        {
            Tile t;

            foreach(GameObject tile in iPile)
            {
                t = tile.GetComponentInParent<Tile>();

                if (t.piece == null)
                    return t;
            }

            return null;
        }

        /// <summary>
        /// TO PLACE A PIECE TAKEN FROM THE PLAYER'S PILE
        /// </summary>
        public void AirDrop(Piece iPiece)
        {
            iPiece.GetComponentInParent<Tile>().piece = null;
            iPiece.bIsFromPile = false;
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
            pieceTransform.position = new Vector3(tileTransform.position.x, tileTransform.position.y, 0f); // Move to parent
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
