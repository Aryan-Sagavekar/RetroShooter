using UnityEngine;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("?? Reached Goal!");
            // TODO: Show win screen or load next level
            UIManager.Instance.ShowWinScreen(); // or any GameManager method
        }
    }
}
