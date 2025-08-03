using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 90f; // Degrees per second

    private void Update()
    {
        // Rotate around Y-axis
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Add 1 to collectible count
            GameManager.Instance.parts++;

            // Update the UI
            UIManager.Instance.UpdatePartsCount();

            // Destroy the collectible
            Destroy(gameObject);
        }
    }
}
