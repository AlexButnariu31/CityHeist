using UnityEngine;
using System.Collections;

public class ManagerMuzica : MonoBehaviour
{
    public static ManagerMuzica instanta; // Singleton pentru acces rapid

    public AudioSource sursaAudio;
    public float vitezaFade = 0.5f; // Cat de repede creste/scade volumul
    public float volumMaxim = 0.8f;

    private int numarPolitistiCareUrmaresc = 0;
    private Coroutine corutinaFade;

    void Awake()
    {
        // Ne asiguram ca exista doar un singur ManagerMuzica
        if (instanta == null) instanta = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (sursaAudio != null)
        {
            sursaAudio.volume = 0;
            sursaAudio.loop = true; // Melodia trebuie sa se repete
            if (!sursaAudio.isPlaying) sursaAudio.Play();
        }
    }

    // Aceasta functie e apelata de Politist
    public void PolitistAInceputUrmarirea()
    {
        numarPolitistiCareUrmaresc++;

        // Daca e primul politist (1), pornim muzica
        if (numarPolitistiCareUrmaresc == 1)
        {
            PornesteFade(volumMaxim);
        }
    }

    // Aceasta functie e apelata de Politist
    public void PolitistATerminatUrmarirea()
    {
        numarPolitistiCareUrmaresc--;

        if (numarPolitistiCareUrmaresc < 0) numarPolitistiCareUrmaresc = 0;

        // Daca nu mai e nimeni (0), oprim muzica
        if (numarPolitistiCareUrmaresc == 0)
        {
            PornesteFade(0f);
        }
    }

    void PornesteFade(float volumTinta)
    {
        if (!gameObject.activeInHierarchy) return; 

        if (corutinaFade != null) StopCoroutine(corutinaFade);
        corutinaFade = StartCoroutine(ProcesFade(volumTinta));
    }

    IEnumerator ProcesFade(float tinta)
    {
        if (sursaAudio == null) yield break;

        float startVolume = sursaAudio.volume;
        float timp = 0f;

        while (timp < 1f)
        {
            timp += Time.deltaTime * vitezaFade;
            sursaAudio.volume = Mathf.Lerp(startVolume, tinta, timp);
            yield return null;
        }

        sursaAudio.volume = tinta;
    }
}