using UnityEngine;
using UnityEngine.UI; // Necesar pentru a modifica UI-ul (Image, Text)

public class VentObject : MonoBehaviour
{
    [Header("Setari UI")]
    public float verticalOffset = 0.5f; // Cat de sus sa stea UI-ul fata de vent
    public Vector3 scaleOverride = new Vector3(0.005f, 0.005f, 0.005f); // Scara UI-ului (Canvas World Space e urias default)

    private GameObject interactUI;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        SetupUI();
    }

    void SetupUI()
    {
        // 1. Incarcam Prefab-ul din Resources
        GameObject prefab = Resources.Load<GameObject>("StealUI");

        if (prefab != null)
        {
            // 2. Il instantiem ca si copil al Vent-ului
            interactUI = Instantiate(prefab, transform);
            interactUI.name = "VentUI_Instance";

            // 3. Pozitionare si Scalare
            interactUI.transform.localPosition = new Vector3(0, verticalOffset, 0);
            interactUI.transform.localRotation = Quaternion.identity;

            // Folosim scara prefab-ului sau o fortam noi daca e nevoie
            // Daca prefabul tau e deja setat bine, poti folosi: interactUI.transform.localScale = prefab.transform.localScale;
            interactUI.transform.localScale = prefab.transform.localScale;

            // 4. Fortam Canvas-ul sa fie World Space (de siguranta)
            Canvas c = interactUI.GetComponent<Canvas>();
            if (c != null) c.renderMode = RenderMode.WorldSpace;

            // 5. CURATENIE: Ascundem Bara de Progres (Nu avem nevoie de ea la Vent)
            // Cautam imaginile de tip "Filled" (bara verde) si le ascundem
            Image[] images = interactUI.GetComponentsInChildren<Image>();
            foreach (Image img in images)
            {
                if (img.type == Image.Type.Filled)
                {
                    img.gameObject.SetActive(false); // Ascundem bara de incarcare
                }
            }

            // 6. Optional: Daca ai Text (TMP sau Legacy), poti sa il schimbi sa scrie "ENTER"
            // TMPro.TextMeshProUGUI txt = interactUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            // if(txt != null) txt.text = "OPEN MAP";

            // 7. La final, ascundem UI-ul (il aratam doar cand vine jucatorul)
            interactUI.SetActive(false);
        }
        else
        {
            Debug.LogError("Nu am gasit 'StealUI' in folderul Resources! Verifica numele.");
        }
    }

    void Update()
    {
        // Logica de Billboard (UI-ul se uita la camera)
        if (interactUI != null && interactUI.activeSelf && mainCamera != null)
        {
            interactUI.transform.rotation = mainCamera.transform.rotation;
            interactUI.transform.Rotate(0, 180, 0); // Rotim 180 grade ca sa nu fie oglindit
        }
    }

    public void ShowPrompt(bool show)
    {
        if (interactUI != null)
        {
            interactUI.SetActive(show);
        }
    }
}