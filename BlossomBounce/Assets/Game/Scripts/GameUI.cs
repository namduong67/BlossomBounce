using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;
using Nami.Controller;

public class GameUI : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private LevelSpawner _levelSpawner;
    [SerializeField] GameObject warning;
    [SerializeField] RectTransform warningSpawnPos;
    public Image levelSlider, levelSliderFill;
    public Image currentLevelImg;
    public Image nextLevelImg;
    public GameObject loadingRewardAds;
    public GameObject blurImg;
    public GameObject firstUI, inGameUI, finishUI, gameOverUI;
    public GameObject allButtons;
    private bool _buttons;
    public TextMeshProUGUI currentLevelText, finishLevelText, gameOverScoreText, gameOverBestText;

    [Header("Sound Img")]
    public Sprite soundOnImg;
    public Sprite soundOffImg;

    [Header("Haptic Img")]
    public Sprite hapticOnImg;
    public Sprite hapticOffImg;

    [Header("Music Img")]
    public Sprite musicOnImg;
    public Sprite musicOffImg;

    [Header("Setting")]
    public Button hapticButton;
    public Button soundButton;
    public Button musicButton;

    private Material _plateMaterial;

    [Space(5f)]
    [Header("Shop")]
    public Button shopBtn;
    public Button closeShopBtn;
    public GameObject shopPanel;
    public GameObject ballView;
    public UnlockSkin unlockSkin;
    public GameObject noti;

    [Space(5f)]
    [Header("Revive System")]
    public GameObject reviveUI;
    public Button reviveBtn;
    public Image reviveCountdown;
    public Button noThanksTxt;
    private bool revived;
    private bool _isShowAdsFinished = false;
    private float reviveTime = 5;

    void Awake()
    {
        _plateMaterial = FindObjectOfType<LevelSpawner>().plateMaterial;
        nextLevelImg.color = _plateMaterial.GetColor("_Tint");
        currentLevelImg.color = _plateMaterial.GetColor("_Tint");
        shopPanel.SetActive(false);
        ballView.transform.localScale = Vector3.zero;
        shopBtn.onClick.AddListener(() =>
        {
            OpenShop();
            UnlockSkin.instance.OpenStore();
        });
        closeShopBtn.onClick.AddListener(CloseShop);

        // Settings
        hapticButton.onClick.AddListener(SoundManager.instance.HapticOnOff);
        soundButton.onClick.AddListener(SoundManager.instance.SoundOnOff);
        musicButton.onClick.AddListener(BGMController.instance.BGMOnOff);

        // Revive
        reviveBtn.onClick.AddListener(GetARevive);
        noThanksTxt.GetComponent<TextMeshProUGUI>().DOFade(0, 0.5f);
        noThanksTxt.gameObject.SetActive(false);
        noThanksTxt.onClick.AddListener(GameOver);

        // Finish
        finishUI.GetComponent<Button>().onClick.AddListener(NextLevel);

        // Game Over
        gameOverUI.GetComponent<Button>().onClick.AddListener(Replay);
    }

    void Start()
    {
        currentLevelText.text = _levelSpawner._level.ToString();
    }

    void Update()
    {
        UIManagement();

        if (GameAds.Get.IsShowAds)
            Time.timeScale = 0;
        else
            Time.timeScale = 1.5f;
    }

    private void UIManagement()
    {
        noti.SetActive(unlockSkin.newSkinUnlocked);

        if (_player.playerState == Player.PlayerState.Prepare)
        {
            if (SoundManager.instance._soundPlay && soundButton.GetComponent<Image>().sprite != soundOnImg)
            {
                soundButton.GetComponent<Image>().sprite = soundOnImg;
            }

            else if (!SoundManager.instance._soundPlay && soundButton.GetComponent<Image>().sprite != soundOffImg)
            {
                soundButton.GetComponent<Image>().sprite = soundOffImg;
            }

            if (SoundManager.instance.isHaptic && hapticButton.GetComponent<Image>().sprite != hapticOnImg)
            {
                hapticButton.GetComponent<Image>().sprite = hapticOnImg;
            }

            else if (!SoundManager.instance.isHaptic && hapticButton.GetComponent<Image>().sprite != hapticOffImg)
            {
                hapticButton.GetComponent<Image>().sprite = hapticOffImg;
            }

            if (BGMController.instance._bgmPlay && musicButton.GetComponent<Image>().sprite != musicOnImg)
            {
                musicButton.GetComponent<Image>().sprite = musicOnImg;
            }

            else if (!BGMController.instance._bgmPlay && musicButton.GetComponent<Image>().sprite != musicOffImg)
            {
                musicButton.GetComponent<Image>().sprite = musicOffImg;
            }
        }

        if (Input.GetMouseButtonDown(0) && !IgnoreUI() && _player.playerState == Player.PlayerState.Prepare)
        {
            _player.playerState = Player.PlayerState.Play;
            firstUI.SetActive(false);
            inGameUI.SetActive(true);
            finishUI.SetActive(false);
            gameOverUI.SetActive(false);
        }

        if (_player.playerState == Player.PlayerState.Revived)
        {
            firstUI.SetActive(false);
            inGameUI.SetActive(true);
            blurImg.SetActive(false);
            finishUI.SetActive(false);
            gameOverUI.SetActive(false);
        }

        if (_player.playerState == Player.PlayerState.Finish)
        {
            firstUI.SetActive(false);
            inGameUI.SetActive(false);
            blurImg.SetActive(true);
            finishUI.SetActive(true);
            gameOverUI.SetActive(false);
            if (!_isShowAdsFinished)
            {
                _isShowAdsFinished = true;
                GameAds.Get.ShowInterstitialAd();
            }

            finishLevelText.text = "Level " + _levelSpawner._level;
        }

        if (_player.playerState == Player.PlayerState.Dead)
        {
            firstUI.SetActive(false);
            inGameUI.SetActive(false);
            finishUI.SetActive(false);
            blurImg.SetActive(true);

            if (reviveUI.activeSelf)
                StartCountdown();

            gameOverScoreText.text = ScoreHandler.instance.score.ToString();
            gameOverBestText.text = PlayerPrefs.GetInt("Highscore").ToString();

            if (gameOverUI.activeSelf)
            {
                if (!_isShowAdsFinished)
                {
                    _isShowAdsFinished = true;
                    GameAds.Get.ShowInterstitialAd();
                }
            }
        }
    }

    private void NextLevel()
    {
        _levelSpawner.IncreaseTheLevel();
    }

    private void Replay()
    {
        ShowLogFireBase.Instance.AddNumberTriesLevel();
        ScoreHandler.instance.ResetScore();
        SceneManager.LoadScene(1);
    }

    private bool IgnoreUI()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;
        List<RaycastResult> raycastResultList = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResultList);
        for (int i = 0; i < raycastResultList.Count; i++)
        {
            if (raycastResultList[i].gameObject.GetComponent<IgnoreUI>() != null)
            {
                raycastResultList.RemoveAt(i);
                i--;
            }
        }

        return raycastResultList.Count > 0;
    }

    public void LevelSliderFill(float fillAmount)
    {
        levelSliderFill.fillAmount = fillAmount;
    }

    public void Settings()
    {
        _buttons = !_buttons;
        allButtons.SetActive(_buttons);
    }

    private void OpenShop()
    {
        shopPanel.SetActive(true);
        ballView.transform.DOScale(1, 0.5f).SetEase(Ease.OutBack);
    }

    private void CloseShop()
    {
        ballView.transform.DOScale(0, 0.5f).SetEase(Ease.InBack).OnComplete(() => shopPanel.SetActive(false));
    }

    public void WantToRevive()
    {
        if (revived)
        {
            GameOver();
        }
        else
        {
            reviveUI.SetActive(true);
            StartCountdown();

            StartCoroutine(Timeline());

            IEnumerator Timeline()
            {
                yield return new WaitForSeconds(1.5f);
                noThanksTxt.gameObject.SetActive(true);
                noThanksTxt.GetComponent<TextMeshProUGUI>().DOFade(1, 0.5f);
            }
        }
    }

    private void StartCountdown()
    {
        reviveCountdown.fillAmount -= 1 / reviveTime * Time.deltaTime;
        if (reviveCountdown.fillAmount <= 0)
            GameOver();
    }

    public void GameOver()
    {
        reviveUI.SetActive(false);
        gameOverUI.SetActive(true);
        GameFirebase.SendEvent("best_score", "best_score", PlayerPrefs.GetInt("Highscore").ToString(),
                "id_level", PlayerPrefs.GetInt("Level").ToString());
    }

    public void RevivePlayer()
    {
        revived = true;
        _player.playerState = Player.PlayerState.Revived;
        GameFirebase.SendEvent("revive", "id_level", PlayerPrefs.GetInt("Level").ToString());
    }

    public void GetARevive()
    {
        loadingRewardAds.SetActive(true);
        noThanksTxt.interactable = false;
        reviveBtn.interactable = false;
        gameOverUI.GetComponent<Button>().interactable = false;
        GameAds.Get.LoadAndShowRewardAd((v) =>
        {
            noThanksTxt.interactable = true;
            reviveBtn.interactable = true;
            gameOverUI.GetComponent<Button>().interactable = true;
            if (v)
            {
                Debug.Log($"Rewarded");
                loadingRewardAds.SetActive(false);
                reviveUI.SetActive(false);
                RevivePlayer();
            }
            else
            {
                Debug.Log($"Fail");
                CantGetRewardAd();
            }
        });
    }

    public void CantGetRewardAd()
    {
        Vector3 spawnPos = new Vector3(warningSpawnPos.position.x, warningSpawnPos.position.y - 5, warningSpawnPos.position.z);
        GameObject warn = Instantiate(warning, warningSpawnPos.position, Quaternion.identity, transform);
        warn.GetComponent<Image>().DOFade(1, 0.25f);
        warn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().DOFade(1, 0.25f).OnComplete(() =>
        {
            StartCoroutine(Timeline());

            IEnumerator Timeline()
            {
                yield return new WaitForSeconds(0.5f);
                warn.GetComponent<Image>().DOFade(0, 0.25f);
                warn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().DOFade(0, 0.25f).OnComplete(() =>
                {
                    Destroy(warn);
                });
            }
        });
    }
}
