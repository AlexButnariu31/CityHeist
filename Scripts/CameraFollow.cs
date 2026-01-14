using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Camera Distance")]
    [SerializeField] private float distance = 10f;
    [SerializeField] private float height = 5f;

    [Header("First Person Settings")]
    [SerializeField] private Vector3 firstPersonOffset = new Vector3(0f, 1.6f, 0.2f); // Unde stau ochii
    [SerializeField] private Renderer[] playerRenderers; // Mesh-urile pentru ascuns

    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;

    [Header("Smoothing")]
    [SerializeField] private float rotationSmoothSpeed = 10f;
    [SerializeField] private float positionSmoothSpeed = 10f;

    private float cameraRotationX = 0f;
    private float cameraRotationY = 0f;

    // --- VARIABILE DE STARE ---
    private bool isFirstPerson = false;         // Starea curenta
    private bool isZoneLocked = false;          // Este adevarat cand suntem intr-o zona fortata (tunel)
    private bool wasFirstPersonBeforeZone;      // Memoria: ce eram inainte sa intram in zona?

    void Start()
    {
        if (player == null)
        {
            PlayerMove playerScript = FindObjectOfType<PlayerMove>();
            if (playerScript != null) player = playerScript.transform;
            else { enabled = false; return; }
        }

        if (playerRenderers == null || playerRenderers.Length == 0)
        {
            playerRenderers = player.GetComponentsInChildren<Renderer>();
        }

        Vector3 angles = transform.eulerAngles;
        cameraRotationX = angles.y;
        cameraRotationY = angles.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Meniu_Pause.IsPaused) return;
        if (Mouse.current == null) return;

        // 1. Input Mouse
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        cameraRotationX += mouseDelta.x * mouseSensitivity * Time.deltaTime * 10f;
        cameraRotationY -= mouseDelta.y * mouseSensitivity * Time.deltaTime * 10f;
        cameraRotationY = Mathf.Clamp(cameraRotationY, minVerticalAngle, maxVerticalAngle);

        if (Keyboard.current != null)
        {
            // 2. Lock/Unlock cursor
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            if (!Meniu_Pause.IsPaused && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            // 3. SCHIMBARE CAMERA (V)
            if (Keyboard.current.vKey.wasPressedThisFrame)
            {
                // Schimbam camera DOAR daca NU suntem blocati de o zona
                if (!isZoneLocked)
                {
                    ToggleViewMode();
                }
            }
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        Quaternion rotation = Quaternion.Euler(cameraRotationY, cameraRotationX, 0);

        if (isFirstPerson)
        {
            // --- FIRST PERSON ---
            Vector3 eyePos = player.position + firstPersonOffset;
            transform.position = eyePos;
            transform.rotation = rotation;
        }
        else
        {
            // --- THIRD PERSON ---
            Vector3 desiredOffset = rotation * new Vector3(0f, height, -distance);
            Vector3 desiredPosition = player.position + desiredOffset;
            Vector3 playerHead = player.position + Vector3.up * height * 0.5f;

            RaycastHit hit;
            float sphereRadius = 0.5f;
            Vector3 direction = desiredPosition - playerHead;
            float desiredDistance = direction.magnitude;

            int layerMask = ~LayerMask.GetMask("Player", "Ignore Raycast");

            if (Physics.SphereCast(playerHead, sphereRadius, direction.normalized, out hit, desiredDistance, layerMask))
            {
                desiredPosition = hit.point - direction.normalized * 0.3f;
            }

            transform.position = Vector3.Lerp(transform.position, desiredPosition, positionSmoothSpeed * Time.deltaTime);
            transform.LookAt(player.position + Vector3.up * height * 0.5f);
        }
    }

    // --- FUNCTII INTERNE ---

    void ToggleViewMode()
    {
        // Schimbam starea
        SetFirstPerson(!isFirstPerson);
    }

    // Functie helper care seteaza starea si vizibilitatea corpului
    public void SetFirstPerson(bool active)
    {
        isFirstPerson = active;

        if (playerRenderers != null)
        {
            foreach (var renderer in playerRenderers)
            {
                renderer.enabled = !isFirstPerson; // Ascundem corpul in First Person
            }
        }
    }

    // --- FUNCTII PENTRU ZONA (TRIGGER) ---

    // Apelata cand INTRI in zona
    public void EnterForcedZone()
    {
        // 1. Tinem minte cum erai inainte
        wasFirstPersonBeforeZone = isFirstPerson;

        // 2. Blocam tasta V
        isZoneLocked = true;

        // 3. Fortam First Person
        SetFirstPerson(true);
    }

    // Apelata cand IESI din zona
    public void ExitForcedZone()
    {
        // 1. Deblocam tasta V
        isZoneLocked = false;

        // 2. Revenim la starea memorata
        SetFirstPerson(wasFirstPersonBeforeZone);
    }

    // --- GETTERS ---
    public Vector3 GetCameraForward()
    {
        Vector3 forward = transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    public Vector3 GetCameraRight()
    {
        Vector3 right = transform.right;
        right.y = 0;
        return right.normalized;
    }
}