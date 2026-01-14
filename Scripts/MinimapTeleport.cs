using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MinimapTeleport : MonoBehaviour, IPointerDownHandler
{
    [Header("Referinte")]
    public Camera mapCamera;
    public Transform player;
    public RawImage minimapImage;
    public VentSystem ventSystem;
    public VentTransitionManager transitionManager; // Video

    // --- NOU: Referinta la tranzitie ---
    public SceneTransition sceneTransition;
    // -----------------------------------

    [Header("Setari")]
    public float teleportOffset = 0.1f;
    public LayerMask targetLayer;
    public LayerMask groundLayer;

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            float xPct = (localPoint.x - rectTransform.rect.x) / rectTransform.rect.width;
            float yPct = (localPoint.y - rectTransform.rect.y) / rectTransform.rect.height;

            if (xPct < 0 || xPct > 1 || yPct < 0 || yPct > 1) return;

            Ray mapRay = mapCamera.ViewportPointToRay(new Vector3(xPct, yPct, 0));
            RaycastHit hit;

            if (Physics.Raycast(mapRay, out hit, Mathf.Infinity, targetLayer))
            {
                if (hit.collider.CompareTag("TeleportPoint"))
                {
                    Vector3 targetPos = hit.transform.position;

                    // Logica de sol
                    Vector3 rayStartPoint = targetPos - new Vector3(0, 1.0f, 0);
                    RaycastHit groundHit;
                    if (Physics.Raycast(rayStartPoint, Vector3.down, out groundHit, 50f, groundLayer))
                    {
                        targetPos = groundHit.point;
                    }

                    // Pornim secventa
                    StartTeleportSequence(targetPos);
                }
            }
        }
    }

    void StartTeleportSequence(Vector3 targetPosition)
    {
        // 1. Inchidem harta imediat
        if (ventSystem != null) ventSystem.ToggleMap(false);

        // Functia care executa efectiv teleportarea si tranzitia vizuala
        System.Action doVisualTransition = () =>
        {
            if (sceneTransition != null)
            {
                // Pasul A: Inchidem cercul (Ecran negru)
                sceneTransition.CloseCircle(() =>
                {
                    // Pasul B: Mutam jucatorul cat timp e negru
                    PerformTeleport(targetPosition);

                    // Pasul C: Deschidem cercul la loc
                    sceneTransition.OpenCircle(null);
                });
            }
            else
            {
                // Daca nu avem tranzitie, teleportam direct
                PerformTeleport(targetPosition);
            }
        };

        // 2. Verificam daca avem VIDEO inainte de tranzitie
        if (transitionManager != null)
        {
            PlayerMove moveScript = player.GetComponent<PlayerMove>();
            if (moveScript) moveScript.enabled = false;

            transitionManager.PlayExitAnimation(() =>
            {
                // Dupa video, facem tranzitia circulara
                doVisualTransition.Invoke();
            });
        }
        else
        {
            // Fara video, facem direct tranzitia
            doVisualTransition.Invoke();
        }
    }

    void PerformTeleport(Vector3 targetPosition)
    {
        PlayerMove moveScript = player.GetComponent<PlayerMove>();

        if (moveScript != null)
        {
            Vector3 finalPos = new Vector3(targetPosition.x, targetPosition.y + teleportOffset, targetPosition.z);
            moveScript.TeleportToPosition(finalPos);
            moveScript.enabled = true;
        }
    }
}