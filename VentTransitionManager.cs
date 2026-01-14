using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;
using System;

public class VentTransitionManager : MonoBehaviour
{
    [Header("Referinte")]
    public VideoPlayer videoPlayer;
    public RawImage screenImage;

    [Header("Setari Intrare")]
    public double startEnterTime = 0.0;
    public double durationEnter = 2.5;

    [Header("Setari Iesire")]
    public double startExitTime = 3.0;
    public double durationExit = 2.0;

    void Start()
    {
        // Ne asiguram ca ecranul e stins la inceput
        if (screenImage != null)
        {
            screenImage.gameObject.SetActive(false);
        }

        if (videoPlayer != null)
        {
            videoPlayer.Prepare();
        }
    }

    public void PlayEnterAnimation(Action onComplete)
    {
        StartCoroutine(PlayVideoRoutine(startEnterTime, durationEnter, onComplete));
    }

    public void PlayExitAnimation(Action onComplete)
    {
        StartCoroutine(PlayVideoRoutine(startExitTime, durationExit, onComplete));
    }

    IEnumerator PlayVideoRoutine(double startTime, double duration, Action onComplete)
    {
        if (screenImage != null) screenImage.gameObject.SetActive(true);

        if (videoPlayer != null)
        {
            videoPlayer.time = startTime;
            videoPlayer.Play();
        }

        yield return new WaitForSeconds((float)duration);

        if (videoPlayer != null) videoPlayer.Pause();

        if (screenImage != null) screenImage.gameObject.SetActive(false);

        if (onComplete != null) onComplete.Invoke();
    }
}