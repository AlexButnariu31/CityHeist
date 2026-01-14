using UnityEngine;

public class ForceFirstPersonZone : MonoBehaviour
{
    private ThirdPersonCamera cameraScript;

    void Start()
    {
        cameraScript = FindObjectOfType<ThirdPersonCamera>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (cameraScript != null)
            {
                // Activam logica inteligenta (Memorie + Blocare)
                cameraScript.EnterForcedZone();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (cameraScript != null)
            {
                // Dezactivam blocarea si revenim la starea veche
                cameraScript.ExitForcedZone();
            }
        }
    }
}