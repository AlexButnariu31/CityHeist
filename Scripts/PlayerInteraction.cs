using UnityEngine;
using System.Linq; // <--- NECESAR pentru a sorta obiectele dupa distanta

public class PlayerInteraction : MonoBehaviour
{
    [Header("Referinte")]
    public Animator animator;
    public Transform detectionPoint;
    public PlayerMove playerMoveScript;

    [Header("Setari Detectie")]
    public float interactionRadius = 1.5f;
    public LayerMask stealableLayer;

    [Header("Setari Animatie")]
    public float baseAnimationDuration = 2.0f;

    // Variabile interne
    private StealableObject currentTarget;
    private float currentStealTimer = 0f;
    private bool isHolding = false;

    // --- VARIABILA NOUA DE PROTECTIE ---
    private bool actionCompletedThisPress = false;

    void Update()
    {
        if (GameState.GameEnded) return;
        // 1. Verificam daca tinem apasat E
        if (Input.GetKey(KeyCode.E))
        {
            // Daca deja am terminat o actiune la aceasta apasare, nu mai facem nimic
            // pana cand jucatorul nu ridica degetul.
            if (!actionCompletedThisPress)
            {
                ProcessStealing();
            }
        }
        else
        {
            // 2. Cand dam drumul la tasta, resetam protectia
            actionCompletedThisPress = false;

            if (isHolding)
            {
                StopStealing();
            }
        }
    }

    void ProcessStealing()
    {
        if (currentTarget == null)
        {
            FindTarget();
        }

        if (currentTarget == null) return;

        isHolding = true;

        if (playerMoveScript != null) playerMoveScript.enabled = false;

        animator.SetBool("IsStealing", true);

        float speedMultiplier = 1f;
        if (currentTarget.timeToSteal > 0)
        {
            speedMultiplier = baseAnimationDuration / currentTarget.timeToSteal;
        }
        animator.SetFloat("PickupSpeed", speedMultiplier);

        currentTarget.ShowUI(true);
        currentTarget.StartHoldSound();

        currentStealTimer += Time.deltaTime;
        float progress = currentStealTimer / currentTarget.timeToSteal;
        currentTarget.UpdateProgress(progress);

        if (currentStealTimer >= currentTarget.timeToSteal)
        {
            CompleteSteal();
        }
    }

    void StopStealing()
    {
        isHolding = false;
        currentStealTimer = 0f;

        animator.SetBool("IsStealing", false);
        animator.SetFloat("PickupSpeed", 1f);

        if (playerMoveScript != null) playerMoveScript.enabled = true;

        if (currentTarget != null)
        {
            currentTarget.ShowUI(false);
            currentTarget.StopHoldSound();
            currentTarget = null;
        }
    }

    void CompleteSteal()
    {
        if (currentTarget != null)
        {
            currentTarget.Steal();
            currentTarget = null;
        }

        // --- ACTIVAM PROTECTIA ---
        // Ii spunem jocului: "Gata, am furat ceva la tura asta. 
        // Nu mai accepta nimic pana nu se reseteaza tasta."
        actionCompletedThisPress = true;

        StopStealing();
    }

    void FindTarget()
    {
        Vector3 center = detectionPoint ? detectionPoint.position : transform.position;
        Collider[] hitColliders = Physics.OverlapSphere(center, interactionRadius, stealableLayer);

        if (hitColliders.Length > 0)
        {
            // --- IMBUNATATIRE: Sortam obiectele dupa distanta ---
            // Asta previne situatia in care furi un obiect din spate in loc de cel din fata

            // Gasim cel mai apropiat collider
            Collider closest = hitColliders
                .OrderBy(c => Vector3.Distance(center, c.transform.position))
                .First();

            currentTarget = closest.GetComponent<StealableObject>();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(detectionPoint ? detectionPoint.position : transform.position, interactionRadius);
    }
}