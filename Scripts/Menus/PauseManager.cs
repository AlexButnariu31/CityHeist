using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instanta;

    public GameObject pauseMenu;
    public GameObject optionsMenu;

    private bool isPaused = false;

    void Awake()
    {
        if (instanta == null) instanta = this;
        else Destroy(gameObject);
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
        isPaused = true;
        Time.timeScale = 0f;

        pauseMenu.SetActive(true);
        optionsMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
