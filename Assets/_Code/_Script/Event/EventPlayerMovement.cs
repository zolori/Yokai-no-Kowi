using System;
using UnityEngine;

namespace _Code._Script.Event
{
    public class EventPlayerMovement: EventArgs
    {
        public Vector2 VectorMovement;


        public EventPlayerMovement(Vector2 vectorMovement)
        {
            VectorMovement = vectorMovement;
        }
    }
}