using UnityEngine;
using UnityEngine.SceneManagement; // Necesar pentru restart
using UnityEngine.UI; // Daca folosesti UI standard


using TMPro;


public class GameOverManager : MonoBehaviour
{
    public static GameOverManager instanta; // Singleton

    public TextMeshProUGUI scoreText;

    [Header("UI Referinte")]
    public GameObject panelGameOver; // Trage Panel-ul aici

    [Header("Audio")]
    public AudioSource sursaAudio;
    public AudioClip sunetGameOver;

    private bool jocTerminat = false;

    void Awake()
    {
        // Ne asiguram ca exista un singur manager
        if (instanta == null) instanta = this;
        else Destroy(gameObject);
    }

    public void DeclanseazaGameOver()
    {
        // Daca jocul s-a terminat deja, nu mai facem nimic (evitam declansari multiple)

        if (jocTerminat) return;

        jocTerminat = true;
        GameState.GameEnded = true;

        DisableAllInputScripts();
        Time.timeScale = 0f;

        GTMainMenu.EnableUICursor();
        GTMainMenu.DezactiveazaPlayer();


        if (panelGameOver != null)
            panelGameOver.SetActive(true);

        if (scoreText != null && WinManager.instanta != null)
            scoreText.text = "SCORE: " + WinManager.instanta.GetScorFinal();
    }

    // Functie pentru butonul de Restart
    public void RestartJoc()
    {
        Time.timeScale = 1f; // Repornim timpul
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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