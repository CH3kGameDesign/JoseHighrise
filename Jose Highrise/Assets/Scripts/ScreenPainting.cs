using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ScreenPainting : MonoBehaviour
{
    public int F_brushRadius = 5;
    public MeshRenderer canvasImage;
    public LayerMask canvasImageLayer;
    private Texture2D canvasPixels;
    public TileManager TM;
    public int dotsPerUnit = 5;
    Vector2Int mapSize;
    public float refreshRate = 0.1f;
    private float timer = 0;
    public float maxPixelDist = 2;
    private Vector3Int lastPos = Vector3Int.zero;

    public LevelCreator.referenceClass references;

    // Start is called before the first frame update
    void Start()
    {
        mapSize = new Vector2Int(TM.curMap.width + 2, TM.curMap.height + 2);
        canvasPixels = new Texture2D(mapSize.x * dotsPerUnit, mapSize.y * dotsPerUnit);
        ClearTexture();
        transform.localScale = new Vector3(mapSize.x, mapSize.y, 1);
        canvasImage.material.SetTexture("_MainTex",canvasPixels);
    }

    void ClearTexture()
    {
        for (int x = 0; x < canvasPixels.width; x++)
        {
            for (int y = 0; y < canvasPixels.height; y++)
            {
                canvasPixels.SetPixel(x, y, Color.clear);
            }
        }
        canvasPixels.Apply();
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.leftButton.IsPressed())
        {
            if (!references.eventSystem.IsPointerOverGameObject())
                Draw();
        }
        else
            lastPos = Vector3Int.one * -100;
    }

    private void Draw()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y));
        if (Physics.Raycast(ray,out hit,100,canvasImageLayer))
        {
            Vector3Int hitPoint = Vector3Int.RoundToInt((hit.point -transform.position)* dotsPerUnit);
            if (lastPos.x > -99)
            {
                while (Vector3Int.Distance(hitPoint,lastPos)> maxPixelDist)
                {
                    lastPos = Vector3Int.RoundToInt(Vector3.MoveTowards(lastPos,hitPoint,maxPixelDist));
                    addCircleAtPoint(lastPos);
                }
            }
                addCircleAtPoint(hitPoint);
                lastPos = hitPoint;
            canvasPixels.Apply();
            canvasImage.material.SetTexture("_MainTex", canvasPixels);
        }
        else
        {
            lastPos = Vector3Int.one * -100;
        }
    }
    private void addCircleAtPoint(Vector3Int point)
    {
        for (int x = -F_brushRadius; x < F_brushRadius + 1; x++)
        {
            for (int y = -F_brushRadius; y < F_brushRadius + 1; y++)
            {
                if (new Vector2(x, y).magnitude < F_brushRadius)
                {
                    Vector2Int pixel = new Vector2Int(point.x + x, point.y + y);
                    if (pixel.x >= 0 && pixel.y >= 0 && pixel.x < (mapSize.x * dotsPerUnit) && pixel.y < (mapSize.y * dotsPerUnit))
                    {
                        canvasPixels.SetPixel(point.x + x, point.y + y, Color.black);
                    }
                }
            }
        }
    }
}
