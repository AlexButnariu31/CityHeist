using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsPause : MonoBehaviour
{
    [SerializeField] private Slider MasterV;
    [SerializeField] private AudioMixer audioMix;

    [SerializeField] private Slider MusicV;
    [SerializeField] private Slider SoundsV;

    private void Start()
    {
        RefreshSett();
    }

    public void RefreshSett()
    {
        MasterV.value = Settings.masterVolume;
        MusicV.value = Settings.musicVolume;
        SoundsV.value = Settings.sfxVolume;

        Apply();
    }

    public void Apply()
    {
        Settings.masterVolume = MasterV.value;
        Settings.musicVolume = MusicV.value;
        Settings.sfxVolume = SoundsV.value;

        // MASTER = afectează tot
        audioMix.SetFloat("MasterVolume", Mathf.Log10(Settings.masterVolume) * 20);

        // MUSIC = se înmulțește cu MASTER
        float finalMusic = Settings.musicVolume * Settings.masterVolume;
        audioMix.SetFloat("MusicVolume", Mathf.Log10(finalMusic <= 0 ? 0.0001f : finalMusic) * 20);

        // SFX = se înmulțește cu MASTER
        float finalSfx = Settings.sfxVolume * Settings.masterVolume;
        audioMix.SetFloat("SFXVolume", Mathf.Log10(finalSfx <= 0 ? 0.0001f : finalSfx) * 20);
    }
}
