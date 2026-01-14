using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class StealableObject : MonoBehaviour
{
    [Header("Setari Furt")]
    public float timeToSteal = 2.0f;

    // Aici poti ajusta manual daca tot nu iti place, dar default e 0
    public Vector3 manualPositionOffset = new Vector3(0, 0.2f, 0);

    [Header("Resurse Automate")]
    public AudioClip holdSound;
    public AudioClip finishSound;
    public GameObject uiCanvas;
    public Image progressBar;

    private Camera mainCamera;
    private bool isStolen = false;
    private AudioSource audioSource;

    void Start()
    {
        mainCamera = Camera.main;

        AutoSetupReferences();

        if (uiCanvas != null) uiCanvas.SetActive(false);

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 1.0f;
        audioSource.clip = holdSound;
    }

    void AutoSetupReferences()
    {
        if (holdSound == null) holdSound = Resources.Load<AudioClip>("StealHold");
        if (finishSound == null) finishSound = Resources.Load<AudioClip>("StealFinish");

        if (uiCanvas == null)
        {
            Canvas existingCanvas = GetComponentInChildren<Canvas>(true);
            if (existingCanvas != null)
            {
                uiCanvas = existingCanvas.gameObject;
            }
            else
            {
                GameObject prefab = Resources.Load<GameObject>("StealUI");
                if (prefab != null)
                {
                    GameObject newUI = Instantiate(prefab);
                    newUI.transform.SetParent(transform);

                    // --- POZITIONARE INTELIGENTA ---
                    // Calculam centrul si inaltimea obiectului folosind Collider-ul
                    Collider col = GetComponent<Collider>();
                    if (col != null)
                    {
                        // Gasim punctul cel mai de sus al obiectului (Top Center)
                        Vector3 topCenter = col.bounds.center;
                        topCenter.y = col.bounds.max.y;

                        // Mutam UI-ul acolo + un mic offset manual
                        newUI.transform.position = topCenter + manualPositionOffset;
                    }
                    else
                    {
                        // Fallback daca nu are collider: il punem putin mai sus de pivot
                        newUI.transform.localPosition = new Vector3(0, 0.5f, 0);
                    }

                    newUI.transform.localRotation = Quaternion.identity;
                    newUI.transform.localScale = prefab.transform.localScale;

                    uiCanvas = newUI;
                }
            }
        }

        if (progressBar == null && uiCanvas != null)
        {
            Image[] images = uiCanvas.GetComponentsInChildren<Image>(true);
            foreach (Image img in images)
            {
                if (img.type == Image.Type.Filled)
                {
                    progressBar = img;
                    break;
                }
            }
        }
    }

    void Update()
    {
        // Aceasta parte face UI-ul sa se uite la tine (Billboard)
        if (uiCanvas != null && uiCanvas.activeSelf && mainCamera != null)
        {
            // Pasul 1: Copiem rotatia camerei (Astfel nu se mai deformeaza cand te uiti de sus)
            uiCanvas.transform.rotation = mainCamera.transform.rotation;

            // Pasul 2: Rotim cu 180 de grade (Pentru ca UI-ul sa fie cu fata, nu cu spatele)
            // Folosim Space.Self ca sa se roteasca in jurul propriei axe
            uiCanvas.transform.Rotate(0, 180, 0);
        }
    }

    // ... Restul functiilor raman la fel ...
    public void StartHoldSound()
    {
        // --- MODIFICARE 1: Blocaj de siguranta ---
        // Daca obiectul e deja furat, refuzam sa mai pornim sunetul de hold
        if (isStolen) return;
        // ----------------------------------------

        if (audioSource != null && !audioSource.isPlaying && holdSound != null)
        {
            audioSource.clip = holdSound;
            audioSource.Play();
        }
    }
    public void StopHoldSound() { if (audioSource != null && audioSource.isPlaying) audioSource.Stop(); }
    public void ShowUI(bool show) { if (isStolen) return; if (uiCanvas != null) uiCanvas.SetActive(show); if (!show) StopHoldSound(); }
    public void UpdateProgress(float percentage) { if (progressBar != null) progressBar.fillAmount = percentage; }
    public void Steal()
    {
        

        if (isStolen) return;
        isStolen = true;

        // Anuntam WinManager despre furt
        if (WinManager.instanta != null)
            WinManager.instanta.InregistreazaFurt();

        StopHoldSound();
        ShowUI(false);

        // --- MODIFICARE 2: Oprire brutala a sunetului ---
        if (audioSource != null)
        {
            audioSource.Stop();       // Opreste redarea curenta
            audioSource.enabled = false; // Dezactiveaza componenta complet
        }
        // ------------------------------------------------

        if (finishSound != null) AudioSource.PlayClipAtPoint(finishSound, transform.position);

        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        foreach (Transform child in transform) child.gameObject.SetActive(false);

        // Obiectul ramane in scena inca 2 secunde, dar acum e complet "mut" si invizibil
        Destroy(gameObject, 2.0f);
    }

    public bool IsStolen() { return isStolen; }
}