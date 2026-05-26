using DG.Tweening;
using ObjectPooler;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using CandyCoded.HapticFeedback;
using Nami.Controller;

public class Player : MonoBehaviour
{
    private Rigidbody _rb;
    private float _overpowerBuildUp;
    public MeshRenderer playerMesh;
    [SerializeField] Animator anim;
    public ParticleSystem splashEffect;
    [SerializeField] private bool _isClicked, _isOverPowered;
    [SerializeField] private float _moveSpeed = 500f;
    private float _speedLimit = 5f;
    [SerializeField] private float _bounceSpeed = 500f;
    [SerializeField] bool onePlatformBreak = false;
    [SerializeField] GameUI gameUI;

    public enum PlayerState
    {
        Prepare,
        Play,
        Revived,
        Dead,
        Finish
    }

    public PlayerState playerState = PlayerState.Prepare;

    public AudioClip bounceClip, deadClip, breakClip, winClip, _overpowerBreakClip;

    private int currentBrokenPlatforms, totalPlatforms;

    public GameObject _overpowerBar;
    public Image _overpowerFill;
    public GameObject leafEffect;
    public GameObject magicLeafEffect;
    public GameObject winEffect;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        currentBrokenPlatforms = 0;
    }

    void Start()
    {
        totalPlatforms = FindObjectOfType<LevelSpawner>().totalPlatforms;
    }

    void Update()
    {
        if (playerState == PlayerState.Play)
        {
            ClickCheck();
            OverpowerCheck();
        }

        if (playerState == PlayerState.Revived)
        {
            anim.SetBool("Dead", false);
            _rb.isKinematic = false;
            ClickCheck();
            OverpowerCheck();
        }

        if (_rb.velocity.y < 0)
            anim.SetBool("Falling", true);
        else
            anim.SetBool("Falling", false);

        if (_isClicked || onePlatformBreak)
            anim.SetBool("Down", true);
    }

    void FixedUpdate()
    {
        BallMovement();
    }

    private void BallMovement()
    {
        if (playerState == PlayerState.Play || playerState == PlayerState.Revived)
        {
            if (Input.GetMouseButton(0) && _isClicked == true)
            {
                Limbs.isRotate = false;
                _rb.velocity = new Vector3(0, -_moveSpeed * Time.fixedDeltaTime / Time.timeScale, 0);
            }
        }

        if (_rb.velocity.y > _speedLimit && _isClicked == true)
        {
            _rb.velocity = new Vector3(_rb.velocity.x, _speedLimit, _rb.velocity.z);
        }
    }

    public void ClickCheck()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isClicked = true;
            onePlatformBreak = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _isClicked = false;
        }
    }

    public void IncreaseScore()
    {
        currentBrokenPlatforms++;
        if (!_isOverPowered)
        {
            ScoreHandler.instance.AddScore(1);
            SoundManager.instance.PlaySoundEffect(breakClip, .5f);
        }
        else
        {
            ScoreHandler.instance.AddScore(2);
            SoundManager.instance.PlaySoundEffect(_overpowerBreakClip, .5f);
        }
    }

    void OnCollisionEnter(Collision target)
    {
        if (!_isClicked)
        {
            _rb.velocity = new Vector3(0, _bounceSpeed * Time.deltaTime, 0);

            Limbs.collided = true;
            anim.SetBool("Down", false);

            if (!target.gameObject.CompareTag("Finish"))
            {
                foreach (ContactPoint missileHit in target.contacts)
                {
                    Vector3 hit = missileHit.point;
                    var splash = Pooler.SpawnFromPool("splash", new Vector3(hit.x, hit.y + 0.1f, hit.z), Quaternion.identity);
                    splash.transform.SetParent(target.transform);
                    splash.transform.localEulerAngles = new Vector3(90, Random.Range(0, 359), 0);
                    splash.GetComponent<ParticleSystem>().startColor = playerMesh.material.GetColor("_Color");
                    StartCoroutine(delayParticle(splash));
                }

                IEnumerator delayParticle(GameObject pool)
                {
                    yield return new WaitUntil(() => !pool.GetComponent<ParticleSystem>().isPlaying);
                    Pooler.AddToPool("splash", pool);
                }
            }
            SoundManager.instance.PlaySoundEffect(bounceClip, .5f);
        }

        if (onePlatformBreak)
        {
            Limbs.isRotate = false;
            anim.SetBool("Down", true);
            if (_isOverPowered)
            {
                if (target.gameObject.tag == "GoodPlatform" || target.gameObject.tag == "BadPlatform")
                {
                    target.transform.parent.GetComponent<NewPlatformController>().BreakAllPlatforms();
                    onePlatformBreak = false;
                    anim.SetBool("Down", false);

                    if (SoundManager.instance.isHaptic)
                        HapticFeedback.LightFeedback();
                }
            }
            else
            {
                if (target.gameObject.tag == "GoodPlatform")
                {
                    target.transform.parent.GetComponent<NewPlatformController>().BreakAllPlatforms();
                    onePlatformBreak = false;
                    anim.SetBool("Down", false);

                    if (SoundManager.instance.isHaptic)
                        HapticFeedback.LightFeedback();
                }

                if (target.gameObject.tag == "BadPlatform")
                {
                    if (SoundManager.instance.isHaptic)
                        HapticFeedback.HeavyFeedback();
                    gameUI.WantToRevive();
                    _rb.isKinematic = true;
                    anim.SetBool("Dead", true);
                    anim.SetBool("Down", false);
                    _isClicked = false;
                    onePlatformBreak = false;
                    playerState = PlayerState.Dead;
                    ScoreHandler.instance.skinCount++;
                    //leafEffect.SetActive(false);
                    SoundManager.instance.PlaySoundEffect(deadClip, 1f);
                    //GameFirebase.SendEvent("level", "die", PlayerPrefs.GetInt("Level").ToString());
                }
            }
        }

        if (!onePlatformBreak && _isClicked)
        {
            Limbs.isRotate = false;

            if (_isOverPowered)
            {
                if (target.gameObject.tag == "GoodPlatform" || target.gameObject.tag == "BadPlatform")
                {
                    target.transform.parent.GetComponent<NewPlatformController>().BreakAllPlatforms();
                    anim.SetBool("Down", true);

                    if (SoundManager.instance.isHaptic)
                        HapticFeedback.LightFeedback();
                }
            }
            else
            {
                if (target.gameObject.tag == "GoodPlatform")
                {
                    target.transform.parent.GetComponent<NewPlatformController>().BreakAllPlatforms();
                    anim.SetBool("Down", true);

                    if (SoundManager.instance.isHaptic)
                        HapticFeedback.LightFeedback();
                }

                if (target.gameObject.tag == "BadPlatform")
                {
                    if (SoundManager.instance.isHaptic)
                        HapticFeedback.HeavyFeedback();
                    gameUI.WantToRevive();
                    _rb.isKinematic = true;
                    anim.SetBool("Dead", true);
                    anim.SetBool("Down", false);
                    _isClicked = false;
                    onePlatformBreak = false;
                    playerState = PlayerState.Dead;
                    ScoreHandler.instance.skinCount++;
                    //leafEffect.SetActive(false);
                    SoundManager.instance.PlaySoundEffect(deadClip, 1f);
                    //GameFirebase.SendEvent("level", "die", PlayerPrefs.GetInt("Level").ToString());
                }
            }

        }

        gameUI.LevelSliderFill(currentBrokenPlatforms / (float)totalPlatforms);

        if (target.gameObject.CompareTag("Finish") && (playerState == PlayerState.Play || playerState == PlayerState.Revived))
        {
            _isClicked = false;
            SoundManager.instance.PlaySoundEffect(winClip, 0.8f);
            playerState = PlayerState.Finish;
            GameObject win = Instantiate(winEffect);
            win.transform.SetParent(Camera.main.transform);
            win.transform.localPosition = Vector3.up * 1.5f;
            win.transform.eulerAngles = Vector3.zero;
            ScoreHandler.instance.skinCount++;
            ShowLogFireBase.Instance.ShowCompleteLevel();
        }
    }

    void OnCollisionStay(Collision target)
    {
        if (!_isClicked || target.gameObject.CompareTag("Finish"))
        {
            anim.SetBool("Down", false);
            Limbs.isRotate = false;
            Limbs.collided = true;
            _rb.velocity = new Vector3(0, _bounceSpeed * Time.deltaTime, 0);
        }
    }

    void OverpowerCheck()
    {
        if (_isOverPowered)
        {
            _overpowerBuildUp -= Time.deltaTime * .3f / Time.timeScale;
            if (!magicLeafEffect.activeSelf)
                magicLeafEffect.SetActive(true);
        }
        else
        {
            if (magicLeafEffect.activeSelf)
                magicLeafEffect.SetActive(false);
            if (_isClicked)
                _overpowerBuildUp += Time.deltaTime * .8f / Time.timeScale;
            else
                _overpowerBuildUp -= Time.deltaTime * .5f / Time.timeScale;
        }

        if (_overpowerBuildUp >= 0.3f || _overpowerFill.color == Color.red)
            _overpowerBar.SetActive(true);
        else
            _overpowerBar.SetActive(false);

        if (_overpowerBuildUp >= 1)
        {
            _overpowerBuildUp = 1;
            _isOverPowered = true;
            _overpowerFill.color = Color.red;
        }
        else if (_overpowerBuildUp <= 0)
        {
            _overpowerBuildUp = 0;
            _isOverPowered = false;
            _overpowerFill.color = Color.white;
        }

        if (_overpowerBar.activeInHierarchy)
            _overpowerFill.fillAmount = _overpowerBuildUp;
    }


}
