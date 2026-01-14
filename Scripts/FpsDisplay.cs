using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    [Header("Setări UI")]
    public TextMeshProUGUI fpsText;

    [Header("Setări Actualizare")]
    public float refreshTime = 0.5f; // Se actualizează de 2 ori pe secundă (mai lizibil)

    [Header("Culori Performanță")]
    public Color goodColor = new Color(0f, 1f, 0f, 1f);   // Verde
    public Color mediumColor = new Color(1f, 1f, 0f, 1f); // Galben
    public Color badColor = new Color(1f, 0f, 0f, 1f);    // Roșu

    private float timer;

    void Update()
    {
        // Folosim un timer simplu pentru actualizare
        if (Time.unscaledTime > timer)
        {
            // Calculăm FPS curent (1 / timpul unui frame)
            int fps = (int)(1f / Time.unscaledDeltaTime);

            // Afișăm textul
            fpsText.text = fps.ToString() + " <size=70%>FPS</size>";

            // Schimbăm culoarea în funcție de performanță
            if (fps >= 60)
                fpsText.color = goodColor;
            else if (fps >= 30)
                fpsText.color = mediumColor;
            else
                fpsText.color = badColor;

            // Resetăm timerul
            timer = Time.unscaledTime + refreshTime;
        }
    }
}