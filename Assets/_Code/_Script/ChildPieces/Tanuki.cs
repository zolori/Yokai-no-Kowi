using UnityEngine;

namespace _Code._Script.ChildPieces
{
    public class Tanuki : Piece
    {
        // Start is called before the first frame update
        void Start()
        {
            VectorMovements = new[]
            {new Vector2(0, 1), new Vector2(-1, 0),
                new Vector2(1, 0), new Vector2(0, -1)};
        }

        public Tanuki(Player player, Tile iTileToSpawn) : base(player, iTileToSpawn)
        {
        }
    }
}
