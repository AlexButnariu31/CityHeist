using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class SceneTransition : MonoBehaviour
{
    [Header("Referinte")]
    public Image transitionImage;

    [Header("Setari")]
    public float duration = 1.0f;

    private Material transMat;

    void Start()
    {
        if (transitionImage != null)
        {
            transMat = new Material(transitionImage.material);
            transitionImage.material = transMat;

            // --- MODIFICARE: Start INVERSAT ---
            // Incepem cu raza 0 (cerc negru invizibil in centru)
            SetRadius(0f);

            gameObject.SetActive(false);
        }
    }

    // Functia care ACOPERA ecranul
    public void CloseCircle(Action onComplete)
    {
        gameObject.SetActive(true);
        if (transitionImage != null) transitionImage.gameObject.SetActive(true);

        // --- MODIFICARE: Animație INVERSATĂ ---
        // Creștem cercul negru de la 0 la 1.5 (acoperă tot)
        StartCoroutine(AnimateCircle(0f, 1.5f, onComplete));
    }

    // Functia care DEZVALUIE ecranul
    public void OpenCircle(Action onComplete)
    {
        gameObject.SetActive(true);
        if (transitionImage != null) transitionImage.gameObject.SetActive(true);

        // --- MODIFICARE: Animație INVERSATĂ ---
        // Micșorăm cercul negru de la 1.5 la 0 (dispare în centru)
        StartCoroutine(AnimateCircle(1.5f, 0f, onComplete));
    }

    IEnumerator AnimateCircle(float startRadius, float endRadius, Action onComplete)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            // Smoothstep pentru miscare fluida
            t = t * t * (3f - 2f * t);

            float currentRadius = Mathf.Lerp(startRadius, endRadius, t);
            SetRadius(currentRadius);

            yield return null;
        }

        SetRadius(endRadius);

        // --- MODIFICARE: Condiție INVERSATĂ ---
        // Daca raza a ajuns la 0 (cercul a disparut complet), dezactivam imaginea.
        if (endRadius < 0.01f)
        {
            gameObject.SetActive(false);
        }

        if (onComplete != null) onComplete.Invoke();
    }

    void SetRadius(float value)
    {
        if (transMat != null)
        {
            transMat.SetFloat("_Radius", value);
        }
    }
}