using System;
using System.Collections.Generic;
using _Code._Script.ChildPieces;
using _Code._Script.Event;
using _Code._Script.UI;
using Unity.Mathematics;
using UnityEngine;

namespace _Code._Script
{
    public class GameManager : MonoBehaviour
    {
        // This event will be invoked when the tile's piece variable must be updated
        public EventHandler<EventTilePieceChange> OnTilePieceChangeEventHandler;

        // This event will be invoked to record the last player movement
        public EventHandler<EventPlayerMovement> OnPieceMovedEventHandler;

        [SerializeField] private GameObject[] board, _pileJ1, _pileJ2;
        [SerializeField] private GameObject kodama, tanuki, koropokkuru, kitsune, kodamaSamurai;

        [SerializeField] private UIManager uiManagerReference;


        public GameObject CurrSelectedPiece { get; set; }

        #region Player

        private IPlayer _currPlayer, _player1, _player2;

        public IPlayer Player1 => _player1;

        public IPlayer Player2 => _player2;

        private int _numberOfPlayers;
        private bool[] _bPlayerAi;

        #endregion

        public bool _isGameOver { private set; get; }
        public int gameState { private set; get; }
        public static GameManager Instance;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitGame();
            /*_bPlayerAi = new bool[_numberOfPlayers];
            _bPlayerAi[0] = false;
            _bPlayerAi[1] = true;*/
        }

        /// <summary>
        /// INSTANTIATE, SET PIECES, PLACEMENT, ETC... FOR STARTING A NEW GAME
        /// </summary>
        private void InitGame()
        {
            _player1 = new Human(0,
                "Player 1",
                new[] { board[9], board[10], board[11] }
                );
            _player2 = new Human(1,
                "Player 2",
                new[] { board[0], board[1], board[2] }
            );

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
            /*
                _player1.PossessedPieces.Add(kitsune1.GetComponent<Piece>());
                _player1.PossessedPieces.Add(tanuki1.GetComponent<Piece>());
                _player1.PossessedPieces.Add(koropokurru1.GetComponent<Piece>());
                _player1.PossessedPieces.Add(kodama1.GetComponent<Piece>());

                _player2.PossessedPieces.Add(kitsune2.GetComponent<Piece>());
                _player2.PossessedPieces.Add(tanuki2.GetComponent<Piece>());
                _player2.PossessedPieces.Add(koropokurru2.GetComponent<Piece>());
                _player2.PossessedPieces.Add(kodama2.GetComponent<Piece>());
            */
        }

        /// <summary>
        /// return if this game is over or not
        /// </summary>
        /// <param name="flag"></param>
        public void GameOver(bool value)
        {
            _isGameOver = value;
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
                if (iNextTile.Piece != null)
                {
                    if (iNextTile.Piece.Player == _currPlayer)
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
            }
            return false;
        }

        /// <summary>
        /// CHECK IF THE PLAYER CAN AIR DROP THE GIVEN PIECE ON THE BOARD ON THE GIVEN TILE
        /// </summary>
        /// <param name="iMyPiece"></param>
        /// <param name="iNextTile"></param>
        /// <returns></returns>
        public bool CanAirDrop(Piece iMyPiece, Tile iNextTile)
        {
            if (iMyPiece.Player == _currPlayer)
                return iMyPiece.bIsFromPile && iNextTile.Piece == null;
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
                if (iNextTile.Piece != null)
                {
                    if (iNextTile.Piece.Player.Name != _currPlayer.Name)
                    {
                        Eat(iNextTile.Piece);
                    }
                }

                Vector2 currVectorMovement =
                    CalculateVectorDirection(iMyPiece.GetComponentInParent<Tile>().transform, iNextTile.transform);

                if (iMyPiece.Player == _player2)
                    currVectorMovement *= -1;

                OnPieceMovedEventHandler?.Invoke(iMyPiece.Player, new EventPlayerMovement(currVectorMovement));
                OnTilePieceChangeEventHandler?.Invoke(iMyPiece.GetComponentInParent<Tile>(), new EventTilePieceChange(null));
                SetPieceAndMoveToParent(iMyPiece, iNextTile);

                if (iMyPiece.GetComponent<Kodama>())
                {
                    TryTransformKodama(iMyPiece.GetComponent<Kodama>(), iNextTile);
                }

                FinishTurn();
            }
            // For Air Drop
            else if (CanAirDrop(iMyPiece, iNextTile)) // need to check if iMyPiece.Player == _currPlayer is useless
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
                //ChangePieceFromList(kodamaTmp.GetComponent<Piece>(),iPiece.GetComponent<KodamaSamurai>());
                Destroy(iPiece.gameObject);
            }

            else
            {
                iPiece.bIsFromPile = true;
                iPiece.ChangePlayer(iPiece.Player == _player1 ? _player2 : _player1);
                SetPieceAndMoveToParent(iPiece, ChooseGoodParent(_currPlayer == _player1 ? _pileJ1 : _pileJ2));
                //ChangePieceFromList(iPiece);
            }

        }

        /// <summary>
        /// Find the right player's pile tile to use to place the eaten piece
        /// </summary>
        /// <param name="iPile"></param>
        /// <returns></returns>
        private Tile ChooseGoodParent(GameObject[] iPile)
        {
            Tile t;

            foreach (GameObject tile in iPile)
            {
                t = tile.GetComponentInParent<Tile>();

                if (t.Piece == null)
                    return t;
            }

            return null;
        }

        /*/// <summary>
        /// Remove and add the right piece of the players' list
        /// </summary>
        /// <param name="iPieceToAdd"></param>
        /// <param name="iPieceToRemove"></param>
        private void ChangePieceFromList(Piece iPieceToAdd, Piece iPieceToRemove = null)
        {
            if (iPieceToRemove != null && iPieceToRemove.GetComponent<KodamaSamurai>())
                _currPlayer.PossessedPieces.Remove(iPieceToRemove);
            else
                _currPlayer.PossessedPieces.Remove(iPieceToAdd);

            if (_currPlayer == _player1)
            {
                _player2.PossessedPieces.Add(iPieceToAdd.GetComponent<Piece>());
            }
            else
            {
                _player1.PossessedPieces.Add(iPieceToAdd.GetComponent<Piece>());
            }
        }*/

        /// <summary>
        /// TO PLACE A PIECE TAKEN FROM THE PLAYER'S PILE
        /// </summary>
        public void AirDrop(Piece iPiece)
        {
            OnTilePieceChangeEventHandler?.Invoke(iPiece.GetComponentInParent<Tile>(), new EventTilePieceChange(null));
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
                    SetPieceAndMoveToParent(kodamaSamuraiTmp.GetComponent<Piece>(), iTile);
                    Destroy(iKodama.gameObject);
                }
            }
        }

        public void CheckForDraw()
        {
            if (_player1.BSameThreeLastMove && _player2.BSameThreeLastMove)
                GameOver(false);
        }

        /// <summary>
        /// TO UPDATE THE PLAYER CURRENTLY PLAYING AT THE END OF THE TURN
        /// </summary>
        public void FinishTurn()
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
            OnTilePieceChangeEventHandler?.Invoke(iNextTile, new EventTilePieceChange(iMyPiece));     // Put the piece in the tile variable
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


        #region *** MinMax ***

        /// <summary>
        /// Return a list of legal moves for the player
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public List<Vector2> GetLegalMoves(int player)
        {
            return new List<Vector2>();
        }

        /// <summary>
        /// Apply a move for the player on the board
        /// </summary>
        /// <param name="move"></param>
        /// <param name="player"></param>
        public void ApplyMove(Vector2 move, int player)
        {
        }

        public int CheckWin() 
        {
            return gameState;
        }

        /// <summary>
        /// Undo a move for the player on the board
        /// </summary>
        /// <param name="move"></param>
        public void UndoMove(Vector2 move)
        {

        }

        /// <summary>
        /// Evaluate the board from the perspective of player 1
        /// CRUCIAL FOR THE ENTIER ALGORITHM
        /// 
        /// Evaluation :
        /// - Type de pièce (kuro : 10000 + augmente en fonction de son avancée / kodama augmente en fonction de son avancée / kodama samourai : 10 / autre : 5)
        /// - Avancée des pièces vers le camp de l'aversaire
        /// - Nombre de pièces, de case/pièce couverte
        /// - Nombre de pièce en danger/mangeable au prochain coup
        /// 
        /// </summary>
        /// <returns></returns>
        public int EvaluateBoard()
        {
            return 0;
        }

        #endregion
    }
}
