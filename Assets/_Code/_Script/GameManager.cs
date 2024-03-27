using System;
using System.Collections.Generic;
using _Code._Script.ChildPieces;
using _Code._Script.Enums;
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

        public EventHandler<EventGameOver> GameOverEventHandler;

        [SerializeField] private GameObject[] board, _pileJ1, _pileJ2;
        [SerializeField] private GameObject kodama, tanuki, koropokkuru, kitsune, kodamaSamurai;

        [SerializeField] private UIManager uiManagerReference;

        private string _playerName1, _playerName2;

        public GameObject CurrSelectedPiece { get; set; }

        #region Player

        private IPlayer _currPlayer, _inactivePlayer, _player1, _player2;

        public IPlayer Player1 => _player1;

        public IPlayer Player2 => _player2;

        private int _numberOfPlayers;
        private bool[] _bPlayerAi;

        #endregion

        public int GameState { private set; get; }
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
        }

        /// <summary>
        /// INSTANTIATE, SET PIECES, PLACEMENT, ETC... FOR STARTING A NEW GAME
        /// </summary>
        public void InitGame()
        {
            Time.timeScale = 1;
            uiManagerReference.InitUI();
            foreach (var tile in board)
            {
                if(tile.GetComponent<Tile>().Piece)
                    Destroy(tile.GetComponent<Tile>().Piece.gameObject);
            }

            foreach (var tile in _pileJ1)
            {
                if(tile.GetComponent<Tile>().Piece)
                    Destroy(tile.GetComponent<Tile>().Piece.gameObject);
            }
            
            foreach (var tile in _pileJ2)
            {
                if(tile.GetComponent<Tile>().Piece)
                    Destroy(tile.GetComponent<Tile>().Piece.gameObject);
            }
            
            
            _player1 = new Human(0,
                "Player 1",
                new[] { board[9], board[10], board[11] }
                );
            _player2 = new Human(1,
                "Player 2",
                new[] { board[0], board[1], board[2] }
            );

            _currPlayer = Player1;
            _inactivePlayer = Player2;

            uiManagerReference.DisplayPlayerTurnText(Player1.Name);

            var kitsune1 = Instantiate(kitsune, Vector3.zero, quaternion.identity);
            var koropokurru1 = Instantiate(koropokkuru, Vector3.zero, quaternion.identity);
            var tanuki1 = Instantiate(tanuki, Vector3.zero, quaternion.identity);
            var kodama1 = Instantiate(kodama, Vector3.zero, quaternion.identity);

            var kitsune2 = Instantiate(kitsune, Vector3.zero, Quaternion.Euler(0, 0, 180));
            var koropokurru2 = Instantiate(koropokkuru, Vector3.zero, Quaternion.Euler(0, 0, 180));
            var tanuki2 = Instantiate(tanuki, Vector3.zero, Quaternion.Euler(0, 0, 180));
            var kodama2 = Instantiate(kodama, Vector3.zero, Quaternion.Euler(0, 0, 180));

            kitsune1.GetComponent<Piece>().Player = Player1;
            tanuki1.GetComponent<Piece>().Player = Player1;
            koropokurru1.GetComponent<Piece>().Player = Player1;
            kodama1.GetComponent<Piece>().Player = Player1;

            kitsune2.GetComponent<Piece>().Player = Player2;
            tanuki2.GetComponent<Piece>().Player = Player2;
            koropokurru2.GetComponent<Piece>().Player = Player2;
            kodama2.GetComponent<Piece>().Player = Player2;

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
        private void GameOver(EventGameOver pGameOverState)
        {
            GameOverEventHandler?.Invoke(this, pGameOverState);
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

                if (iMyPiece.Player == Player2)
                    currVectorMovement *= -1;
                foreach (var movement in iMyPiece.VectorMovements)
                {
                    if (currVectorMovement == movement)
                    {
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

                if (iMyPiece.Player == Player2)
                    currVectorMovement *= -1;

                OnPieceMovedEventHandler?.Invoke(iMyPiece.Player, new EventPlayerMovement(currVectorMovement));
                OnTilePieceChangeEventHandler?.Invoke(iMyPiece.GetComponentInParent<Tile>(), new EventTilePieceChange(null));
                SetPieceAndMoveToParent(iMyPiece, iNextTile);

                if (iMyPiece.GetComponent<Kodama>())
                    TryTransformKodama(iMyPiece.GetComponent<Kodama>(), iNextTile);
                
                CheckForDraw();
                FinishTurn();
            }
            // For Air Drop
            else if (CanAirDrop(iMyPiece, iNextTile))
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
                GameOver(new EventGameOver(EGameOverState.Victory, _currPlayer.Name));
            }

            else if (iPiece.GetComponent<KodamaSamurai>())
            {
                var kodamaTmp = Instantiate(kodama, Vector3.zero, iPiece.transform.rotation);
                kodamaTmp.transform.Rotate(0, 0, 180);
                kodamaTmp.GetComponent<Piece>().Player = _currPlayer;
                SetPieceAndMoveToParent(kodamaTmp.GetComponent<Piece>(),
                    ChooseGoodParent(_currPlayer == Player1 ? _pileJ1 : _pileJ2));
                kodamaTmp.GetComponent<Piece>().bIsFromPile = true;
                //ChangePieceFromList(kodamaTmp.GetComponent<Piece>(),iPiece.GetComponent<KodamaSamurai>());
                Destroy(iPiece.gameObject);
            }

            else
            {
                iPiece.bIsFromPile = true;
                iPiece.ChangePlayer(iPiece.Player == Player1 ? Player2 : Player1);
                SetPieceAndMoveToParent(iPiece, ChooseGoodParent(_currPlayer == Player1 ? _pileJ1 : _pileJ2));
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

            if (_currPlayer == Player1)
            {
                Player2.PossessedPieces.Add(iPieceToAdd.GetComponent<Piece>());
            }
            else
            {
                Player1.PossessedPieces.Add(iPieceToAdd.GetComponent<Piece>());
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
            if (Player1.BSameThreeLastMove && Player2.BSameThreeLastMove)
                GameOver(new EventGameOver(EGameOverState.Draw));
        }

        /// <summary>
        /// TO UPDATE THE PLAYER CURRENTLY PLAYING AT THE END OF THE TURN
        /// </summary>
        public void FinishTurn()
        {
            _inactivePlayer = _currPlayer;
            _currPlayer = _currPlayer == Player1 ? Player2 : Player1;

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

        #region *** IA ***

        // A faire : Modifier CanMove() + finir GetTileToMove()

        /// <summary>
        /// GET A REFERENCE TO THE TILE WHERE THE IA TRY TO MOVE THE PIECE
        /// </summary>
        /// <param name="iPieceToMove"></param>
        /// <param name="iVectorMovement"></param>
        /// <returns>The reference to the tile hit by the raycast</returns>
        private Vector2? GetTileToMove(Piece iPieceToMove, Vector2 iVectorMovement)
        {
            Vector2 currPieceTile = iPieceToMove.GetComponentInParent<Transform>().position;

            if (iPieceToMove.Player == Player2)
                iVectorMovement *= -1;

            var xPos = currPieceTile.x + iVectorMovement.x;
            var yPos = currPieceTile.y + iVectorMovement.y;
            Vector2 originPos = new Vector2(xPos, yPos);


            // Start a raycast with the current piece + movement vector as origin
            RaycastHit2D[] hits = Physics2D.RaycastAll(originPos, Vector2.zero);

            // Filter the result to get the first object with Tile script component on it, ignore other gameobject
            foreach (RaycastHit2D hit in hits)
            {
                GameObject dropArea = hit.collider.gameObject;
                Debug.Log("IA | GameObject hit par le raycast : " + dropArea.name);

                if (dropArea.GetComponent<Tile>())
                {
                    CanMove(iPieceToMove, dropArea.GetComponent<Tile>());
                    // return dropArea.GetComponent<Vector2>();
                }

            }

            return null;
        }

        #region *** MinMax ***

        /// <summary>
        /// Return a list of legal moves for the player
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public List<Vector2> GetLegalMoves(IPlayer player)
        {
            List<Vector2> moves = new List<Vector2>();

            foreach (Piece piece in player.PossessedPieces)
            {
                if (!piece.bIsFromPile)
                    foreach (Vector2 mouvements in piece.VectorMovements)
                    {
                        Vector2? move = GetTileToMove(piece, mouvements);
                        if (move != null)
                            moves.Add(move.Value);
                    }
            }

            return new List<Vector2>();
        }

        /// <summary>
        /// Apply a move for the player on the board
        /// </summary>
        /// <param name="move"></param>
        /// <param name="player"></param>
        public void ApplyMove(Vector2 move, IPlayer player)
        {
        }

        public int CheckWin()
        {
            return GameState;
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

        #endregion

        public IPlayer getPlayerThatsNotHisTurn()
        {
            return _currPlayer == Player1 ? Player2 : Player1;
        }
    }
}
