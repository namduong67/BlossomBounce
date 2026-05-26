using Nami.Controller;
using ObjectPooler;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSpawner : MonoBehaviour
{
    [SerializeField] private StackList platformStackList;
    [SerializeField] private GameObject[] _selectedEasyPlatforms = new GameObject[10];
    [SerializeField] private GameObject[] _selectedHardPlatforms = new GameObject[11];
    [SerializeField] private GameObject _winPrefab;
    [SerializeField] Transform stackParent;

    [SerializeField] List<GameObject> stackPlatforms;

    [Space(5f)]
    [Header("Item Data")]
    public BallView ballView;
    public BallSelector ballSelector;
    public Image background;

    [Space(5f)]
    public GameObject _normalPlatforms, _winPlatform;
    public int _level = 1, _platformAddition = 7;
    private float i = 0;
    private int j = 0;
    public Material plateMaterial, baseMaterial, particleMaterial, newPlaneMaterial, bgMaterial;
    public Image currentLevelImage, nextLevelImage, progressBarImage, progressFillImage;
    public MeshRenderer playerMesh;

    int number = 0;
    int normalPattern = 0;
    int ezPattern = 0;
    int numbertries;

    public int lastSpawnedPlatformIndex;
    public int totalPatterns;
    public int totalPlatforms;

    void Awake()
    {
        ballSelector.onSelectBall = OnSelectBall;
        SetBallData();
        LoadBallData();
        LevelManagement();
    }

    private void Update()
    {
        BallLockState();
    }

    private void SetBallData()
    {
        if (!PlayerPrefs.HasKey("ball_Id"))
        {
            PlayerPrefs.SetInt("ball_Id", 0);
        }
    }

    private void OnSelectBall(int id)
    {
        Ball cellData = ballView.GetCellDataById(id);
        playerMesh.GetComponent<MeshFilter>().mesh = cellData.mesh;
        playerMesh.material = cellData.material;
        int preID = PlayerPrefs.GetInt("ball_Id");
        if (preID != cellData.id)
        {
            ballView.GetCellDataById(preID).skinCount += ScoreHandler.instance.skinCount;
            GameFirebase.SendEvent("pic_skin", "id_skin", ballView.GetCellDataById(preID).id.ToString(),
                "number_uses", ballView.GetCellDataById(preID).skinCount.ToString());
            ScoreHandler.instance.skinCount = 0;
            ShowLogFireBase.Instance.PickSkin();
        }
        PlayerPrefs.SetInt("ball_Id", id);
    }

    private void BallLockState()
    {
        foreach(Ball ball in ballView.listData)
        {
            if (ball.unlockType == UnlockType.Score)
                if (PlayerPrefs.GetInt("CurrentScore") >= ball.unlockCondition)
                    ball.isLocked = false;
            if (ball.unlockType == UnlockType.Level)
                if (PlayerPrefs.GetInt("Level") >= ball.unlockCondition)
                    ball.isLocked = false;
        }
    }

    private void LoadBallData()
    {
        int id = PlayerPrefs.GetInt("ball_Id");
        playerMesh.GetComponent<MeshFilter>().mesh = ballView.listData[id].mesh;
        playerMesh.material = ballView.listData[id].material;
    }

    private void LevelManagement()
    {
        baseMaterial.color = plateMaterial.GetColor("_Tint") + Color.gray;
        newPlaneMaterial.SetColor("_Tint", plateMaterial.GetColor("_Tint"));
        particleMaterial.color = plateMaterial.GetColor("_Tint");
        currentLevelImage.color = plateMaterial.GetColor("_Tint");
        nextLevelImage.color = plateMaterial.GetColor("_Tint");

        _level = PlayerPrefs.GetInt("Level", 1);
        if (_level > 9)
            _platformAddition = 0;

        PlatformSelection();

        if (_level < 5)
        {
            ezPattern = 4;
            normalPattern = 0;
            SpawnEasyPattern();
            SpawnHardPattern();
        }
        else
        {
            number = 65 + (_level - 5) * 3;

            int a = Mathf.RoundToInt((_level - 5) / 10);

            normalPattern = 1 + (_level - 5) / 6 - _level / 10 - a;

            ezPattern = (number - normalPattern * 4) / 15;

            SpawnEasyPattern();

            SpawnHardPattern();
        }

        totalPatterns = ezPattern + normalPattern;

        for (int k = 0; k < stackPlatforms.Count; k++)
        {
            totalPlatforms += stackPlatforms[k].GetComponent<StackManager>().platformArray.Length;
        }

        if (stackPlatforms != null && stackPlatforms.Count > 1)
        {
            for (j = 1; j < stackPlatforms.Count; j++)
            {
                // Position
                float previousPos = stackPlatforms[j - 1].GetComponent<StackManager>().LastPlatformPosition();
                float newYPos = previousPos - 0.5f;
                stackPlatforms[j].transform.position = new Vector3(0, newYPos, 0);

                // Rotation
                float previousRotation = stackPlatforms[j - 1].GetComponent<StackManager>().LastPlatformRotation();
                float i = previousRotation - 5f;
                int number = _normalPlatforms.GetComponent<StackManager>().sideNumber;
                float newYRotation = i;
                if (_level > 5)
                {
                    newYRotation = i + 360 / number * UnityEngine.Random.Range(0, 3);
                }
                //if (_level > 0)
                //{
                //    newYRotation = i + 360 / number * UnityEngine.Random.Range(0, 4);
                //}
                stackPlatforms[j].transform.rotation = Quaternion.Euler(0, newYRotation, 0);
            }
        }

        for (int i = 0; i < stackPlatforms.Count; ++i)
        {
            if (!stackPlatforms[i].activeSelf)
            {
                stackPlatforms[i].SetActive(true);
            }
        }

        _winPlatform = Instantiate(_winPrefab);
        _winPlatform.transform.position = new Vector3(0, stackPlatforms[j - 1].GetComponent<StackManager>().LastPlatformPosition() - 0.5f, 0);
    }

    GameObject CreateStack(GameObject item)
    {
        GameObject gobject = Instantiate(item, stackParent);
        gobject.SetActive(false);
        return gobject;
    }

    void SpawnEasyPattern()
    {
        for (i = 0; i > -ezPattern / 2; i -= 0.5f)
        {
            _normalPlatforms = CreateStack(_selectedEasyPlatforms[UnityEngine.Random.Range(0, _selectedEasyPlatforms.Length)]);

            _normalPlatforms.GetComponent<StackManager>().InitPlatforms();

            _normalPlatforms.GetComponent<StackManager>().SetPlatformRotation();

            stackPlatforms.Add(_normalPlatforms);

            _normalPlatforms.transform.parent = stackParent;
        }
    }

    void SpawnHardPattern()
    {
        for (i = 0; i > -normalPattern / 2; i -= 0.5f)
        {
            _normalPlatforms = CreateStack(_selectedHardPlatforms[UnityEngine.Random.Range(0, _selectedHardPlatforms.Length)]);

            _normalPlatforms.GetComponent<StackManager>().InitPlatforms();

            _normalPlatforms.GetComponent<StackManager>().SetPlatformRotation();

            stackPlatforms.Add(_normalPlatforms);

            _normalPlatforms.transform.parent = stackParent;
        }
    }

    void PlatformSelection()
    {
        int randomModel = UnityEngine.Random.Range(0, platformStackList.stackList.Count);
        ColorAndBG pair = platformStackList.RandomPair(randomModel);
        background.sprite = pair.background;
        Array.Resize(ref _selectedEasyPlatforms, platformStackList.stackList[randomModel].easyStack.Length);
        platformStackList.RandomEasyStack(randomModel, _selectedEasyPlatforms);
        Array.Resize(ref _selectedHardPlatforms, platformStackList.stackList[randomModel].hardStack.Length);
        platformStackList.RandomHardStack(randomModel, _selectedHardPlatforms);
        for (int i = 0; i < _selectedEasyPlatforms.Length; i++)
        {
            _selectedEasyPlatforms[i].GetComponent<StackManager>().SetRandomPlatform(pair.material);
        }

        for (int i = 0; i < _selectedHardPlatforms.Length; i++)
        {
            _selectedHardPlatforms[i].GetComponent<StackManager>().SetRandomPlatform(pair.material);
        }
    }

    public void PoolingStack()
    {
        for (int i = 0; i < stackPlatforms.Count; i++)
            Pooler.AddPlatformToPool("Stack", stackPlatforms[i]);
    }

    public void IncreaseTheLevel()
    {
        PlayerPrefs.SetInt("Level", PlayerPrefs.GetInt("Level") + 1);
        SceneManager.LoadScene(1);
    }
}
