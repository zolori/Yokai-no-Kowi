using UnityEngine;

namespace _Code._Script.ChildPieces
{
    public class Kodama : Piece
    {
        // Start is called before the first frame update
        void Start()
        {
            VectorMovements = new[]
                {new Vector2(0, 1)};
        }
    }
}
