using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleMove : MonoBehaviour
{
    [Header("Setari Miscare")]
    public float moveSpeed = 8.0f;

    [Header("Setari Camera")]
    public Transform cameraTransform;
    public float cameraDistance = 10f;
    public float mouseSensitivity = 2f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private float cameraRotationX = 0f;
    private float cameraRotationY = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("[SimpleMove] ERROR: Rigidbody lipsa!", this);
        }
        else
        {
            rb.freezeRotation = true;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        CapsuleCollider cap = GetComponent<CapsuleCollider>();
        if (cap == null)
            Debug.LogError("[SimpleMove] ERROR: CapsuleCollider lipsa!", this);
        else if (cap.isTrigger)
            Debug.LogWarning("[SimpleMove] WARNING: CapsuleCollider este trigger!", this);

        if (cameraTransform == null)
        {
            if (Camera.main == null)
            {
                Debug.LogError("[SimpleMove] ERROR: Nu exista Camera.main in scena!", this);
            }
            else
            {
                cameraTransform = Camera.main.transform;
                Debug.Log("[SimpleMove] INFO: Camera detectată automat.");
            }
        }

        if (cameraTransform == null)
        {
            Debug.LogError("[SimpleMove] ERROR: cameraTransform este NULL! Scriptul este activ dar nu avem camera!", this);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform != null)
        {
            Vector3 ang = cameraTransform.eulerAngles;
            cameraRotationX = ang.y;
            cameraRotationY = ang.x;
        }
    }

    void Update()
    {
        if (Keyboard.current == null)
        {
            Debug.LogWarning("[SimpleMove] WARNING: Keyboard.current este NULL!");
            return;
        }

        moveInput = Vector2.zero;

        if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
        if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
        if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;
        if (Keyboard.current.dKey.isPressed) moveInput.x += 1;

        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            if (float.IsNaN(mouseDelta.x) || float.IsNaN(mouseDelta.y))
                Debug.LogWarning("[SimpleMove] WARNING: MouseDelta este NaN!");

            cameraRotationX += mouseDelta.x * mouseSensitivity * Time.deltaTime * 10f;
            cameraRotationY -= mouseDelta.y * mouseSensitivity * Time.deltaTime * 10f;

            cameraRotationY = Mathf.Clamp(cameraRotationY, minVerticalAngle, maxVerticalAngle);
        }

  
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("[SimpleMove] INFO: Cursor unlocked.");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("[SimpleMove] INFO: Cursor locked.");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void FixedUpdate()
    {
        if (rb == null || cameraTransform == null)
        {
            Debug.LogError("[SimpleMove] ERROR: rb sau cameraTransform NULL in FixedUpdate!", this);
            return;
        }

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 movement = (forward * moveInput.y + right * moveInput.x);
        Vector3 newVelocity = movement.normalized * moveSpeed;

        if (float.IsNaN(newVelocity.x) || float.IsNaN(newVelocity.z))
        {
            Debug.LogError("[SimpleMove] ERROR: newVelocity a devenit NaN! Something broke!", this);
            return;
        }

        newVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = newVelocity;
    }

    void LateUpdate()
    {
        if (cameraTransform == null)
        {
            Debug.LogError("[SimpleMove] ERROR: cameraTransform NULL în LateUpdate!", this);
            return;
        }

        Quaternion rot = Quaternion.Euler(cameraRotationY, cameraRotationX, 0);
        Vector3 offset = rot * new Vector3(0f, 0f, -cameraDistance);

        if (offset.magnitude > 10000)
        {
            Debug.LogError("[SimpleMove] ERROR: Camera offset gigantic! Something broke badly!", this);
            return;
        }

        cameraTransform.position = transform.position + offset;
        cameraTransform.LookAt(transform.position);
    }
}
