using UnityEngine;

using TMPro; // daca folosesti TMP

public class WinManager : MonoBehaviour
{
    public static WinManager instanta;

    [Header("Victory UI")]
    public GameObject victoryCanvas;
    public TextMeshProUGUI scoreText;


    public int totalPachete;
    private int pacheteFurate = 0;
    private bool victoryTriggered = false;

    void Start()
    {
        totalPachete =  FindObjectsOfType<StealableObject>().Length;
    }

    void Awake()
    {
        if (instanta == null) instanta = this;
        else Destroy(gameObject);
    }

    public void InregistreazaFurt()
    {
        if (victoryTriggered) return;

        pacheteFurate++;
        Debug.Log($"Pachet furat: {pacheteFurate}/{totalPachete}");

        if (pacheteFurate >= totalPachete)
        {
            Castig();
        }
    }

    void Castig()
    {
        if (victoryTriggered) return;

        victoryTriggered = true;
        GameState.GameEnded = true;

        Time.timeScale = 0f;

        GTMainMenu.EnableUICursor();
        GTMainMenu.DezactiveazaPlayer();

        // OPRIM scripturi problematice
        DisableAllInputScripts();

        if (victoryCanvas != null)
            victoryCanvas.SetActive(true);

        if (scoreText != null)
            scoreText.text = "SCORE: " + GetScorFinal();

    }

    public int GetScorFinal()
    {
        return pacheteFurate * 245;
    }

    void DisableAllInputScripts()
    {
        ThirdPersonCamera cam = FindObjectOfType<ThirdPersonCamera>();
        if (cam) cam.enabled = false;

        SimpleMove sm = FindObjectOfType<SimpleMove>();
        if (sm) sm.enabled = false;

        VentSystem vs = FindObjectOfType<VentSystem>();
        if (vs) vs.enabled = false;

        PlayerInteraction pi = FindObjectOfType<PlayerInteraction>();
        if (pi) pi.enabled = false;

        PlayerMove pm = FindObjectOfType<PlayerMove>();
        if (pm) pm.enabled = false;
    }
}