using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class GameMenu : MonoBehaviour
{


    public void Play()
    {
        PlayerPrefs.SetString("NextScene", "abc");
        SceneManager.LoadScene("LoadingSc");
    }

    public void Quit()
    {
        Application.Quit();
    }




}