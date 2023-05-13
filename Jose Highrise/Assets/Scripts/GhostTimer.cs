using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Jose.Maps;
using static LevelCreator;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

public class GhostTimer : MonoBehaviour
{
    private static GhostTimer instance;

    private bool b_runTimer = false;
    public bool b_runGhost = true;
    private bool b_ghostExists = false;

    private float timer;

    private ghostTimeClass playerTime = new ghostTimeClass();
    private ghostTimeClass ghostTime;

    private float playerTimer = 10f;
    private float ghostTimer = 0;
    private int ghostIndex = 0;

    public TextMeshProUGUI timerText;

    public Transform player;
    public Transform ghost;

    private float timeSinceLastFrame = 0;

    [System.Serializable]
    public class ghostTimeClass
    {
        public string playerName;
        public bool mapCreator = false;
        public float totalTime;
        public string stringTime;
        public List<float> posListX = new List<float>();
        public List<float> posListY = new List<float>();
        public List<float> posListZ = new List<float>();
        public float refreshRate = 0.2f;
    }

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        Load(StaticData.currentMap.levelName);
    }

    // Update is called once per frame
    void Update()
    {
        if (b_runTimer)
        {
            timeSinceLastFrame = Time.deltaTime;
            timer += timeSinceLastFrame;

            if (b_runGhost)
                GhostHandler();

            timerText.text = ConvertFloatToString(timer);
        }
    }

    string ConvertFloatToString(float time)
    {
        time = Mathf.FloorToInt(timer * 100);

        int minutes = Mathf.FloorToInt(time/6000);
        int seconds = Mathf.FloorToInt((time-(minutes*6000)) / 100);
        int milliseconds = Mathf.FloorToInt(time - (minutes * 6000) - (seconds * 100));

        string timeString = "";
        if (minutes < 10)
            timeString += "0";
        timeString += minutes + ":";
        if (seconds < 10)
            timeString += "0";
        timeString+= seconds+ ":";
        if (milliseconds < 10)
            timeString += "0";
        timeString += milliseconds;

        return timeString;
    }

    void GhostHandler()
    {
        playerTimer += timeSinceLastFrame;
        if (playerTimer >= playerTime.refreshRate)
        {
            playerTimer = 0;
            playerTime.posListX.Add(player.position.x);
            playerTime.posListY.Add(player.position.y);
            playerTime.posListZ.Add(player.position.z);
        }

        if (b_ghostExists)
        {
            ghost.gameObject.SetActive(true);
            ghostTimer += timeSinceLastFrame;
            if (ghostTimer >= ghostTime.refreshRate)
            {
                ghostTimer = 0;
                ghostIndex++;
            }
            if (ghostIndex == ghostTime.posListX.Count - 1) ghost.position = new Vector3(ghostTime.posListX[ghostIndex], ghostTime.posListY[ghostIndex], ghostTime.posListZ[ghostIndex]);
            else if (ghostIndex < ghostTime.posListX.Count - 1)
            {
                Vector3 lastPos = new Vector3(ghostTime.posListX[ghostIndex], ghostTime.posListY[ghostIndex], ghostTime.posListZ[ghostIndex]);
                Vector3 nextPos = new Vector3(ghostTime.posListX[ghostIndex + 1], ghostTime.posListY[ghostIndex + 1], ghostTime.posListZ[ghostIndex + 1]);
                ghost.position = Vector3.Lerp(lastPos, nextPos, ghostTimer / ghostTime.refreshRate);
            }
        }
    }



    public static void StartTimer()
    {
        instance.timerStarted();
    }

    private void timerStarted()
    {
        b_runTimer = true;
        timerText.color = new Color(1, 1, 1, 0.75f);
    }

    public static void Finish()
    {
        instance.b_runTimer = false;
        if (instance.playerTimer < instance.ghostTimer || !instance.b_ghostExists) instance.SaveGhost(); 
    }
    public void SaveGhost()
    {
        playerTime.totalTime = playerTimer;
        playerTime.stringTime = ConvertFloatToString(playerTimer);
        playerTime.playerName = "Dastardly Devin";

        Save(StaticData.currentMap.levelName);
    }

    public void Save(string filename)
    {
        string tardir = "";
#if UNITY_EDITOR
        tardir = Application.dataPath + "/Ghosts";
#else
        tardir = Application.persistentDataPath + "/Ghosts";
        
#endif
        if (!Directory.Exists(tardir))
            Directory.CreateDirectory(tardir);

        // Creates a filestream to the desired file path
        FileStream fs = new FileStream(tardir
            + "/" + filename + ".dat.txt", FileMode.Create);
        BinaryFormatter bf = new BinaryFormatter();
        try
        {
            bf.Serialize(fs, playerTime);
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
    }
    public void Load(string filename)
    {
        string tardir = "";
#if UNITY_EDITOR
        tardir = Application.dataPath + "/Ghosts";
#else
        tardir = Application.persistentDataPath + "/Ghosts";
        
#endif
        // Verifies the file path exists and creates one if not
        if (!Directory.Exists(tardir))
            Directory.CreateDirectory(tardir);

        if (File.Exists(tardir + "/" + filename + ".dat.txt"))
        {
            using (Stream stream = File.Open(tardir + "/" + filename + ".dat.txt", FileMode.Open))
            {
                //Debug.Log(Application.persistentDataPath);
                BinaryFormatter bformatter = new BinaryFormatter();
                ghostTime = (ghostTimeClass)bformatter.Deserialize(stream);
            }
            b_ghostExists = true;
        }
        else b_ghostExists = false;
    }
}
