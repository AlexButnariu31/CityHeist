using UnityEngine;
using UnityEngine.SceneManagement;




public class Meniu_Pause : MonoBehaviour
{

    public static bool IsPaused = false;


    [Header("UI - CANVASURI")]
    [SerializeField] private GameObject pauseCanvas;     // Pause (Canvas)
    [SerializeField] private GameObject optionsCanvas;   // OptionsAspect (Canvas)

    private bool isPaused = false;

    void Start()
    {
        if (!pauseCanvas || !optionsCanvas)
        {
            Debug.LogError("PauseMenu: Canvas references NOT assigned!");
            enabled = false;
            return;
        }

        pauseCanvas.SetActive(false);
        optionsCanvas.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
                Pause();
            else
                Resume();
        }
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;

        pauseCanvas.SetActive(true);
        optionsCanvas.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        pauseCanvas.SetActive(false);
        optionsCanvas.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenOptions()
    {
        pauseCanvas.SetActive(false);
        optionsCanvas.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsCanvas.SetActive(false);
        pauseCanvas.SetActive(true);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

}