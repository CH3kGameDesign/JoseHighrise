using Jose.Maps;
using Jose.Objects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public class TileManager : MonoBehaviour
{
    public Map curMap;

    public GameObject blockPrefab;
    public ObjectList blockList;

    public GameObject testingButton;
    public Transform player;
    // Start is called before the first frame update
    void Awake()
    {
        if (StaticData.currentMap != null)
        {
            curMap = CopyMap(StaticData.currentMap);
            int blockID = GetBlockID("Start", blockList);
            Vector2Int tempPos = CheckForBlockPos(blockID, curMap);
            StaticData.respawnPos = new Vector3(tempPos.x, tempPos.y, 0);
            player.position = new Vector3(tempPos.x, tempPos.y, 0);
            Movement temp = player.GetComponentInParent<Movement>();
            temp.References.T_cam.position = temp.References.RB.position + temp.CameraValues.offset;
        }

        testingButton.SetActive(StaticData.testingMode);

        UpdateMap();
    }

    public static int GetBlockID(string name, ObjectList list)
    {
        for (int i = 0; i < list.blocks.Count; i++)
        {
            if (list.blocks[i].name == name)
                return i;
        }
        return -1;
    }

    public static Vector2Int CheckForBlockPos(int blockNo, Map map)
    {
        for (int x = 0; x < map.width; x++)
        {
            for (int y = 0; y < map.height; y++)
            {
                if (map.level[x, y] == blockNo + 1)
                    return new Vector2Int(x, y);
            }
        }
        return new Vector2Int(-1, -1);
    }

    private Map CopyMap(Map map)
    {
        Map newMap = new Map();
        newMap.height = map.height;
        newMap.width = map.width;
        newMap.level = new int[map.width, map.height];
        newMap.bg = new int[map.width, map.height];
        for (int x = 0; x < map.width; x++)
        {
            for (int y = 0; y < map.height; y++)
            {
                newMap.level[x,y] = map.level[x,y];
                newMap.bg[x, y] = map.bg[x, y];
            }
        }
        newMap.levelName = map.levelName;
        return newMap;
    }

    public void UpdateMap(Map newMap)
    {
        curMap = newMap;
        StaticData.currentMap = CopyMap(newMap);
        DeleteMap();
        RenderMap();
    }

    public void UpdateMap()
    {
        DeleteMap();
        RenderMap();
    }

    void DeleteMap()
    {
        if (transform.childCount > 0)
            GameObject.Destroy(transform.GetChild(0).gameObject);
    }
    void RenderMap()
    {
        GameObject GO = new GameObject();
        GO.transform.parent = transform;
        GO.name = "TileMap";
        for (int x = 0; x < curMap.width; x++)
        {
            GameObject GO1 = new GameObject();
            GO1.transform.parent = GO.transform;
            GO1.name = "Column " + x;
            GO1.transform.localPosition = new Vector3(x, 0, 0);
            for (int y = 0; y < curMap.height; y++)
            {
                GameObject GO2 = new GameObject();
                GO2.transform.parent = GO1.transform;
                GO2.name = "Cell ["+x+","+y+"]";
                GO2.transform.localPosition = new Vector3(0, y, 0);
                if (curMap.level[x,y] > 0)
                {
                    GameObject GOBlock = Instantiate(blockPrefab, GO2.transform);
                    Vector2Int pos = new Vector2Int(x, y);
                    int sprite = 0;
                    ObjectList.spriteClass temp = blockList.blocks[curMap.level[x, y] - 1];
                    switch (temp.blockType)
                    {
                        case ObjectList.blockTypeEnum.single:
                            sprite = 0;
                            break;
                        case ObjectList.blockTypeEnum.joined:
                            if (temp.sprites.Count > 30)
                                sprite = GetTileNum_New(pos, GetNeighbours(pos));
                            else
                                sprite = GetTileNum(pos, GetNeighbours(pos));
                            break;
                        case ObjectList.blockTypeEnum.random:
                            sprite = Random.Range(0, temp.sprites.Count);
                            break;
                        case ObjectList.blockTypeEnum.hidden:
                            sprite = 0;
                            GOBlock.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
                            break;
                        default:
                            break;
                    }
                    GOBlock.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = temp.sprites[sprite];
                    GOBlock.GetComponent<Block>().UpdateBlockInfo(new Vector2Int(x, y), temp);
                    SetCollider(GOBlock.GetComponent<BoxCollider>(), temp);
                }
            }
        }
    }
    void SetCollider (BoxCollider col, ObjectList.spriteClass block)
    {
        switch (block.colliderType)
        {
            case ObjectList.colliderTypeEnum.none:
                col.enabled = false;
                break;
            case ObjectList.colliderTypeEnum.fullBlock:
                break;
            case ObjectList.colliderTypeEnum.leftHalf:
                col.center = new Vector3(-0.25f, 0, 0);
                col.size = new Vector3(0.5f, 1, 1);
                break;
            case ObjectList.colliderTypeEnum.rightHalf:
                col.center = new Vector3(0.25f, 0, 0);
                col.size = new Vector3(0.5f, 1, 1);
                break;
            case ObjectList.colliderTypeEnum.upHalf:
                col.center = new Vector3(0,0.25f, 0);
                col.size = new Vector3(1, 0.5f, 1);
                break;
            case ObjectList.colliderTypeEnum.downHalf:
                col.center = new Vector3(0,-0.25f, 0);
                col.size = new Vector3(1, 0.5f, 1);
                break;
            default:
                break;
        }
        col.isTrigger = block.colliderTrigger;

    }

    int[] GetNeighbours(Vector2Int coord)
    {
        int[] neighbour = new int[8];
        int key = curMap.level[coord.x, coord.y];

        if (coord.x == 0)
        { neighbour[0] = -1; neighbour[3] = -1; neighbour[5] = -1; }
        if (coord.y == 0)
        { neighbour[5] = -1; neighbour[6] = -1; neighbour[7] = -1; }
        if (coord.x >= curMap.width - 1)
        { neighbour[2] = -1; neighbour[4] = -1; neighbour[7] = -1; }
        if (coord.y >= curMap.height - 1)
        { neighbour[0] = -1; neighbour[1] = -1; neighbour[2] = -1; }

        if (neighbour[0] != -1)
            if (curMap.level[coord.x - 1, coord.y + 1] == key) neighbour[0] = 1;
        if (neighbour[1] != -1)
            if (curMap.level[coord.x, coord.y + 1] == key) neighbour[1] = 1;
        if (neighbour[2] != -1)
            if (curMap.level[coord.x + 1, coord.y + 1] == key) neighbour[2] = 1;
        if (neighbour[3] != -1)
            if (curMap.level[coord.x - 1, coord.y] == key) neighbour[3] = 1;
        if (neighbour[4] != -1)
            if (curMap.level[coord.x + 1, coord.y] == key) neighbour[4] = 1;
        if (neighbour[5] != -1)
            if (curMap.level[coord.x - 1, coord.y - 1] == key) neighbour[5] = 1;
        if (neighbour[6] != -1)
            if (curMap.level[coord.x, coord.y - 1] == key) neighbour[6] = 1;
        if (neighbour[7] != -1)
            if (curMap.level[coord.x + 1, coord.y - 1] == key) neighbour[7] = 1;

        return neighbour;
    }
    public static int GetTileNum_New(Vector2Int coord, int[] neighbour)
    {
        int temp = 0;
        int neighbourNum = 0;
        int cornerNum = 0;

        if (neighbour[1] == 1)
            neighbourNum += 1;
        if (neighbour[3] == 1)
            neighbourNum += 2;
        if (neighbour[4] == 1)
            neighbourNum += 4;
        if (neighbour[6] == 1)
            neighbourNum += 8;

        if (neighbour[0] == 1)
            cornerNum += 1;
        if (neighbour[2] == 1)
            cornerNum += 2;
        if (neighbour[5] == 1)
            cornerNum += 4;
        if (neighbour[7] == 1)
            cornerNum += 8;
        switch (neighbourNum)
        {
            case 12: temp = 0; if (neighbour[7] != 1) temp = 32; break;
            case 14: temp = 1; if (neighbour[5] != 1) { if (neighbour[7] != 1) temp = 29; else temp = 41;} else if (neighbour[7] != 1) temp = 40; break;
            case 10: temp = 2; if (neighbour[5] != 1) temp = 33; break;
            case 0: temp = 3; break;
            case 13: temp = 6; if (neighbour[2] != 1) { if (neighbour[7] != 1) temp = 28; else temp = 42; } else if (neighbour[7] != 1) temp = 44; break;
            case 15: temp = 7;
                switch (cornerNum)
                {
                    case 0: temp = 21; break;
                    case 1: temp = 30; break;
                    case 2: temp = 31; break;
                    case 3: temp = 18; break;
                    case 4: temp = 36; break;
                    case 5: temp = 25; break;
                    case 6: temp = 5; break;
                    case 7: temp = 10; break;
                    case 8: temp = 37; break;
                    case 9: temp = 4; break;
                    case 10: temp = 26; break;
                    case 11: temp = 11; break;
                    case 12: temp = 24; break;
                    case 13: temp = 16; break;
                    case 14: temp = 17; break;
                }
                break;
            case 11: temp = 8; if (neighbour[0] != 1) { if (neighbour[5] != 1) temp = 35; else temp = 43; } else if (neighbour[5] != 1) temp = 45; break;
            case 8: temp = 9; break;
            case 5: temp = 12; if (neighbour[2] != 1) temp = 38; break;
            case 7: temp = 13; if (neighbour[0] != 1) { if (neighbour[2] != 1) temp = 34; else temp = 47; } else if (neighbour[2] != 1) temp = 46; break;
            case 3: temp = 14; if (neighbour[0] != 1) temp = 39; break;
            case 9: temp = 15; break;
            case 4: temp = 19; break;
            case 6: temp = 20; break;
            case 2: temp = 22; break;
            case 1: temp = 27; break;

            default: temp = 7; break;
        }
        return temp;
    }
    public static int GetTileNum(Vector2Int coord, int[] neighbour)
    {
        int temp = 0;

        if (neighbour[1] == 1)
        {
            if (neighbour[3] == 1)
            {
                if(neighbour[4] == 1)
                {
                    if (neighbour[6] == 1)
                    {
                        temp = 7;
                    }
                    else
                    {
                        temp = 13;
                    }
                }
                else
                {
                    if (neighbour[6] == 1)
                    {
                        temp = 8;
                    }
                    else
                    {
                        temp = 14;
                    }
                }
            }
            else
            {
                if (neighbour[4] == 1)
                {
                    if (neighbour[6] == 1)
                    {
                        temp = 6;
                    }
                    else
                    {
                        temp = 12;
                    }
                }
                else
                {
                    if (neighbour[6] == 1)
                    {
                        temp = 15;
                    }
                    else
                    {
                        temp = 27;
                    }
                }
            }
        }
        else
        {
            if (neighbour[3] == 1)
            {
                if (neighbour[4] == 1)
                {
                    if (neighbour[6] == 1)
                    {
                        temp = 1;
                    }
                    else
                    {
                        temp = 20;
                    }
                }
                else
                {
                    if (neighbour[6] == 1)
                    {
                        temp = 2;
                    }
                    else
                    {
                        temp = 22;
                    }
                }
            }
            else
            {
                if (neighbour[4] == 1)
                {
                    if (neighbour[6] == 1)
                    {
                        temp = 0;
                    }
                    else
                    {
                        temp = 19;
                    }
                }
                else
                {
                    if (neighbour[6] == 1)
                    {
                        temp = 9;
                    }
                    else
                    {
                        temp = 3;
                    }
                }
            }
        }

        return temp;
    }

    public void ExitTestingMode()
    {
        StaticData.testingMode = false;
        SceneManager.LoadScene(2);
    }

    public void UpdateSprites(Vector2Int tarPos)
    {
        for (int x = Mathf.Max(tarPos.x - 1,0); x < Mathf.Min(tarPos.x +2,curMap.width); x++)
        {
            for (int y = Mathf.Max(tarPos.y - 1, 0); y < Mathf.Min(tarPos.y + 2, curMap.height); y++)
            {
                if (curMap.level[x, y] > 0)
                {
                    GameObject GOBlock = transform.GetChild(0).GetChild(x).GetChild(y).GetChild(0).gameObject;
                    Vector2Int pos = new Vector2Int(x, y);
                    int sprite = 0;
                    ObjectList.spriteClass temp = blockList.blocks[curMap.level[x, y] - 1];
                    switch (temp.blockType)
                    {
                        case ObjectList.blockTypeEnum.single:
                            GOBlock.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
                            GOBlock.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = temp.sprites[sprite];
                            break;
                        case ObjectList.blockTypeEnum.joined:
                            GOBlock.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
                            if (temp.sprites.Count > 30)
                                sprite = GetTileNum_New(pos, GetNeighbours(pos));
                            else
                                sprite = GetTileNum(pos, GetNeighbours(pos));
                            GOBlock.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = temp.sprites[sprite];
                            break;
                        case ObjectList.blockTypeEnum.random:
                            GOBlock.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
                            if (pos == tarPos)
                                sprite = Random.Range(0, temp.sprites.Count);
                            break;
                        case ObjectList.blockTypeEnum.hidden:
                            GOBlock.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    public Block GetBlock(Vector2Int pos)
    {
        if (pos.x >= 0 && pos.x < curMap.width && pos.y >= 0 && pos.y < curMap.height)
        {
            if (curMap.level[pos.x, pos.y] > 0)
            {
                return transform.GetChild(0).GetChild(pos.x).GetChild(pos.y).GetChild(0).GetComponent<Block>();
            }
        }
        return null;
    }

    public IEnumerator MoveToLocal(MoveClass move)
    {
        while (move.timeSinceStart < move.timer)
        {
            move.timeSinceStart += 0.02f;
            if (move.tar != null)
                move.tar.localPosition = Vector3.Lerp(move.startPos, move.tarPos, move.timeSinceStart / move.timer);
            else
                break;
            yield return new WaitForSecondsRealtime(0.02f);
        }
        if (move.tar != null)
            move.tar.localPosition = move.tarPos;
    }

    public void UnlockAllBlocks()
    {
        for (int x = 0; x < curMap.width; x++)
        {
            for (int y = 0; y < curMap.height; y++)
            {
                if (curMap.level[x, y] > 0)
                {
                    string temp = blockList.blocks[curMap.level[x, y] - 1].switchToBlockOnUnlock;
                    if (temp.Length > 0)
                    {
                        int blockID = GetBlockID(temp, blockList);
                        curMap.level[x, y] = blockID + 1;
                        GameObject.Destroy(transform.GetChild(0).GetChild(x).GetChild(y).GetChild(0).gameObject);
                        GameObject GOBlock = Instantiate(blockPrefab, transform.GetChild(0).GetChild(x).GetChild(y));
                        GOBlock.transform.SetSiblingIndex(0);
                        GOBlock.GetComponent<Block>().UpdateBlockInfo(new Vector2Int(x, y), blockList.blocks[blockID]);
                        SetCollider(GOBlock.GetComponent<BoxCollider>(), blockList.blocks[blockID]);

                        UpdateSprites(new Vector2Int(x, y));
                    }
                }
            }
        }
    }

    public class MoveClass
    {
        public Transform tar;
        public Vector3 tarPos = Vector3.zero;
        public Vector3 startPos = Vector3.zero;
        public float timer = 0.2f;
        public float timeSinceStart = 0;
    }
}
