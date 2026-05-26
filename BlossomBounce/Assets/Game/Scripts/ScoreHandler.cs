using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreHandler : MonoBehaviour
{
    public static ScoreHandler instance;
    public int score = 0;
    private TextMeshProUGUI _scoreText;

    public int skinCount = 0;
    public int unlockedSkin = 1;

    void Awake()
    {
        _scoreText = GameObject.Find("ScoreText").GetComponent<TextMeshProUGUI>();

        var ballView = GameObject.Find("LevelSpawner").GetComponent<LevelSpawner>().ballView;
        skinCount = ballView.GetCellDataById(PlayerPrefs.GetInt("ball_Id")).skinCount;

        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }      
    }

    void Start()
    {
        AddScore(PlayerPrefs.GetInt("CurrentScore"));
    }

    void Update()
    {
        if (_scoreText == null)
        {
            _scoreText = GameObject.Find("ScoreText").GetComponent<TextMeshProUGUI>();
            _scoreText.text = score.ToString();
            PlayerPrefs.SetInt("CurrentScore", score);
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        if (score > PlayerPrefs.GetInt("Highscore", 0))
        {
            PlayerPrefs.SetInt("Highscore", score);
        }
        _scoreText.text = score.ToString();
    }

    public void ResetScore()
    {
        score = 0;
    }
}
