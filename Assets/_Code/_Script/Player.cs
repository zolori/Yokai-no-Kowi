using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace _Code._Script
{
    public interface Player
    {
        string Name { get; set; }

        public Tile Play();
    }
}

