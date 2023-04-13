using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Jose.Objects
{
    [CreateAssetMenu(fileName = "New PlayList", menuName = "Jose/Map/Playlist", order = 1)]
    public class Playlist : ScriptableObject
    {
        public string playlistName;
        public List<TextAsset> maps = new List<TextAsset>();
    }
}