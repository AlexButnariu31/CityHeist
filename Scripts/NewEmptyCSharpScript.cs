using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class DoorObject : MonoBehaviour
{
    [Header("Setari Usa")]
    public float timeToOpen = 1.0f; // Timp mai scurt decat la furt
    public float openAngle = 90f;   // La cate grade se deschide
    public float doorSpeed = 2f;    // Viteza animatiei
    public Vector3 manualPositionOffset = new Vector3(0, 0, 0);

    [Header("Resurse Automate")]
    public AudioClip lockedSound; // Sunet cand tii apasat (ex: clanta)
    public AudioClip openSound;   // Sunet cand se deschide (scartait)
    public GameObject uiCanvas;
    public Image progressBar;

    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private AudioSource audioSource;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        // Retinem pozitia inchisa si calculam pozitia deschisa
        closedRotation = transform.localRotation;
        openRotation = Quaternion.Euler(0, openAngle, 0) * closedRotation;

        AutoSetupReferences();

        if (uiCanvas != null) uiCanvas.SetActive(false);

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f;
    }

    void Update()
    {
        // 1. Rotim UI-ul spre camera (fara deformare)
        if (uiCanvas != null && uiCanvas.activeSelf && mainCamera != null)
        {
            uiCanvas.transform.rotation = mainCamera.transform.rotation;
            uiCanvas.transform.Rotate(0, 180, 0);
        }

        // 2. Animam Usa (Smooth Rotation)
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * doorSpeed);
    }

    // --- LOGICA DE INTERACTIUNE ---

    public void ToggleDoor()
    {
        isOpen = !isOpen; // Deschide sau Inchide

        // Oprim sunetul de "hold" si UI-ul
        StopInteractSound();
        ShowUI(false);

        // Redam sunetul de deschidere/inchidere
        if (openSound != null) AudioSource.PlayClipAtPoint(openSound, transform.position);
    }

    // --- FUNCTII AJUTATOARE (Audio & UI) ---

    public void StartInteractSound()
    {
        if (!audioSource.isPlaying && lockedSound != null)
        {
            audioSource.clip = lockedSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void StopInteractSound()
    {
        if (audioSource.isPlaying) audioSource.Stop();
    }

    public void ShowUI(bool show)
    {
        if (uiCanvas != null) uiCanvas.SetActive(show);
        if (!show) StopInteractSound();
    }

    public void UpdateProgress(float percentage)
    {
        if (progressBar != null) progressBar.fillAmount = percentage;
    }

    // --- SETUP AUTOMAT (Exact ca la StealableObject) ---
    void AutoSetupReferences()
    {
        // Poti pune alte sunete in Resources daca vrei (ex: "DoorHandle", "DoorCreak")
        // Momentan le refolosim pe cele de furt daca nu ai altele
        if (lockedSound == null) lockedSound = Resources.Load<AudioClip>("StealHold");
        if (openSound == null) openSound = Resources.Load<AudioClip>("StealFinish");

        if (uiCanvas == null)
        {
            Canvas existingCanvas = GetComponentInChildren<Canvas>(true);
            if (existingCanvas != null) uiCanvas = existingCanvas.gameObject;
            else
            {
                GameObject prefab = Resources.Load<GameObject>("StealUI");
                if (prefab != null)
                {
                    GameObject newUI = Instantiate(prefab);
                    newUI.transform.SetParent(transform);

                    // Pozitionam UI-ul pe mijlocul usii
                    Collider col = GetComponent<Collider>();
                    if (col == null) col = GetComponentInChildren<Collider>(); // Cautam si in copii (grafica usii)

                    if (col != null)
                    {
                        Vector3 centerPos = col.bounds.center;
                        // Il punem putin in fata usii ca sa nu intre in ea
                        newUI.transform.position = centerPos + manualPositionOffset;
                    }
                    else
                    {
                        newUI.transform.localPosition = new Vector3(0, 1f, 0);
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
            foreach (Image img in images) if (img.type == Image.Type.Filled) { progressBar = img; break; }
        }
    }
}