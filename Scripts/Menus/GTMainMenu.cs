using UnityEngine;
using UnityEngine.SceneManagement;

public class GTMainMenu : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public void GoToMainMenu()
    {
        GameState.GameEnded = false;

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public static void EnableUICursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public static void DezactiveazaPlayer()
    {
        PlayerMove pm = FindObjectOfType<PlayerMove>();
        if (pm != null) pm.enabled = false;

        PlayerSteal ps = FindObjectOfType<PlayerSteal>();
        if (ps != null) ps.enabled = false;

        PlayerInteraction pi = FindObjectOfType<PlayerInteraction>();
        if (pi != null) pi.enabled = false;

        ThirdPersonCamera cam = FindObjectOfType<ThirdPersonCamera>();
        if (cam != null) cam.enabled = false;

        SimpleMove sm = FindObjectOfType<SimpleMove>();
        if (sm != null) sm.enabled = false;

        VentSystem vs = FindObjectOfType<VentSystem>();
        if (vs != null) vs.enabled = false;
    }
}
