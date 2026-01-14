using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSteal : MonoBehaviour
{
    [Header("Setari Detectie")]
    public float interactionRange = 3.0f;
    public LayerMask stealableLayer; // <--- ASIGURA-TE CA AI BIFAT LAYER-UL CORECT AICI!
    public Transform cameraTransform; // Nu mai e strict necesar la OverlapSphere, dar il pastram

    private StealableObject currentTarget;
    private float holdTimer = 0f;

    void Update()
    {
        DetectObject();
        HandleInput();
    }

    void DetectObject()
    {
        // 1. FOLOSIM O SFERA (Arie) in loc de Raza
        // Asta gaseste tot ce e in jurul tau, indiferent daca te uiti fix la el sau nu.
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange, stealableLayer);

        StealableObject bestCandidate = null;
        float closestDistance = float.MaxValue;

        // 2. Cautam cel mai apropiat obiect din cele gasite
        foreach (Collider hit in hits)
        {
            // Cautam scriptul pe obiect sau pe parintii lui
            StealableObject obj = hit.GetComponent<StealableObject>();
            if (obj == null) obj = hit.GetComponentInParent<StealableObject>();

            // Daca e valid si nu a fost deja furat
            if (obj != null && !obj.IsStolen())
            {
                float distance = Vector3.Distance(transform.position, obj.transform.position);

                // Algoritmul de "Cel mai apropiat vecin"
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestCandidate = obj;
                }
            }
        }

        // --- 3. Logica de Schimbare a Tintei ---

        // Daca am gasit un candidat nou (ne-am apropiat de ceva)
        if (bestCandidate != null)
        {
            // Daca aveam altceva selectat inainte
            if (currentTarget != null && currentTarget != bestCandidate)
            {
                currentTarget.ShowUI(false);
                currentTarget.UpdateProgress(0);
                currentTarget.StopHoldSound(); // Oprim sunetul celui vechi
                holdTimer = 0;
            }

            currentTarget = bestCandidate;
            currentTarget.ShowUI(true);
        }
        // Daca nu mai e nimic in zona, dar noi inca aveam ceva selectat
        else if (currentTarget != null)
        {
            currentTarget.ShowUI(false);
            currentTarget.UpdateProgress(0);
            currentTarget.StopHoldSound(); // Oprim sunetul cand ne indepartam
            currentTarget = null;
            holdTimer = 0;
        }
    }

    void HandleInput()
    {
        // Daca nu avem nicio tinta valida, nu avem ce fura
        if (currentTarget == null) return;

        // Verificam tasta E
        if (Keyboard.current != null && Keyboard.current.eKey.isPressed)
        {
            // Crestem timpul
            holdTimer += Time.deltaTime;

            // --- SUNET ---
            currentTarget.StartHoldSound(); // Pornim sunetul de scotocit
            // -------------

            // Calculam procentul
            float percentage = holdTimer / currentTarget.timeToSteal;
            currentTarget.UpdateProgress(percentage);

            // Daca s-a umplut bara
            if (holdTimer >= currentTarget.timeToSteal)
            {
                currentTarget.Steal(); // Executam furtul
                currentTarget = null;  // Uitam de obiect
                holdTimer = 0;
            }
        }
        else
        {
            // Daca am luat degetul de pe E, resetam totul
            if (holdTimer > 0)
            {
                holdTimer = 0;
                currentTarget.UpdateProgress(0);

                // --- SUNET ---
                currentTarget.StopHoldSound(); // Oprim sunetul
                // -------------
            }
        }
    }

    // --- DEBUG ---
    // Deseneaza sfera galbena in fereastra Scene ca sa vezi raza de actiune
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 0, 0.3f); // Galben transparent
        Gizmos.DrawSphere(transform.position, interactionRange);
    }
}