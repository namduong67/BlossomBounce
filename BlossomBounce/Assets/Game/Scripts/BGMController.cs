using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMController : MonoBehaviour
{
    public static BGMController instance;

    public bool _bgmPlay = true;
    private AudioSource _audioSource;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        _audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (_bgmPlay)
            _audioSource.UnPause();
        else
            _audioSource.Pause();
    }

    public void BGMOnOff()
    {
        _bgmPlay = !_bgmPlay;
    }
}
