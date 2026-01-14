using UnityEngine;
using UnityEngine.InputSystem;

public class DoorController : MonoBehaviour
{
    [Header("Componente")]
    public Transform doorHinge; // AICI tragi obiectul "Hinge" (balamaua)
    public GameObject interactText; // Textul "Apasa E"

    [Header("Setari Usa")]
    public float openAngle = 90f;
    public float speed = 2f;

    private bool isOpen = false;
    private bool isPlayerNearby = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        if (doorHinge == null)
        {
            Debug.LogError("Nu ai pus obiectul Hinge in script!");
            return;
        }

        // Retinem rotatia initiala a BALAMALEI, nu a scriptului
        closedRotation = doorHinge.localRotation;
        openRotation = Quaternion.Euler(0, openAngle, 0) * closedRotation;

        if (interactText != null) interactText.SetActive(false);
    }

    void Update()
    {
        if (isPlayerNearby && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            isOpen = !isOpen;
        }

        // Rotim Hinge-ul, nu pe noi
        if (doorHinge != null)
        {
            Quaternion targetRotation = isOpen ? openRotation : closedRotation;
            doorHinge.localRotation = Quaternion.Slerp(doorHinge.localRotation, targetRotation, Time.deltaTime * speed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (interactText != null) interactText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (interactText != null) interactText.SetActive(false);
        }
    }
}