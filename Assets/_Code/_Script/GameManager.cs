using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Code._Script
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Tile[][] _board;

        private List<Piece> _pile;
        private Player _currPlayer, _player1, _player2;
        public GameObject currSelectedPiece;

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
            // TODO: Set prefab piece to position

            _player1 = new Human();
            _player2 = new Human();
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
            // CAN MOVE HERE ?
            // IF YES : RETURN YES
            return false;
        }

        /// <summary>
        /// TO MOVE A PIECE FROM A TILE TO AN OTHER TILE WHILE DOING SOME CHECK
        /// </summary>
        /// <param name="iPiece"></param>
        /// <param name="iTile"></param>
        public void Move(Piece iPiece, Tile iTile)
        {
            if (iTile.piece != null && CanMove(iPiece, iTile) && !iPiece.bIsFromPile)
            {
                if (iPiece.Player.Name != _currPlayer.Name)
                {
                    Eat(iTile.piece);
                }
                SetPieceAndMoveToParent(iPiece, iTile);
                iTile.piece = iPiece;
            }
            else if (iPiece.bIsFromPile && iTile.piece == null)
            {
                SetPieceAndMoveToParent(iPiece, iTile);
                iTile.piece = iPiece;
            }
            else
            {
                SetPieceAndMoveToParent(iPiece, iPiece.GetComponentInParent<Tile>());
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
        /// <param name="iPiece"></param>
        /// <param name="iTile"></param>
        public void SetPieceAndMoveToParent(Piece iPiece, Tile iTile)
        {
            var pieceTransform = iPiece.transform;
            var tileTransform = iTile.gameObject.transform;
            pieceTransform.parent = tileTransform;              // Set parent
            pieceTransform.position = tileTransform.position;   // Move to parent
        }
    }
}
