using UnityEngine;

namespace _Code._Script.ChildPieces
{
    public class Kitsune : Piece
    {
        // Start is called before the first frame update
        void Start()
        {
            VectorMovements = new[]
                {new Vector2(-1, 1), new Vector2(1, 1), new Vector2(-1, -1), new Vector2(1, -1)};
        }
    }
}
