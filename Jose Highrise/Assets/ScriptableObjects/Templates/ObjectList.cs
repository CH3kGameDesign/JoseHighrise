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
            [Space(5)]
            public blockTypeEnum blockType;
            public colliderTypeEnum colliderType;
            [Space(5)]
            public bool colliderTrigger = false;
            public bool canJumpOn = false;
            public bool deadly = false;
            public bool finishOnTouch = false;
            public bool unlockOnTouch = false;
            public bool gainPointsOnTouch = false;
            [Space (5)]
            public shootInfoClass shootInfo;
            public moveClass moveInfo;
            [Space(5)]
            public string switchToBlockOnUnlock;
            public ColorClass breaksWhenSwappedWith = new ColorClass();
            public grabEnum grabbability;
            [Space(5)]
            public List<Sprite> sprites = new List<Sprite>();
        }
        public List<spriteClass> blocks = new List<spriteClass>();

        [System.Serializable]
        public class shootInfoClass
        {
            public GameObject bullet;
            public float firerate = 1;
            public Vector2 dir;
            public Vector2 spawnOffset;
            public float destroyAfter = 10;
        }

        [System.Serializable]
        public class moveClass
        {
            public float moveSpeed = 3;
            public Vector2 dir;
            public moveTypeEnum moveType;
            public bool faceDir = false;
        }
        public enum moveTypeEnum {doesntMove,breakOnContact, reboundOnContact};

        public enum blockTypeEnum { single,joined,random,hidden};
        public enum colliderTypeEnum { none, fullBlock, leftHalf, rightHalf, upHalf, downHalf };
        public enum grabEnum { none, grab, swap, grabAndSwap};
        [System.Serializable]
        public class ColorClass
        {
            public bool aqua = false;
            public bool blue = false;
            public bool green = false;
            public bool orange = false;
            public bool purple = false;
            public bool red = false;
        }
    }
}