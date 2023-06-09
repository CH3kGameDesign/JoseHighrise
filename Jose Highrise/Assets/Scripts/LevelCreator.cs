using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Drawing;
using Jose.Objects;
using UnityEngine.InputSystem;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Jose.Maps;
using System.Linq;
using System.Text;
using Microsoft.Win32.SafeHandles;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LevelCreator : MonoBehaviour
{
    public referenceClass references;
    [HideInInspector]
    public int objectChosen = 1;
    private int lastObjectChosen = 1;
    private int lastBGChosen = 1;
    [HideInInspector]
    public Map curMap = new Map();
    public AnimationCurve showHideAnim;
    public AnimationCurve moveAnim;
    public Sprite emptyCell;
    public enum drawTypeEnum { draw, background, erase};
    public drawTypeEnum drawType = drawTypeEnum.draw;
    [System.Serializable]
    public class referenceClass
    {
        public EventSystem eventSystem;
        [Space(5)]
        public TMP_InputField nameText;
        [Space(5)]
        public TMP_InputField widthText;
        public TMP_InputField heightText;
        [Space(5)]
        public GameObject scrollView;
        public Transform scrollContent;
        public GameObject scrollItemPrefab;
        public GameObject itemPreview;
        public TextMeshProUGUI drawText;
        [Space(5)]
        public GameObject fileScrollView;
        public Transform fileScrollContent;
        public GameObject fileItemPrefab;
        [Space(5)]
        public GameObject blockPrefab;
        public ObjectList blockList;
        [Space(5)]
        public GameObject overwritePopup;
        [Space(5)]
        public GameObject savePopup;
        [Space(5)]
        public Button fileNameHandle;
        public Button sizeHandle;
    }
    public class popupVariableClass
    {
        public GameObject[] popupObjects = new GameObject[0];
        public Image[] popupImages = new Image[0];
        public TextMeshProUGUI[] popupTexts = new TextMeshProUGUI[0];
        public float timeSinceStart = 0;
        public float timer = 1;
        public AnimationCurve timeline = new AnimationCurve();
        public Vector3 tarLocalDestination = Vector3.zero;
        public Vector3 startLocalDestination = Vector3.zero;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (StaticData.currentMap != null)
            curMap = StaticData.currentMap;
        references.nameText.text = curMap.levelName;
        references.widthText.text = curMap.width.ToString();
        references.heightText.text = curMap.height.ToString();
        RenderMap(0,true);
        RenderMap(1, false);
        UpdateItemList();
    }

    void UpdateItemList()
    {
        for (int i = references.scrollContent.childCount-1; i >= 0; i--)
            GameObject.Destroy(references.scrollContent.GetChild(i).gameObject);

        switch (drawType)
        {
            case drawTypeEnum.draw:
                for (int i = 0; i < references.blockList.blocks.Count; i++)
                {
                    Vector2 pos = new Vector2((i - (Mathf.Floor(i / 3) * 3)) * 70 + 40, -Mathf.Floor(i / 3) * 70 - 40);
                    GameObject GO = Instantiate(references.scrollItemPrefab, references.scrollContent);
                    GO.transform.localPosition = pos;
                    GO.GetComponent<Image>().sprite = references.blockList.blocks[i].sprites[0];
                    int num = i;
                    GO.GetComponent<Button>().onClick.AddListener(delegate { ChangeItemType(num); });
                }
                references.scrollContent.GetComponent<RectTransform>().sizeDelta = new Vector2(references.scrollContent.GetComponent<RectTransform>().sizeDelta.x, Mathf.Floor((references.blockList.blocks.Count) / 3) * 70 + 140);
                break;
            case drawTypeEnum.background:
                for (int i = 0; i < references.blockList.bgTiles.Count; i++)
                {
                    Vector2 pos = new Vector2((i - (Mathf.Floor(i / 3) * 3)) * 70 + 40, -Mathf.Floor(i / 3) * 70 - 40);
                    GameObject GO = Instantiate(references.scrollItemPrefab, references.scrollContent);
                    GO.transform.localPosition = pos;
                    GO.GetComponent<Image>().sprite = references.blockList.bgTiles[i].sprites[0];
                    int num = i;
                    GO.GetComponent<Button>().onClick.AddListener(delegate { ChangeItemType(num); });
                }
                references.scrollContent.GetComponent<RectTransform>().sizeDelta = new Vector2(references.scrollContent.GetComponent<RectTransform>().sizeDelta.x, Mathf.Floor((references.blockList.bgTiles.Count) / 3) * 70 + 140);
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.leftButton.IsPressed())
        {
            if (!references.eventSystem.IsPointerOverGameObject())
                TileClick();
        }
    }
    void TileClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y));
        RaycastHit hit;
        if (Physics.Raycast(ray,out hit,100))
        {
            if (hit.collider.tag == "LC_Tile")
            {
                
                string[] name = hit.collider.transform.parent.name.Split(new char[] { '[', ',', ']' });
                Vector2Int location = new Vector2Int(int.Parse(name[1]), int.Parse(name[2]));
                switch (drawType)
                {
                    case drawTypeEnum.background:
                        if (curMap.bg[location.x, location.y] != objectChosen)
                        {
                            curMap.bg[location.x, location.y] = objectChosen;
                            UpdateArea(location, 1);
                        }
                        break;
                    default:
                        if (curMap.level[location.x, location.y] != objectChosen)
                        {
                            curMap.level[location.x, location.y] = objectChosen;
                            UpdateArea(location, 0);
                        }
                        break;
                }
                
            }
        }
    }

    public void UpdateArea(Vector2Int pos, int layerNum)
    {
        for (int x = Mathf.Max(pos.x - 1, 0); x < Mathf.Min(pos.x + 2, curMap.width); x++)
        {
            for (int y = Mathf.Max(pos.y - 1, 0); y < Mathf.Min(pos.y + 2, curMap.height); y++)
            {
                Transform parent = transform.GetChild(layerNum).GetChild(x).GetChild(y);
                DeleteCell(parent);
                RenderCell(new Vector2Int(x,y), parent, layerNum);
            }
        }
    }

    public void DeleteCell(Transform parent)
    {
        if (parent.childCount > 0)
            GameObject.Destroy(parent.GetChild(0).gameObject);
    }

    public void RenderCell(Vector2Int pos, Transform parent, int layerNum)
    {
        GameObject GOBlock = Instantiate(references.blockPrefab, parent);
        ObjectList.spriteClass block;
        bool trueBlock;
        if (layerNum == 0)
            trueBlock = curMap.level[pos.x, pos.y] > 0;
        else
            trueBlock = curMap.bg[pos.x, pos.y] > 0;

        int sprite = 4;
        if (trueBlock)
        {
            if (layerNum == 0)
                block = references.blockList.blocks[curMap.level[pos.x, pos.y] - 1];
            else
                block = references.blockList.bgTiles[curMap.bg[pos.x, pos.y] - 1];
            switch (block.blockType)
            {
                case ObjectList.blockTypeEnum.single:
                    sprite = 0;
                    break;
                case ObjectList.blockTypeEnum.joined:
                    if (block.sprites.Count > 30)
                        sprite = TileManager.GetTileNum_New(pos, GetNeighbours(pos, layerNum));
                    else
                        sprite = TileManager.GetTileNum(pos, GetNeighbours(pos, layerNum));
                    break;
                case ObjectList.blockTypeEnum.random:
                    sprite = Random.Range(0, block.sprites.Count);
                    break;
                case ObjectList.blockTypeEnum.hidden:
                    sprite = 0;
                    break;
                default:
                    break;
            }
            GOBlock.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = block.sprites[sprite];
        }
        else
            GOBlock.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = emptyCell;
    }

    public void MoveArea (int dir)
    {
        switch (dir)
        {
            case 0:
                for (int y = 0; y < curMap.height; y++)
                {
                    for (int x = 0; x < curMap.width; x++)
                    {
                        if (y >= curMap.height - 1)
                        {
                            curMap.level[x, y] = 0;
                        }
                        else
                        {
                            curMap.level[x, y] = curMap.level[x,y+1];
                        }
                    }
                }
                break;
            case 1:
                for (int x = 0; x < curMap.width; x++)
                {
                    for (int y = 0; y < curMap.height; y++)
                    {
                        if (x >= curMap.width - 1)
                        {
                            curMap.level[x, y] = 0;
                        }
                        else
                        {
                            curMap.level[x, y] = curMap.level[x + 1, y];
                        }
                    }
                }
                break;
            case 2:
                for(int y = curMap.height - 1; y >= 0; y--)
                {
                    for (int x = 0; x < curMap.width; x++)
                    {
                        if (y == 0)
                        {
                            curMap.level[x, y] = 0;
                        }
                        else
                        {
                            curMap.level[x, y] = curMap.level[x, y - 1];
                        }
                    }
                }
                break;
            case 3:
                for (int x = curMap.width - 1; x >= 0; x--)
                {
                    for (int y = 0; y < curMap.height; y++)
                    {
                        if (x == 0)
                        {
                            curMap.level[x, y] = 0;
                        }
                        else
                        {
                            curMap.level[x, y] = curMap.level[x - 1, y];
                        }
                    }
                }
                break;
            default:
                break;
        }
        UpdateMap();
    }

    public void ResizeArea ()
    {
        int newWidth;
        int newHeight;
        bool widthAttempt = int.TryParse(references.widthText.text, out newWidth);
        bool heightAttempt = int.TryParse(references.heightText.text, out newHeight);
        if (!widthAttempt || !heightAttempt)
        {
            Debug.LogWarning("Only numbers can be entered in the Width/Height fields");
            return;
        }
        if (newWidth<=0 || newHeight<=0)
        {
            Debug.LogWarning("Only positive numbers can be entered in the Width/Height fields");
            return;
        }
        int[,] newlevel = new int[newWidth, newHeight];
        for (int x = 0; x < newWidth; x++)
        {
            for (int y = 0; y < newHeight; y++)
            {
                if (x >= curMap.width || y >= curMap.height)
                    newlevel[x, y] = 0;
                else
                    newlevel[x, y] = curMap.level[x, y];

            }
        }
        curMap.level = newlevel;
        curMap.width = newWidth;
        curMap.height = newHeight;
        UpdateMap();
    }

    public void UpdateMap(int[,] temp, Vector2Int sizeTemp)
    {
        curMap.level = temp;
        curMap.width = sizeTemp.x;
        curMap.height = sizeTemp.y;
        DeleteMap();
        RenderMap(0, true);
        RenderMap(1, false);
    }

    public void UpdateMap()
    {
        references.nameText.text = curMap.levelName;
        references.widthText.text = curMap.width.ToString();
        references.heightText.text = curMap.height.ToString();
        DeleteMap();
        RenderMap(0, true);
        RenderMap(1, false);
    }

    void DeleteMap()
    {
        for (int i = transform.childCount-1; i >= 0; i++)
        {
            GameObject.Destroy(transform.GetChild(i).gameObject);
        }
    }
    void RenderMap(int layerNum, bool interactive)
    {
        GameObject GO = new GameObject();
        GO.transform.parent = transform;
        GO.transform.localPosition = new Vector3(0, 0, layerNum);
        GO.name = "TileMap " + layerNum;

        int[,] map = new int[0, 0];
        List<ObjectList.spriteClass> tiles = new List<ObjectList.spriteClass>();
        if (layerNum == 0)
        {
            map = curMap.level;
            tiles = references.blockList.blocks;
        }
        if (layerNum == 1)
        {
            map = curMap.bg;
            tiles = references.blockList.bgTiles;
        }

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
                GO2.name = "Cell [" + x + "," + y + "]";
                GO2.transform.localPosition = new Vector3(0, y, 0);
                GameObject GOBlock = Instantiate(references.blockPrefab, GO2.transform);
                Vector2Int pos = new Vector2Int(x, y);
                int sprite = 4;
                if (map[x, y] > 0)
                {
                    switch (tiles[map[x,y]-1].blockType)
                    {
                        case ObjectList.blockTypeEnum.single:
                            sprite = 0;
                            break;
                        case ObjectList.blockTypeEnum.joined:
                            sprite = TileManager.GetTileNum(pos, GetNeighbours(pos, layerNum));
                            break;
                        case ObjectList.blockTypeEnum.random:
                            sprite = Random.Range(0, tiles[map[x, y]-1].sprites.Count);
                            break;
                        case ObjectList.blockTypeEnum.hidden:
                            sprite = 0;
                            break;
                        default:
                            break;
                    }
                    GOBlock.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = tiles[map[x, y] - 1].sprites[sprite];
                }
                else
                    GOBlock.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = emptyCell;

            }
        }
    }
    int[] GetNeighbours(Vector2Int coord, int layerNum)
    {
        int[] neighbour = new int[8];
        int[,] map;
        if (layerNum == 0)
            map = curMap.level;
        else
            map = curMap.bg;
        int key = map[coord.x, coord.y];

        if (coord.x == 0)
        { neighbour[0] = -1; neighbour[3] = -1; neighbour[5] = -1; }
        if (coord.y == 0)
        { neighbour[5] = -1; neighbour[6] = -1; neighbour[7] = -1; }
        if (coord.x >= curMap.width - 1)
        { neighbour[2] = -1; neighbour[4] = -1; neighbour[7] = -1; }
        if (coord.y >= curMap.height - 1)
        { neighbour[0] = -1; neighbour[1] = -1; neighbour[2] = -1; }

        if (neighbour[0] != -1)
            if (map[coord.x - 1, coord.y + 1] == key) neighbour[0] = 1;
        if (neighbour[1] != -1)
            if (map[coord.x, coord.y + 1] == key) neighbour[1] = 1;
        if (neighbour[2] != -1)
            if (map[coord.x + 1, coord.y + 1] == key) neighbour[2] = 1;
        if (neighbour[3] != -1)
            if (map[coord.x - 1, coord.y] == key) neighbour[3] = 1;
        if (neighbour[4] != -1)
            if (map[coord.x + 1, coord.y] == key) neighbour[4] = 1;
        if (neighbour[5] != -1)
            if (map[coord.x - 1, coord.y - 1] == key) neighbour[5] = 1;
        if (neighbour[6] != -1)
            if (map[coord.x, coord.y - 1] == key) neighbour[6] = 1;
        if (neighbour[7] != -1)
            if (map[coord.x + 1, coord.y - 1] == key) neighbour[7] = 1;

        return neighbour;
    }

    void ChangeLayerOpacity(int layerNum, float tarOpacity)
    {
        for (int x = 0; x < curMap.width; x++)
        {
            for (int y = 0; y < curMap.height; y++)
            {
                transform.GetChild(layerNum).GetChild(x).GetChild(y).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().color = new UnityEngine.Color(1,1,1,tarOpacity);
            }
        }
    }

    void ChangeItemType (int num)
    {
        objectChosen = num + 1;
        switch (drawType)
        {
            case drawTypeEnum.draw:
                lastObjectChosen = num + 1;
                references.itemPreview.transform.GetChild(0).GetComponent<Image>().sprite = references.blockList.blocks[num].sprites[0];
                break;
            case drawTypeEnum.background:
                lastBGChosen = num + 1;
                references.itemPreview.transform.GetChild(0).GetComponent<Image>().sprite = references.blockList.bgTiles[num].sprites[0];
                break;
            default:
                break;
        }
        
        SetVisibleItemView(false);
    }

    public void SetVisibleItemView(bool show)
    {
        references.scrollView.SetActive(show);
        references.itemPreview.SetActive(!show);
    }
    public void SetDrawType(bool draw)
    {
        if (draw)
        {
            references.drawText.text = "Draw";
            drawType = drawTypeEnum.draw;
            objectChosen = lastObjectChosen;
            references.itemPreview.transform.GetChild(0).GetComponent<Image>().sprite = references.blockList.blocks[objectChosen].sprites[0];
            ChangeLayerOpacity(0, 1);
        }
        else
        {
            objectChosen = 0;
            drawType = drawTypeEnum.erase;
            references.drawText.text = "Erase";
            ChangeLayerOpacity(0, 1);
        }
        references.itemPreview.SetActive(draw);
        references.scrollView.SetActive(false);
    }
    public void SetDrawType()
    {
        switch (drawType)
        {
            case drawTypeEnum.draw:
                objectChosen = lastBGChosen;
                references.itemPreview.transform.GetChild(0).GetComponent<Image>().sprite = references.blockList.bgTiles[objectChosen - 1].sprites[0];
                drawType = drawTypeEnum.background;
                references.drawText.text = "BG";
                references.itemPreview.SetActive(true);
                ChangeLayerOpacity(0, 0.2f);
                UpdateItemList();
                break;
            case drawTypeEnum.background:
                objectChosen = 0;
                drawType = drawTypeEnum.erase;
                references.drawText.text = "Erase";
                references.itemPreview.SetActive(false);
                ChangeLayerOpacity(0, 1);
                break;
            case drawTypeEnum.erase:
                references.drawText.text = "Draw";
                drawType = drawTypeEnum.draw;
                objectChosen = lastObjectChosen;
                references.itemPreview.transform.GetChild(0).GetComponent<Image>().sprite = references.blockList.blocks[objectChosen - 1].sprites[0];
                references.itemPreview.SetActive(true);
                ChangeLayerOpacity(0, 1);
                UpdateItemList();
                break;
            default:
                break;
        }
        references.scrollView.SetActive(false);
    }
    public string CheckDupicate (string filename)
    {
        // Verifies the file path exists and creates one if not
        string tardir = "";
#if UNITY_EDITOR
        tardir = Application.dataPath + "/Maps/";
#else
        tardir = Application.persistentDataPath + "/Maps";
#endif
        string[] files = Directory.GetFiles(tardir);
        if (files.Contains<string>(tardir+filename + ".dat.txt"))
        {
            int dupeCounter = 1;
            while (true)
            {
                if (files.Contains<string>(tardir+filename + "_" + dupeCounter.ToString() + ".dat.txt"))
                    dupeCounter++;
                else
                {
                    filename += "_" + dupeCounter.ToString();
                    break;
                }
            }
        }
        return filename;
    }

    public string RemoveSpecialCharacters(string temp)
    {
        StringBuilder sb = new StringBuilder();
        foreach (char c in temp)
        {
            if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
    public void Save(int option)
    {
        string fileName = references.nameText.text;
        fileName = RemoveSpecialCharacters(fileName);
        string temp = CheckDupicate(fileName);
        if (fileName == temp || option == 1)
        {
            Save(fileName);
            ShowOverwritePopup(false);
        }
        else
        {
            if (option == 2)
            {
                Save(temp);
                ShowOverwritePopup(false);
            }
            if (option == 0)
            {
                ShowOverwritePopup(true);
                references.overwritePopup.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "A Map named " + fileName + " already exists, what'd wanna do?";
            }
        }
    }
    public void Save(string filename)
    {
        string tardir = "";
#if UNITY_EDITOR
        tardir = Application.dataPath + "/Maps";
#else
        tardir = Application.persistentDataPath + "/Maps";
        
#endif
        if (!Directory.Exists(tardir))
            Directory.CreateDirectory(tardir);
        
        // Creates a filestream to the desired file path
        FileStream fs = new FileStream(tardir
            + "/"+filename+".dat.txt", FileMode.Create);
        BinaryFormatter bf = new BinaryFormatter();
        try
        {
            bf.Serialize(fs, curMap);
        }
        catch (SerializationException e)
        {
            Debug.Log("Failed to serialize: " + e.Message);
            throw;
        }
        finally
        {
            fs.Close();
        }
        popupVariableClass temp = new popupVariableClass();
        temp.timeline = showHideAnim;
        temp.popupObjects = new GameObject[] { references.savePopup};
        temp.popupImages = new Image[] { references.savePopup.GetComponent<Image>()};
        temp.popupTexts = new TextMeshProUGUI[] { references.savePopup.transform.GetChild(0).GetComponent<TextMeshProUGUI>() };
        temp.timer = 0.5f;
        temp.popupTexts[0].text = "Map saved as " + filename + " under " + tardir;
        StartCoroutine("showHidePopup", temp);
    }
    public void Load(string filename)
    {
        string tardir = "";
#if UNITY_EDITOR
        tardir = Application.dataPath + "/Maps";
#else
        tardir = Application.persistentDataPath + "/Maps";
        
#endif
        // Verifies the file path exists and creates one if not
        if (!Directory.Exists(tardir))
            Directory.CreateDirectory(tardir);
        
        if (File.Exists(tardir + "/"+filename+".dat.txt"))
        {
            using (Stream stream = File.Open(tardir + "/"+filename + ".dat.txt", FileMode.Open))
            {
                //Debug.Log(Application.persistentDataPath);
                BinaryFormatter bformatter = new BinaryFormatter();
                curMap = (Map)bformatter.Deserialize(stream);
            }
            UpdateMap();
        }
    }

    public void UpdateFileList()
    {
        string tardir = "";
#if UNITY_EDITOR
        tardir = Application.dataPath + "/Maps";
#else
        tardir = Application.persistentDataPath + "/Maps";
        
#endif
        if (!Directory.Exists(tardir))
            Directory.CreateDirectory(tardir);
        for (int i = references.fileScrollContent.childCount-1; i >= 0; i--)
        {
            GameObject.Destroy(references.fileScrollContent.GetChild(i).gameObject);
        }
        string[] temp = Directory.GetFiles(tardir);
        List<string> fileNames = new List<string>();
        for (int i = 0; i < temp.Length; i++)
        {
            if (temp[i].EndsWith(".dat.txt"))
                fileNames.Add(temp[i]);
        }
        for (int i = 0; i < fileNames.Count; i++)
        {
            GameObject GO = Instantiate(references.fileItemPrefab, references.fileScrollContent);
            GO.transform.localPosition = new Vector3(341.5f,-70 -35 * i, 0);
            string[] fileName = fileNames[i].Split(new char[] { '/' ,'\\','.'});
            string name = fileName[fileName.Length - 3];
            GO.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = name;
           
            GO.GetComponent<Button>().onClick.AddListener(delegate { ShowFileList(false); Load(name); });
        }
        references.fileScrollContent.GetComponent<RectTransform>().sizeDelta = new Vector2(references.fileScrollContent.GetComponent<RectTransform>().sizeDelta.x, fileNames.Count * 35 + 200);
    }

    public void ShowFileList(bool show)
    {
        UpdateFileList();
        references.fileScrollView.SetActive(show);
    }

    public void ShowOverwritePopup(bool show)
    {
        references.overwritePopup.SetActive(show);
    }
    
    public void ChangeMapName ()
    {
        string temp = references.nameText.text;
        curMap.levelName = temp;
    }

    IEnumerator showHidePopup(popupVariableClass popup)
    {
        while (popup.timeSinceStart < popup.timer)
        {
            popup.timeSinceStart += Time.deltaTime;

            foreach (var item in popup.popupObjects)
                item.SetActive(true);
            foreach (Image item in popup.popupImages)
                item.color = new UnityEngine.Color(item.color.r,item.color.g,item.color.b,popup.timeline.Evaluate(popup.timeSinceStart/popup.timer));
            foreach (TextMeshProUGUI item in popup.popupTexts)
                item.color = new UnityEngine.Color(item.color.r, item.color.g, item.color.b, popup.timeline.Evaluate(popup.timeSinceStart / popup.timer));
            
            yield return new WaitForSecondsRealtime(0.02f);
        }
        foreach (var item in popup.popupObjects)
            item.SetActive(false);
    }

    IEnumerator movePopup(popupVariableClass popup)
    {
        while (popup.timeSinceStart < popup.timer)
        {
            popup.timeSinceStart += Time.deltaTime;

            foreach (var item in popup.popupObjects)
                item.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(popup.startLocalDestination,popup.tarLocalDestination,popup.timeline.Evaluate(popup.timeSinceStart/popup.timer));

            yield return new WaitForSecondsRealtime(0.02f);
        }
    }
    public void ShowHideFileName()
    {
        popupVariableClass temp = new popupVariableClass();
        temp.popupObjects = new GameObject[] { references.fileNameHandle.transform.parent.gameObject};
        temp.startLocalDestination = temp.popupObjects[0].GetComponent<RectTransform>().anchoredPosition;
        temp.timeline = moveAnim;
        temp.timer = 0.1f;

        if (references.fileNameHandle.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text == "Hide")
        {
            references.fileNameHandle.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "File";
            temp.tarLocalDestination = new Vector3(-100,temp.startLocalDestination.y,0);
        }
        else
        {
            references.fileNameHandle.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Hide";
            temp.tarLocalDestination = new Vector3(50, temp.startLocalDestination.y, 0);
        }
        StartCoroutine("movePopup", temp);
    }
    public void ShowHideSize()
    {
        popupVariableClass temp = new popupVariableClass();
        temp.popupObjects = new GameObject[] { references.sizeHandle.transform.parent.gameObject };
        temp.startLocalDestination = temp.popupObjects[0].GetComponent<RectTransform>().anchoredPosition;
        temp.timeline = moveAnim;
        temp.timer = 0.1f;

        if (references.sizeHandle.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text == "Hide")
        {
            references.sizeHandle.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Size";
            temp.tarLocalDestination = new Vector3(-100, temp.startLocalDestination.y, 0);
        }
        else
        {
            references.sizeHandle.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Hide";
            temp.tarLocalDestination = new Vector3(50, temp.startLocalDestination.y, 0);
        }
        StartCoroutine("movePopup", temp);
    }


    public void Play()
    {
        int blockID = TileManager.GetBlockID("Start",references.blockList);
        Vector2Int spawnPos = TileManager.CheckForBlockPos(blockID,curMap);
        if (spawnPos.x != -1)
        {
            StaticData.respawnPos = new Vector3(spawnPos.x, spawnPos.y, 0);
            StaticData.currentMap = curMap;
            StaticData.testingMode = true;
            SceneManager.LoadScene(1);
        }
        else
        {
            popupVariableClass temp = new popupVariableClass();
            temp.timeline = showHideAnim;
            temp.popupObjects = new GameObject[] { references.savePopup };
            temp.popupImages = new Image[] { references.savePopup.GetComponent<Image>() };
            temp.popupTexts = new TextMeshProUGUI[] { references.savePopup.transform.GetChild(0).GetComponent<TextMeshProUGUI>() };
            temp.timer = 0.5f;
            temp.popupTexts[0].text = "Missing A Start Block";
            StartCoroutine("showHidePopup", temp);
        }
    }

    #region Camera Controls
    public void moveCamera(int temp)
    {
        Vector3 pos = Camera.main.transform.position;
        switch (temp)
        {
            case 0:
                pos += Vector3.left;
                break;
            case 1:
                pos += Vector3.right;
                break;
            case 2:
                pos += Vector3.up;
                break;
            case 3:
                pos += Vector3.down;
                break;
            default:
                break;
        }
        pos = new Vector3(Mathf.Clamp(pos.x, -1, curMap.width), Mathf.Clamp(pos.y ,- 1, curMap.height), pos.z);
        Camera.main.transform.position = pos;

    }
    public void zoomCamera(float zoom)
    {
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize + zoom, 1, 10);
    }

    public void zoomCameraIn(InputAction.CallbackContext cnt)
    {
        if (cnt.performed)
            zoomCamera(-1);
    }
    public void zoomCameraOut(InputAction.CallbackContext cnt)
    {
        if (cnt.performed)
            zoomCamera(1);
    }

    public void OnHorizontal(InputAction.CallbackContext cnt)
    {
        if (cnt.performed)
        {
            int temp = 0;
            if (cnt.ReadValue<float>() > 0)
                temp = 1;
            moveCamera(temp);
        }
    }
    public void OnVertical(InputAction.CallbackContext cnt)
    {
        if (cnt.performed)
        {
            int temp = 3;
            if (cnt.ReadValue<float>() > 0)
                temp = 2;
            moveCamera(temp);
        }
    }
    #endregion

    public void Quit()
    {
        SceneManager.LoadScene(0);

    }

   
}
