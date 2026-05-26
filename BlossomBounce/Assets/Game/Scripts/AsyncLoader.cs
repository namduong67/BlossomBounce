using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AsyncLoader : MonoBehaviour
{
    [SerializeField] Image loadingSlider;

    void Start()
    {
        StartCoroutine(LoadLevelAsync(1));
    }

    IEnumerator LoadLevelAsync(int levelToLoad)
    {
        loadingSlider.fillAmount = 0;

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);
        loadOperation.allowSceneActivation = false;
        float progress = 0;
        while(!loadOperation.isDone)
        {
            progress = Mathf.MoveTowards(progress, loadOperation.progress, Time.deltaTime);
            loadingSlider.fillAmount = progress;
            if (progress >= 0.9f)
            {
                loadingSlider.fillAmount = 1;
                loadOperation.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}
