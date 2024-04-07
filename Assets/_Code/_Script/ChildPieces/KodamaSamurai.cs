using UnityEngine;

namespace _Code._Script.ChildPieces
{
    public class KodamaSamurai : Piece
    {
        // Start is called before the first frame update
        void Start()
        {
            VectorMovements = new[]
                {new Vector2(-1, 1), new Vector2(0, 1), new Vector2(1, 1), new Vector2(-1, 0),
                    new Vector2(1, 0), new Vector2(0, -1)};
            Value = 10;
        }


        public KodamaSamurai(IPlayer player, Tile iTileToSpawn, int iId) : base(player, iTileToSpawn, iId)
        {
        }
    }
}
