using System;

namespace _Code._Script.Event
{
    public class EventTilePieceChange : EventArgs
    {
        public readonly Piece NewPiece;

        public EventTilePieceChange(Piece newPiece)
        {
            NewPiece = newPiece;
        }
    }
}