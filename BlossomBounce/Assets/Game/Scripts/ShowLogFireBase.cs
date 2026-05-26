using Nami.Controller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowLogFireBase : MonoBehaviour
{
    private static ShowLogFireBase instance;
    public static ShowLogFireBase Instance => instance;
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }




    float timeStartLevel = 0;
    int numberTries = 0;

    int skinCount = 0;

    private void Start()
    {
        ResetValue();
        numberTries += (PlayerPrefs.GetInt("numbertries") > 0 ? PlayerPrefs.GetInt("numbertries") : 0);
        timeStartLevel -= (PlayerPrefs.GetFloat("timeplaylevel") > 0 ? PlayerPrefs.GetFloat("timeplaylevel") : 0);
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            Debug.Log("pause application");
            PlayerPrefs.SetInt("CurrentScore", ScoreHandler.instance.score);
            PlayerPrefs.SetInt("skin_count", ScoreHandler.instance.skinCount);
            ShowLogPauseQuit();
            SaveDataPlayLevel();
        }
    }
    private void OnApplicationQuit()
    {
        Debug.Log("quit application");
        PlayerPrefs.SetInt("CurrentScore", ScoreHandler.instance.score);
        PlayerPrefs.SetInt("skin_count", ScoreHandler.instance.skinCount);
        SaveDataPlayLevel();
        ShowLogPauseQuit();
    }
    void SaveDataPlayLevel()
    {
        PlayerPrefs.SetFloat("timeplaylevel", GetTimePlay());
        PlayerPrefs.SetInt("numbertries",GetNumberTriesLevel()-1);
    }
    void ShowLogPauseQuit()
    {
        int preID = PlayerPrefs.GetInt("ball_Id");
        var ballView = GameObject.Find("LevelSpawner").GetComponent<LevelSpawner>().ballView.GetCellDataById(preID);
        GameFirebase.SendEvent("pause_game","id_level",PlayerPrefs.GetInt("Level").ToString(), 
            "time_play", Mathf.Round(GetTimePlay()).ToString(), "number_tries", GetNumberTriesLevel().ToString(), 
            "level_point", PlayerPrefs.GetInt("CurrentScore").ToString(), "id_skin", ballView.id.ToString(), 
            "number_uses", ballView.skinCount.ToString());
    }
    void StartTimingLevel()
    {
        timeStartLevel = Time.time;
        
    }
    float GetTimePlay()
    {
        return Time.time - timeStartLevel;
    }
    public void AddNumberTriesLevel()
    {
        numberTries++;
    }
    int GetNumberTriesLevel()
    {
        return numberTries;
    }
    void StartNumberTries()
    {
        numberTries = 1;
    }
    void ResetValue()
    {
        StartTimingLevel();
        StartNumberTries();
    }

    public void ShowCompleteLevel()
    {
        GameFirebase.SendEvent("complete_level", "id_level", PlayerPrefs.GetInt("Level").ToString(), "time_play", Mathf.Round(GetTimePlay()).ToString(), "number_tries", GetNumberTriesLevel().ToString());
        ResetValue();
    }

    public int TimeUsedSkin()
    {
        return skinCount;
    }

    public void PickSkin()
    {
        StartTimingLevel();
    }

    public void ShowStartLevel()
    {
        //GameFirebase.SendEvent("start_level", "id_level", GameCtr.instance.currentLevel.ToString());

    }

}
