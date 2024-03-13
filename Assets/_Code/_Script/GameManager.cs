using System;
using _Code._Script.ChildPieces;
using _Code._Script.UI;
using Unity.Mathematics;
using UnityEngine;

namespace _Code._Script
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] board, _pileJ1, _pileJ2;
        [SerializeField] private GameObject kodama, tanuki, koropokkuru, kitsune, kodamaSamurai;

        [SerializeField] private UIManager uiManagerReference;

        public GameObject currSelectedPiece;

        #region Player

        private IPlayer _currPlayer, _player1, _player2;

        #endregion

        public static GameManager Instance;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            else
                Instance = this;
        }

        private void Start()
        {
            InitGame();
        }

        private void InitGame()
        {
            _player1 = new Human();
            _player2 = new Human();

            _player1.Name = "Player 1";
            _player2.Name = "Player 2";
            
            _player1.EnemyLastLine = new []
            {
                board[9],
                board[10],
                board[11]
            };
            _player2.EnemyLastLine = new []
            {
                board[0],
                board[1],
                board[2]
            };
            
            _currPlayer = _player1;
            
            uiManagerReference.DisplayPlayerTurnText(_player1.Name);

            var kitsune1 = Instantiate(kitsune, Vector3.zero, quaternion.identity);
            var koropokurru1 = Instantiate(koropokkuru, Vector3.zero, quaternion.identity);
            var tanuki1 = Instantiate(tanuki, Vector3.zero, quaternion.identity);
            var kodama1 = Instantiate(kodama, Vector3.zero, quaternion.identity);

            var kitsune2 = Instantiate(kitsune, Vector3.zero, Quaternion.Euler(0, 0, 180));
            var koropokurru2 = Instantiate(koropokkuru, Vector3.zero, Quaternion.Euler(0, 0, 180));
            var tanuki2 = Instantiate(tanuki, Vector3.zero, Quaternion.Euler(0, 0, 180));
            var kodama2 = Instantiate(kodama, Vector3.zero, Quaternion.Euler(0, 0, 180));

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

                if (iMyPiece.Player == _player2)
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
                iNextTile.piece = iMyPiece;
                
                if (iMyPiece.GetComponent<Kodama>())
                {
                    TryTransformKodama(iMyPiece.GetComponent<Kodama>(), iNextTile);
                }
                FinishTurn();
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
            if (iPiece.GetComponent<Koropokkuru>())
            {
                uiManagerReference.DisplayVictoryScreen(_currPlayer.Name);
            }
            
            else if (iPiece.GetComponent<KodamaSamurai>())
            {
                var kodamaTmp = Instantiate(kodama, Vector3.zero, iPiece.transform.rotation);
                kodamaTmp.transform.Rotate(0, 0, 180);
                kodamaTmp.GetComponent<Piece>().Player = _currPlayer;
                SetPieceAndMoveToParent(kodamaTmp.GetComponent<Piece>(), 
                    ChooseGoodParent(_currPlayer == _player1 ? _pileJ1 : _pileJ2));
                kodamaTmp.GetComponent<Piece>().bIsFromPile = true;
                Destroy(iPiece.gameObject);
            }

            else
            {
                iPiece.bIsFromPile = true;
                iPiece.changePlayer(iPiece.Player == _player1 ? _player2 : _player1);
                SetPieceAndMoveToParent(iPiece, ChooseGoodParent(_currPlayer == _player1 ? _pileJ1 : _pileJ2));
            }


        }

        private Tile ChooseGoodParent(GameObject[] iPile)
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
        private void TryTransformKodama(Kodama iKodama, Tile iTile)
        {
            foreach (GameObject tile in _currPlayer.EnemyLastLine)
            {
                if (iTile.gameObject == tile)
                {
                    var kodamaSamuraiTmp = Instantiate(kodamaSamurai, Vector3.zero, iKodama.transform.rotation);
                    kodamaSamuraiTmp.GetComponent<Piece>().Player = _currPlayer;
                    //iTile.piece = kodamaSamuraiTmp.GetComponent<Piece>();
                    SetPieceAndMoveToParent(kodamaSamuraiTmp.GetComponent<Piece>(), iTile);
                    Destroy(iKodama.gameObject);
                    Debug.Log("piece : "+ iTile.piece);
                }
            }
        }

        /// <summary>
        /// TO UPDATE THE PLAYER CURRENTLY PLAYING AT THE END OF THE TURN
        /// </summary>
        private void FinishTurn()
        {
            _currPlayer = _currPlayer.Name == _player1.Name ? _player2 : _player1;
            uiManagerReference.DisplayPlayerTurnText(_currPlayer.Name);
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
