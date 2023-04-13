using Jose.Maps;
using Jose.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{    
    public List<Playlist> playlistList = new List<Playlist>();
    // Start is called before the first frame update
    void Start()
    {
        StaticData.testingMode = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadPlaylist(int playlistNum)
    {
        StaticData.currentPlaylist = playlistList[playlistNum];
        StaticData.levelNum = 0;

        //playlistList[0].maps[0]

        StaticData.currentMap = Deserialize<Map>(playlistList[playlistNum].maps[0].bytes);


        LoadScene(1);
    }


    private T Deserialize<T>(byte[] param)
    {
        using (MemoryStream ms = new MemoryStream(param))
        {
            IFormatter br = new BinaryFormatter();
            return (T)br.Deserialize(ms);
        }
    }

    public void LoadScene(int sceneNum)
    {
        if (sceneNum == -1)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        else
            SceneManager.LoadScene(sceneNum);
    }
}
