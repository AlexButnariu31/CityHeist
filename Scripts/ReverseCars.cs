    using UnityEngine;

    public class ReverseCars : MonoBehaviour
    {
        public float baseSpeed = 5f;
        public float maxSpeed = 10f;
        public float detectionDistance = 10f;
        public float xStart = -254.02f;
        public float xEnd = 16.61f;

        private float currentSpeed;
        private Vector3 startPosition;
        private Vector3 endPosition;

        void Start()
        {
            if (tag != "Car") tag = "Car";

            float y = transform.position.y;
            float z = transform.position.z;
            startPosition = new Vector3(xEnd, y, z);
            endPosition = new Vector3(xStart, y, z);

            currentSpeed = baseSpeed + Random.Range(-2f, 2f);
        }

        void Update()
        {
            bool carInFront = false;
            RaycastHit hit;

            if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, detectionDistance))
                if (hit.collider.CompareTag("Car") && hit.collider.gameObject != gameObject)
                    carInFront = true;

            if (carInFront)
                currentSpeed = Mathf.Lerp(currentSpeed, baseSpeed * 0.3f, Time.deltaTime * 2f);
            else
                currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, Time.deltaTime * 1.5f);

            transform.position = Vector3.MoveTowards(transform.position, endPosition, currentSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, endPosition) < 0.1f)
        {

            Vector3 offset = new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));
            transform.position = startPosition + offset;

            currentSpeed = baseSpeed + Random.Range(-2f, 2f);
        }
    }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position + Vector3.up, transform.forward * detectionDistance);
        }
    }
