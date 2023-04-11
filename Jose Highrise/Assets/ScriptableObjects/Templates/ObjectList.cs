using System.Collections.Generic;
using UnityEngine;

namespace Jose.Objects
{
    [CreateAssetMenu(fileName = "Object List", menuName = "Jose/Object/List", order = 1)]
    public class ObjectList : ScriptableObject
    {
        [System.Serializable]
        public class spriteClass
        {
            public string name;
            public blockTypeEnum blockType;
            public List<Sprite> sprites = new List<Sprite>();
        }
        public List<spriteClass> blocks = new List<spriteClass>();

        public enum blockTypeEnum { single,joined,random,};
    }
}