using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;


public class LoadingGame : MonoBehaviour
{
    public Slider loadingBar;

    void Start()
    {

        if (loadingBar == null)
        {
            loadingBar = FindObjectOfType<Slider>();
            if (loadingBar == null)
            {
                Debug.LogError("Nu s-a gãsit niciun Slider în scenã!");
                return;
            }
        }

        StartCoroutine(LoadNextScene());
    }

    IEnumerator LoadNextScene()
    {
        string nextScene = PlayerPrefs.GetString("NextScene");

        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;


        float displayedProgress = 0f;

        while (!op.isDone)
        {
            float targetProgress = Mathf.Clamp01(op.progress / 0.9f);
            displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, Time.deltaTime * 0.5f);
            loadingBar.value = displayedProgress;

            if (op.progress >= 0.9f && displayedProgress >= 1f)
            {
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }



}
