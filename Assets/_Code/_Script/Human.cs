using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Code._Script
{
    public class Human : Player
    {
        public string Name { get; set; }
        public int FirstEmptyTileInPile { get ; set; }

        Tile Player.Play()
        {
            Tile t = null;
            return t;
        }
    }
}