using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        //[SerializeField] private GameObject[] boardCopy, _pileJ1Copy, _pileJ2Copy;
        [SerializeField] private GameObject kodama, tanuki, koropokkuru, kitsune, kodamaSamurai;

        [SerializeField] private int GameMode { get; set; }

        [SerializeField] private UIManager uiManagerReference;

        public GameObject CurrSelectedPiece { get; set; }

        public KeyValuePair<Piece, Vector2> bestMove = new KeyValuePair<Piece, Vector2>();

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
            GameMode = PlayerPrefs.GetInt("GameMode");

            InitGame();
        }

        /// <summary>
        /// INSTANTIATE, SET PIECES, PLACEMENT, ETC... FOR STARTING A NEW GAME
        /// </summary>
        public void InitGame()
        {
            GameState = -2;

            Time.timeScale = 1;
            uiManagerReference.InitUI();

            // Simplification de la destruction des pièces existantes
            DestroyExistingPieces(board);
            DestroyExistingPieces(_pileJ1);
            DestroyExistingPieces(_pileJ2);

            if (GameMode == 1)
            {
                _player1 = new Human(0, "Player 1", new[] { board[9], board[10], board[11] });
                _player2 = new Human(1, "Player 2", new[] { board[0], board[1], board[2] });
            }
            else if (GameMode == 2)
            {
                _player1 = new Human(0, "Player 1", new[] { board[9], board[10], board[11] });
                _player2 = new IA(1, "Player 2", new[] { board[0], board[1], board[2] });
            }

            _currPlayer = Player1;
            _inactivePlayer = Player2;

            uiManagerReference.DisplayPlayerTurnText(Player1.Name);

            GameObject[] pieces = { kitsune, koropokkuru, tanuki, kodama };

            // Positions initiales pièces joueurs
            int[][] positions = {
                new int[] { 0, 1, 2, 4 },
                new int[] { 11, 10, 9, 7 }
            };

            Quaternion[] rotations = { Quaternion.identity, Quaternion.Euler(0, 0, 180) };
            IPlayer[] players = { Player1, Player2 };

            for (int playerIndex = 0; playerIndex < players.Length; playerIndex++)
                for (int pieceIndex = 0; pieceIndex < pieces.Length; pieceIndex++)
                {
                    var pieceInstance = Instantiate(pieces[pieceIndex], Vector3.zero, rotations[playerIndex]);
                    var pieceComponent = pieceInstance.GetComponent<Piece>();
                    pieceComponent.Player = players[playerIndex];
                    players[playerIndex].PossessedPieces.Add(pieceComponent);

                    int positionIndex = positions[playerIndex][pieceIndex];
                    SetPieceAndMoveToParent(pieceComponent, board[positionIndex].GetComponent<Tile>());
                    //SetPieceAndMoveToParent(pieceComponent, boardCopy[positionIndex].GetComponent<Tile>());
                }

            moves = new Dictionary<Piece, List<Vector2>>();
            movesHistory = new List<MoveHistory>();

            if (GameMode == 2)
            {
                Play();
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

        private async Task<bool> Play()
        {            
            float bestMoveValue;

            if (_currPlayer.isPlaying)
                return false;

            _currPlayer.isPlaying = true;

            if (_currPlayer is IA ia)
            {
                bestMoveValue = await ia.MinMax(3, true);
                
                Debug.Log("Best move value :" + bestMoveValue + " , piece : " + bestMove.Key + " , déplacement : " + bestMove.Value);

                Move(bestMove.Key, GetTileToMove(bestMove.Key, bestMove.Value));
            }

            return true;
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

                foreach (GameObject tile in _currPlayer.EnemyLastLine)
                {
                    if (iNextTile.gameObject == tile)
                        if (iMyPiece.GetComponent<Kodama>())
                            TryTransformKodama(iMyPiece.GetComponent<Kodama>(), iNextTile);
                        else if (iMyPiece.GetComponent<Koropokkuru>())
                            GameOver(new EventGameOver(EGameOverState.Victory, _currPlayer.Name));
                }
                CheckForDraw();
                StartCoroutine(FinishTurn());
            }
            // For Air Drop
            else if (CanAirDrop(iMyPiece, iNextTile))
            {
                AirDrop(iMyPiece);
                SetPieceAndMoveToParent(iMyPiece, iNextTile);
                StartCoroutine(FinishTurn());
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
        public void Eat(Piece iPiece)
        {
            if (iPiece.GetComponent<Koropokkuru>())
            {
                iPiece.bIsFromPile = true;
                iPiece.ChangePiecePlayer(SwitchPlayer(iPiece.Player));
                SetPieceAndMoveToParent(iPiece, ChooseGoodParent(_currPlayer == Player1 ? _pileJ1 : _pileJ2));
                GameState = _currPlayer == Player1 ? 1 : -1;
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
                iPiece.ChangePiecePlayer(SwitchPlayer(iPiece.Player));
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
        private Piece TryTransformKodama(Kodama iKodama, Tile iTile)
        {
            var kodamaSamuraiTmp = Instantiate(kodamaSamurai, Vector3.zero, iKodama.transform.rotation);
            kodamaSamuraiTmp.GetComponent<Piece>().Player = _currPlayer;
            SetPieceAndMoveToParent(kodamaSamuraiTmp.GetComponent<Piece>(), iTile);
            Destroy(iKodama.gameObject);

            return kodamaSamuraiTmp.GetComponent<Piece>();
        }

        public void CheckForDraw()
        {
            if (Player1.BSameThreeLastMove && Player2.BSameThreeLastMove)
                GameOver(new EventGameOver(EGameOverState.Draw));
        }

        /// <summary>
        /// TO UPDATE THE PLAYER CURRENTLY PLAYING AT THE END OF THE TURN
        /// </summary>
        public IEnumerator FinishTurn()
        {
            if (GameState == -1 || GameState == 0 || GameState == 1)
                GameOver(new EventGameOver(EGameOverState.Victory, _currPlayer.Name));

            _currPlayer.isPlaying = false;
            _inactivePlayer = _currPlayer;
            _currPlayer = _currPlayer == Player1 ? Player2 : Player1;

            uiManagerReference.DisplayPlayerTurnText(_currPlayer.Name);

            yield return new WaitForSeconds(0.1f);
            Play();
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

        public struct MoveHistory
        {
            public Piece piece { get; set; }
            public Vector2 move { get; set; }
            public bool isFromPile { get; set; }
            public Piece pieceEaten { get; set; }
            public Tile currTile { get; set; }
            public Tile prevTile { get; set; }
            public int indexFromPile { get; set; }
        }

        Dictionary<Piece, List<Vector2>> moves;
        private List<MoveHistory> movesHistory;

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
                if (iNextTile.Piece.Player == iMyPiece.Player)
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
        public Dictionary<Piece, List<Vector2>> GetLegalMoves(IPlayer player)
        {
            moves = new Dictionary<Piece, List<Vector2>>();
            List<Vector2> moveList = new List<Vector2>();

            foreach (Piece piece in player.PossessedPieces)
            {
                if (!piece.bIsFromPile)
                    foreach (Vector2 mouvements in piece.VectorMovements)
                    {
                        Tile targetTile = GetTileToMove(piece, mouvements);
                        if (targetTile != null)
                            if (CanMoveIA(piece, targetTile))
                                moveList.Add(mouvements);
                    }
                moves.Add(piece, moveList);
                moveList = new List<Vector2>();
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
            Vector2 myMove = shift.Value;
            Tile nextTile = GetTileToMove(myPiece, myMove);
            Tile currTile = myPiece.GetComponentInParent<Tile>();

            MoveHistory myMoveHistory = new MoveHistory()
            {
                piece = myPiece, 
                move = myMove, 
                isFromPile = myPiece.bIsFromPile, 
                prevTile = currTile
            };

            if (nextTile == null)
                return;

            if (!myPiece.bIsFromPile && CanMoveIA(myPiece, nextTile))
            {
                if (nextTile.Piece != null)
                    if (nextTile.Piece.Player != player)
                    {
                        myMoveHistory.pieceEaten = nextTile.Piece;
                        Eat(nextTile.Piece);
                    }

                Vector2 currVectorMovement = CalculateVectorDirection(myPiece.GetComponentInParent<Tile>().transform, nextTile.transform);
                if (myPiece.Player == Player2)
                    currVectorMovement *= -1;

                OnPieceMovedEventHandler?.Invoke(myPiece.Player, new EventPlayerMovement(currVectorMovement)); // check if it's game is even
                OnTilePieceChangeEventHandler?.Invoke(myPiece.GetComponentInParent<Tile>(), new EventTilePieceChange(null)); // Update Tile piece variable ref
                SetPieceAndMoveToParent(myPiece, nextTile); // CETTE FONCTION DEVRAIT ETRE UNE FONCTION DE SIMULATION

                foreach (GameObject tile in _currPlayer.EnemyLastLine)
                {
                    if (nextTile.gameObject == tile)
                        if (myPiece.GetComponent<Kodama>())
                            myPiece = TryTransformKodama(myPiece.GetComponent<Kodama>(), nextTile);
                }

                myMoveHistory.currTile = nextTile;

                movesHistory.Add(myMoveHistory);
            }
            else if (CanAirDrop(myPiece, nextTile))
            {
                movesHistory.Add(myMoveHistory);
                AirDrop(myPiece);
                SetPieceAndMoveToParent(myPiece, nextTile);
            }
            else
                SetPieceAndMoveToParent(myPiece, currTile);
        }

        /// <summary>
        /// Undo a move for the player on the board
        /// </summary>
        /// <param name="move"></param>
        public void UndoMove(KeyValuePair<Piece, Vector2> move)
        {
            MoveHistory myMoveHistory = movesHistory.Last();
/*            Debug.Log("=======History Move Info : \n Piece :" + myMoveHistory.piece +
                      "\n Last Move : " + myMoveHistory.move +
                      "\n Previous Tile : " + myMoveHistory.prevTile.transform.position + 
                      "\n Current Tile : " + myMoveHistory.currTile.transform.position);
*/
            if(GameState == 1 || GameState == 0 || GameState == -1)
                GameState = -2;

            if (!myMoveHistory.isFromPile)
            {
                OnTilePieceChangeEventHandler?.Invoke(myMoveHistory.piece.GetComponentInParent<Tile>(), new EventTilePieceChange(null)); // Update Tile piece variable ref
                SetPieceAndMoveToParent(myMoveHistory.piece, myMoveHistory.prevTile);
                if (myMoveHistory.pieceEaten != null)
                {
                    OnTilePieceChangeEventHandler?.Invoke(myMoveHistory.pieceEaten.GetComponentInParent<Tile>(), new EventTilePieceChange(null)); // Update Tile piece variable ref
                    myMoveHistory.pieceEaten.ChangePiecePlayer(SwitchPlayer(myMoveHistory.pieceEaten.Player));
                    SetPieceAndMoveToParent(myMoveHistory.pieceEaten, myMoveHistory.currTile);
                    myMoveHistory.pieceEaten.bIsFromPile = false;
                }
            }
            else
                SetPieceAndMoveToParent(myMoveHistory.piece, myMoveHistory.prevTile);

            movesHistory.Remove(myMoveHistory);
        }

        /// <summary>
        /// return the game state
        /// </summary>
        /// <returns></returns>
        public int CheckWin()
        {
            return GameState;
        }

        /// <summary>
        /// Evaluate the board from the perspective of player 1
        /// CRUCIAL FOR THE ENTIER ALGORITHM
        /// 
        /// Utilisation de la boucle for au lieu de foreach car l'ordre de parcourt du tableau est important, et j'ai peur qu'à grande vitesse le tableau ne soit plus lu dans l'ordre.
        /// 
        /// Evaluation :
        /// V Type de pièce (kuro : 10000 + augmente en fonction de son avancée / kodama augmente en fonction de son avancée / kodama samourai : 10 / autre : 5)
        /// V Avancée des pièces vers le camp de l'aversaire
        /// - Nombre de pièces, de case/pièce couverte, en danger/mangeable au prochain coup 
        /// 
        /// </summary>
        /// <returns></returns>
        public float EvaluateBoard()
        {
            float mark = 0;
            for (int i = 0; i < board.Length; i++)
            {
                Tile tile = board[i].GetComponent<Tile>();
                Piece piece = tile.Piece;

                if (piece == null)
                    continue;

                float pieceValue = piece.Value;

                mark += pieceValue;

                float markAdjustment = piece.Player == _currPlayer ? 1 : -1;
                float positionValue;

                if (piece is KodamaSamurai)
                {
                    positionValue = i switch
                    {
                        0 or 1 or 2 => pieceValue + 9,
                        3 or 4 or 5 => pieceValue + 12,
                        6 or 7 or 8 => pieceValue + 12,
                        9 or 10 or 11 => pieceValue + 9,
                        _ => 0
                    };
                }
                else // Covers Koropokkuru, Kodama, and the orther pieces :DDD
                {
                    bool isSpecialPiece = piece is Koropokkuru || piece is Kodama;
                    positionValue = i switch
                    {
                        0 or 1 or 2 => pieceValue + (_currPlayer == Player1 ? (isSpecialPiece ? 0 : 0) : (isSpecialPiece ? 9 : 6)),
                        3 or 4 or 5 => pieceValue + (_currPlayer == Player1 ? (isSpecialPiece ? 3 : 2) : (isSpecialPiece ? 6 : 4)),
                        6 or 7 or 8 => pieceValue + (_currPlayer == Player1 ? (isSpecialPiece ? 6 : 4) : (isSpecialPiece ? 3 : 2)),
                        9 or 10 or 11 => pieceValue + (_currPlayer == Player1 ? (isSpecialPiece ? 9 : 6) : 0),
                        _ => 0
                    };
                }

                mark += markAdjustment * positionValue;
            }
            return mark;
        }

        #endregion

        #endregion

        /// <summary>
        /// Return the player that is not playing, as the IA can use it to simulate the opposite player's turn
        /// </summary>
        /// <returns></returns>
        public IPlayer getPlayerThatsNotHisTurn()
        {
            return _currPlayer == Player1 ? Player2 : Player1;
        }

        /// <summary>
        /// Return the other player than the one in argument
        /// </summary>
        /// <param name="_player"></param>
        public IPlayer SwitchPlayer(IPlayer pPlayer)
        {
            return pPlayer == Player1 ? Player2 : Player1;
        }
    }
}
