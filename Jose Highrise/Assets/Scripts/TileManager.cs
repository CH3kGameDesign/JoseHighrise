using Jose.Objects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;

public class TileManager : MonoBehaviour
{
    public int[,] tiles = new int[0,0];
    [HideInInspector]
    public Vector2Int size;

    public GameObject blockPrefab;
    public ObjectList blockList;
    // Start is called before the first frame update
    void Start()
    {
        int [,] temp = new int[5, 5];
        temp[1, 1] = 1;
        temp[1, 0] = 1;
        temp[1, 3] = 1;
        temp[1, 4] = 1;
        temp[2, 1] = 1;
        temp[3, 1] = 1;
        temp[2, 3] = 1;
        temp[2, 4] = 1;
        temp[0, 1] = 1;
        temp[0, 2] = 1;
        temp[0, 3] = 1;
        temp[0, 4] = 1;
        temp[4, 1] = 1;
        temp[4, 2] = 1;
        temp[4, 3] = 1;
        temp[4, 4] = 1;
        UpdateMap(temp, new Vector2Int(5, 5));
    }

    public void UpdateMap(int[,] temp, Vector2Int sizeTemp)
    {
        tiles = temp;
        size = sizeTemp;
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
        for (int x = 0; x < size.x; x++)
        {
            GameObject GO1 = new GameObject();
            GO1.transform.parent = GO.transform;
            GO1.name = "Column " + x;
            GO1.transform.localPosition = new Vector3(x, 0, 0);
            for (int y = 0; y < size.y; y++)
            {
                GameObject GO2 = new GameObject();
                GO2.transform.parent = GO1.transform;
                GO2.name = "Cell ["+x+","+y+"]";
                GO2.transform.localPosition = new Vector3(0, y, 0);
                if (tiles[x,y] > 0)
                {
                    GameObject GOBlock = Instantiate(blockPrefab, GO2.transform);
                    Vector2Int pos = new Vector2Int(x, y);
                    int sprite = 0;
                    switch (blockList.blocks[tiles[x, y] - 1].blockType)
                    {
                        case ObjectList.blockTypeEnum.single:
                            sprite = 0;
                            break;
                        case ObjectList.blockTypeEnum.joined:
                            sprite = GetTileNum(pos, GetNeighbours(pos));
                            break;
                        case ObjectList.blockTypeEnum.random:
                            sprite = Random.Range(0, blockList.blocks[tiles[x, y] - 1].sprites.Count);
                            break;
                        default:
                            break;
                    }
                    GOBlock.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = blockList.blocks[tiles[x, y] - 1].sprites[sprite];
                }
            }
        }
    }
    int[] GetNeighbours(Vector2Int coord)
    {
        int[] neighbour = new int[8];
        int key = tiles[coord.x, coord.y];

        if (coord.x == 0)
        { neighbour[0] = -1; neighbour[3] = -1; neighbour[5] = -1; }
        if (coord.y == 0)
        { neighbour[5] = -1; neighbour[6] = -1; neighbour[7] = -1; }
        if (coord.x >= size.x - 1)
        { neighbour[2] = -1; neighbour[4] = -1; neighbour[7] = -1; }
        if (coord.y >= size.y - 1)
        { neighbour[0] = -1; neighbour[1] = -1; neighbour[2] = -1; }

        if (neighbour[0] != -1)
            if (tiles[coord.x - 1, coord.y + 1] == key) neighbour[0] = 1;
        if (neighbour[1] != -1)
            if (tiles[coord.x, coord.y + 1] == key) neighbour[1] = 1;
        if (neighbour[2] != -1)
            if (tiles[coord.x + 1, coord.y + 1] == key) neighbour[2] = 1;
        if (neighbour[3] != -1)
            if (tiles[coord.x - 1, coord.y] == key) neighbour[3] = 1;
        if (neighbour[4] != -1)
            if (tiles[coord.x + 1, coord.y] == key) neighbour[4] = 1;
        if (neighbour[5] != -1)
            if (tiles[coord.x - 1, coord.y - 1] == key) neighbour[5] = 1;
        if (neighbour[6] != -1)
            if (tiles[coord.x, coord.y - 1] == key) neighbour[6] = 1;
        if (neighbour[7] != -1)
            if (tiles[coord.x + 1, coord.y - 1] == key) neighbour[7] = 1;

        return neighbour;
    }

    public static int GetTileNum(Vector2Int coord, int[] neighbour)
    {
        int temp = 0;
        

        

        if (neighbour[1] == 1)
        {
            if (neighbour[4] == 1)
            {
                if(neighbour[3] == 1)
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
                if (neighbour[3] == 1)
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
            if (neighbour[4] == 1)
            {
                if (neighbour[3] == 1)
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
                if (neighbour[3] == 1)
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
}
