using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace _Code._Script
{
    public class Human : Player
    {
        public Action playerDroppedPieceCallback;

        public string Name { get; set; }

        Tile Player.Play()
        {
            Tile t = null;
            return t;
        }
    }
}