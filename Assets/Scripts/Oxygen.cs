using UnityEngine;
using UnityEngine.Events;

public class Oxygen : MonoBehaviour
{
    [Header("Oxygen Settings")]
    public float startTime = 180f; // seconds
    public bool countDown = true;
    public UnityEvent onTimerEnd;

    private float currentTime;
    private bool isRunning = false;

    public float CurrentTime => currentTime;

    void Start()
    {
        ResetTimer();
        StartTimer();
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime += (countDown ? -1 : 1) * Time.deltaTime;

        if (countDown && currentTime <= 0)
        {
            currentTime = 0;
            isRunning = false;
            onTimerEnd?.Invoke();
        }

        // You can use a UI Manager to show currentTime on screen
        UIManager.Instance.UpdateOxygenUI(currentTime);
    }

    public void StartTimer() => isRunning = true;
    public void StopTimer() => isRunning = false;
    public void ResetTimer() => currentTime = countDown ? startTime : 0f;
}
