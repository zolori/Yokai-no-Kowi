using System;
using _Code._Script.Enums;

namespace _Code._Script.Event
{
    public class EventGameOver:EventArgs
    {
        public readonly EGameOverState CurrState;
        public readonly string Winner;

        public EventGameOver(EGameOverState pCurrState, string pWinner = null)
        {
            CurrState = pCurrState;
            Winner = pWinner;
        }
    }
}