using UnityEngine;

public class SimpleRotate : MonoBehaviour
{
    [Tooltip("Viteza de rotatie")]
    public Vector3 rotationSpeed = new Vector3(0, 100f, 0);

    void Update()
    {
        float rotateX = rotationSpeed.x * Time.deltaTime;
        float rotateY = rotationSpeed.y * Time.deltaTime;
        float rotateZ = rotationSpeed.z * Time.deltaTime;

        transform.Rotate(rotateX, rotateY, rotateZ);
    }
}