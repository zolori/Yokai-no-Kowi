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
        [SerializeField] private GameObject[] boardCopy, _pileJ1Copy, _pileJ2Copy;
        [SerializeField] private GameObject kodama, tanuki, koropokkuru, kitsune, kodamaSamurai;

        [SerializeField] private UIManager uiManagerReference;

        public GameObject CurrSelectedPiece { get; set; }

        #region Player

        private IPlayer _currPlayer, _inactivePlayer, _player1, _player2;

        public IPlayer Player1 => _player1;

        public IPlayer Player2 => _player2;

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

            // Simplification de la destruction des pièces existantes
            DestroyExistingPieces(board);
            DestroyExistingPieces(_pileJ1);
            DestroyExistingPieces(_pileJ2);

            _player1 = new Human(0, "Player 1", new[] { board[9], board[10], board[11] });
            _player2 = new Human(1, "Player 2", new[] { board[0], board[1], board[2] });

            _currPlayer = Player1;
            _inactivePlayer = Player2;

            uiManagerReference.DisplayPlayerTurnText(Player1.Name);

            GameObject[] pieces = { kitsune, koropokkuru, tanuki, kodama };

            int[][] positions = {
                new int[] { 0, 1, 2, 4 }, // Positions initiales pièces Player1
                new int[] { 11, 10, 9, 7 } // Positions initiales pièces Player2
            };

            Quaternion[] rotations = { Quaternion.identity, Quaternion.Euler(0, 0, 180) };
            IPlayer[] players = { Player1, Player2 };

            for (int playerIndex = 0; playerIndex < players.Length; playerIndex++)
                for (int pieceIndex = 0; pieceIndex < pieces.Length; pieceIndex++)
                {
                    var pieceInstance = Instantiate(pieces[pieceIndex], Vector3.zero, rotations[playerIndex]);
                    var pieceComponent = pieceInstance.GetComponent<Piece>();
                    pieceComponent.Player = players[playerIndex];

                    int positionIndex = positions[playerIndex][pieceIndex];
                    SetPieceAndMoveToParent(pieceComponent, board[positionIndex].GetComponent<Tile>());
                }
        }

        private void DestroyExistingPieces(GameObject[] tiles)
        {
            foreach (var tile in tiles)
            {
                var tileComponent = tile.GetComponent<Tile>();
                if (tileComponent.Piece)
                {
                    Destroy(tileComponent.Piece.gameObject);
                }
            }
        }

        /// <summary>
        /// return if this game is over or not
        /// </summary>
        private void GameOver(EventGameOver pGameOverState)
        {
            GameOverEventHandler?.Invoke(this, pGameOverState);
        }

        #region Movements region

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
                    if (iNextTile.Piece.Player == _currPlayer) return false;

                Vector2 currVectorMovement = CalculateVectorDirection(iMyPiece.GetComponentInParent<Tile>().transform, iNextTile.transform);
                if (iMyPiece.Player == Player2) currVectorMovement *= -1;

                foreach (var movement in iMyPiece.VectorMovements)
                    if (currVectorMovement == movement) return true;
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
                    if (iNextTile.Piece.Player.Name != _currPlayer.Name)
                        Eat(iNextTile.Piece);

                Vector2 currVectorMovement = CalculateVectorDirection(iMyPiece.GetComponentInParent<Tile>().transform, iNextTile.transform);

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
                SetPieceAndMoveToParent(iMyPiece, iMyPiece.GetComponentInParent<Tile>());
        }

        /// <summary>
        /// TO PLACE A PIECE TAKEN FROM THE PLAYER'S PILE
        /// </summary>
        public void AirDrop(Piece iPiece)
        {
            OnTilePieceChangeEventHandler?.Invoke(iPiece.GetComponentInParent<Tile>(), new EventTilePieceChange(null));
            iPiece.bIsFromPile = false;
        }

        /// <summary>
        /// TO ADD THE CURRENT PIECE ON THE TILE INTO THE PLAYER'S PILE
        /// </summary>
        /// <param name="iPiece"></param>
        private void Eat(Piece iPiece)
        {
            if (iPiece.GetComponent<Koropokkuru>())
                GameOver(new EventGameOver(EGameOverState.Victory, _currPlayer.Name));

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

        #endregion

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

        // A faire : finir applyMove() qui est une simulation
        // Opti : faire en sorte de garder les moves des pieces qui n'ont pas été bougées pour chaque joueur, pour ne pas avoir à recalculer à chaque fois.


        Dictionary<Piece, Vector2> moves;

        /// <summary>
        /// Return the position of the target Tile
        /// </summary>
        /// <param name="iPieceToMove"></param>
        /// <param name="iVectorMovement"></param>
        /// <returns>The reference to the tile hit by the raycast</returns>
        private bool MoveChecker(Piece iPieceToMove, Vector2 iVectorMovement)
        {
            Vector2 currPieceTile = iPieceToMove.GetComponentInParent<Transform>().position;

            if (iPieceToMove.Player == Player2)
                iVectorMovement *= -1;

            Vector2 targetPos = currPieceTile + iVectorMovement;

            RaycastHit2D[] hits = Physics2D.RaycastAll(targetPos, Vector2.zero);

            foreach (RaycastHit2D hit in hits)
            {
                GameObject dropArea = hit.collider.gameObject;

                if (dropArea.GetComponent<Tile>())
                    if (CanMoveIA(iPieceToMove, dropArea.GetComponent<Tile>()))
                        return true;
            }

            return false;
        }

        /// <summary>
        /// GET A REFERENCE TO THE TILE WHERE THE IA TRY TO MOVE THE PIECE
        /// </summary>
        /// <param name="iPieceToMove"></param>
        /// <param name="iVectorMovement"></param>
        /// <returns></returns>
        private Tile GetTileToMove(Piece iPieceToMove, Vector2 iVectorMovement)
        {
            Vector2 currPieceTile = iPieceToMove.GetComponentInParent<Transform>().position;
            if (iPieceToMove.Player == Player2)
                iVectorMovement *= -1;

            Vector2 targetPos = currPieceTile + iVectorMovement;
            RaycastHit2D[] hits = Physics2D.RaycastAll(targetPos, Vector2.zero);

            foreach (RaycastHit2D hit in hits)
            {
                GameObject dropArea = hit.collider.gameObject;

                if (dropArea.GetComponent<Tile>())
                    return dropArea.GetComponent<Tile>();
            }

            return null;
        }

        private bool CanMoveIA(Piece iMyPiece, Tile iNextTile)
        {
            if (iNextTile.Piece != null)
                if (iNextTile.Piece.Player == _currPlayer)
                    return false;

            Vector2 currVectorMovement = CalculateVectorDirection(iMyPiece.GetComponentInParent<Tile>().transform, iNextTile.transform);

            if (iMyPiece.Player == Player2)
                currVectorMovement *= -1;

            foreach (var movement in iMyPiece.VectorMovements)
                if (currVectorMovement == movement)
                    return true;

            return false;
        }

        #region *** MinMax ***

        /// <summary>
        /// Return a list of legal moves for the player
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public Dictionary<Piece, Vector2> GetLegalMoves(IPlayer player)
        {
            moves = new Dictionary<Piece, Vector2>();

            foreach (Piece piece in player.PossessedPieces)
            {
                if (!piece.bIsFromPile)
                    foreach (Vector2 mouvements in piece.VectorMovements)
                        if (CanMoveIA(piece, GetTileToMove(piece, mouvements)))
                            moves.Add(piece, mouvements);
            }

            return moves;
        }

        /// <summary>
        /// Apply a move for the player on the board
        /// </summary>
        /// <param name="move"></param>
        /// <param name="player"></param>
        public void ApplyMove(KeyValuePair<Piece, Vector2> shift, IPlayer player)
        {
            Piece myPiece = shift.Key;
            Vector2 move = shift.Value;
            Tile nextTile = GetTileToMove(myPiece, move);

            // It assumes that the move it legit as it's suppose to have already checked it
            if (!myPiece.bIsFromPile && CanMoveIA(myPiece, nextTile))
            {
                if (nextTile.Piece != null)
                    if (nextTile.Piece.Player != player)
                        Eat(nextTile.Piece);

                Vector2 currVectorMovement = CalculateVectorDirection(myPiece.GetComponentInParent<Tile>().transform, nextTile.transform);
                if (myPiece.Player == Player2)
                    currVectorMovement *= -1;

                OnPieceMovedEventHandler?.Invoke(myPiece.Player, new EventPlayerMovement(currVectorMovement)); // check if it's game is even
                OnTilePieceChangeEventHandler?.Invoke(myPiece.GetComponentInParent<Tile>(), new EventTilePieceChange(null)); // Update Tile piece variable ref
                //SetPieceAndMoveToParent(myPiece, nextTile);
                // FONCTION DE SIMULATION D'IA (C'EST DANS SA TETE)

                if (myPiece.GetComponent<Kodama>())
                    TryTransformKodama(myPiece.GetComponent<Kodama>(), nextTile);
            }
            // For Air Drop
            else if (CanAirDrop(myPiece, nextTile))
            {
                AirDrop(myPiece);
                SetPieceAndMoveToParent(myPiece, nextTile);
                FinishTurn();
            }
            // Move back the piece to the tile where it belongs
            else
                SetPieceAndMoveToParent(myPiece, myPiece.GetComponentInParent<Tile>());
        }

        public int CheckWin()
        {
            return GameState;
        }

        /// <summary>
        /// Undo a move for the player on the board
        /// </summary>
        /// <param name="move"></param>
        public void UndoMove(KeyValuePair<Piece, Vector2> move)
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
