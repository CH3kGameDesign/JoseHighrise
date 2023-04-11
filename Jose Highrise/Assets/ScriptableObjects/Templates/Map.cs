using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Jose.Maps
{
    [System.Serializable]
    public class Map
    {
        
        public string levelName = "New Map";
        public int[,] level = new int[10,8];
        [HideInInspector]
        public int width = 10;
        public int height = 8;
    }
}