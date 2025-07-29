using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    //[SerializeField] private float smoothSpeed = 5f;

    // Offset represents the camera's position relative to player in default isometric view
    [SerializeField] private Vector3 defaultOffset = new Vector3(0, 12, -8);
    [SerializeField] private Vector3 defaultRotation = new Vector3(45f, 30f, 0f); // X, Y, Z in degrees

    [SerializeField] private float rotationLimit = 30f; // ±30° from default Y rotation
    [SerializeField] private float rotationSpeed = 90f; // Degrees/sec
    [SerializeField] private float resetLerpTime = 0.2f; // Rubber band feel

    [Header("Transition")]
    [SerializeField] private Vector3 startOffset = new Vector3(0, 24, 0);   // Bird's eye starting offset (higher above)
    [SerializeField] private Vector3 startRotation = new Vector3(90f, 0f, 0f); // Top-down rotation
    [SerializeField] private float transitionDuration = 1.0f; // Seconds

    private Transform target;
    private float currentYawOffset = 0f;    // Offset from default Y rotation
    private float yawVelocity;              // For SmoothDamp
    private bool isTransitioning = true;
    private float transitionTimer = 0f;

    void Start()
    {
        // Start camera at bird's eye, transition to isometric
        transitionTimer = 0f;
        isTransitioning = true;
        ApplyTransform(startOffset, startRotation);
    }

    void LateUpdate()
    {
        var keyboard = Keyboard.current;

        // Start transition from bird's-eye to isometric at game start
        if (isTransitioning)
        {
            transitionTimer += Time.deltaTime / transitionDuration;
            float t = Mathf.Clamp01(transitionTimer);

            Vector3 playerPos = target != null ? target.position : Vector3.zero;
            Vector3 fromPos = playerPos + startOffset;
            Vector3 toPos = playerPos + defaultOffset;
            Vector3 lerpPos = Vector3.Lerp(fromPos, toPos, t);

            Quaternion fromRot = Quaternion.Euler(startRotation);
            Quaternion toRot = Quaternion.Euler(defaultRotation);
            Quaternion lerpRot = Quaternion.Lerp(fromRot, toRot, t);

            transform.position = lerpPos;
            transform.rotation = lerpRot;

            if (transitionTimer >= 1f)
            {
                isTransitioning = false;
                ApplyTransform(defaultOffset, defaultRotation);
                currentYawOffset = 0f;
            }
            return;
        }

        // Only allow limited camera rotation while Q/E held
        float yawDelta = 0f;
        if (keyboard != null)
        {
            if (keyboard.qKey.isPressed)
                yawDelta -= rotationSpeed * Time.deltaTime;
            if (keyboard.eKey.isPressed)
                yawDelta += rotationSpeed * Time.deltaTime;
        }

        // Update yaw offset (clamped) when key held, reset with rubber band when released
        if (Mathf.Abs(yawDelta) > 0.01f)
        {
            currentYawOffset = Mathf.Clamp(currentYawOffset + yawDelta, -rotationLimit, rotationLimit);
        }
        else
        {
            currentYawOffset = Mathf.SmoothDamp(currentYawOffset, 0f, ref yawVelocity, resetLerpTime);
        }

        // Determine camera's current position/rotation around the target
        Vector3 cameraTargetPos = target != null ? target.position : Vector3.zero;
        transform.position = cameraTargetPos + defaultOffset;
        transform.rotation = Quaternion.Euler(defaultRotation.x, defaultRotation.y + currentYawOffset, defaultRotation.z);
    }

    /// <summary>
    /// Assign the player to follow.
    /// </summary>
    public void SetTarget(Transform player)
    {
        target = player;
    }

    /// <summary>
    /// Utility to set camera transform based on offset and euler rotation, relative to target (if assigned).
    /// </summary>
    private void ApplyTransform(Vector3 offset, Vector3 eulerAngles)
    {
        Vector3 playerPos = target != null ? target.position : Vector3.zero;
        transform.position = playerPos + offset;
        transform.rotation = Quaternion.Euler(eulerAngles);
    }
}
