using UnityEngine;

public class VentSystem : MonoBehaviour
{
    [Header("Referinte")]
    public GameObject bigMapUI;
    public PlayerMove playerScript;
    public VentTransitionManager transitionManager; // <--- REFERINTA NOUA

    [Header("Setari Vent")]
    public float interactionRange = 3.0f;
    public LayerMask ventLayer;

    private bool isMapOpen = false;
    private VentObject currentVent;

    void Start()
    {
        if (bigMapUI != null) bigMapUI.SetActive(false);
    }

    void Update()
    {
        if (GameState.GameEnded) return;
        if (Meniu_Pause.IsPaused) return;
        DetectVent();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isMapOpen)
            {
                // Cand inchizi harta manual, nu mai punem video, doar o inchidem
                ToggleMap(false);
            }
            else if (currentVent != null)
            {
                // --- MODIFICARE: Pornim video inainte de harta ---
                if (transitionManager != null)
                {
                    // Oprim jucatorul cat timp ruleaza videoul
                    if (playerScript != null) playerScript.enabled = false;

                    // Ascundem prompt-ul "Press E"
                    if (currentVent != null) currentVent.ShowPrompt(false);

                    // Pornim animatia si ii spunem ce sa faca la final (sa deschida harta)
                    transitionManager.PlayEnterAnimation(() =>
                    {
                        ToggleMap(true);
                    });
                }
                else
                {
                    // Fallback daca ai uitat sa pui managerul
                    ToggleMap(true);
                }
            }
        }
    }

    // ... Restul functiei DetectVent ramane neschimbata ...
    void DetectVent()
    {
        if (isMapOpen) return;
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange, ventLayer);
        if (hits.Length > 0)
        {
            VentObject foundVent = hits[0].GetComponent<VentObject>();
            if (foundVent != null && foundVent != currentVent)
            {
                if (currentVent != null) currentVent.ShowPrompt(false);
                currentVent = foundVent;
                currentVent.ShowPrompt(true);
            }
        }
        else
        {
            if (currentVent != null)
            {
                currentVent.ShowPrompt(false);
                currentVent = null;
            }
        }
    }

    public void ToggleMap(bool state)
    {
        isMapOpen = state;
        if (bigMapUI != null) bigMapUI.SetActive(state);

        if (state)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (playerScript != null) playerScript.enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (playerScript != null) playerScript.enabled = true;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}