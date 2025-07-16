using UnityEngine;
using UnityEngine.InputSystem;

// Define the enum outside the class, but within the namespace (if you have one)
public enum FiringMode
{
    SemiAutomatic,
    Automatic,
    Burst
}

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Combat Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float baseFireRate = 0.2f; // Time between individual shots (for auto/burst)
    [SerializeField] private FiringMode currentFiringMode = FiringMode.Automatic; // Default mode

    [Header("Burst Fire Settings")]
    [SerializeField] private int burstAmount = 3; // Number of bullets in a burst
    [SerializeField] private float burstInterval = 0.05f; // Time between bullets in a burst
    [SerializeField] private float burstCooldown = 0.5f; // Cooldown after a full burst

    private PlayerControls playerControls;
    private Vector2 movementInput;
    private Rigidbody rb;
    private Camera mainCamera;

    private float nextAvailableShotTime; // When the next individual shot can happen
    private bool canFireSemiAuto = true; // For semi-auto: true if button was released
    private bool isFiringInputHeld = false; // To track if fire button is held down
    private int bulletsFiredInBurst; // Counter for burst mode
    private float burstEndTime; // When burst cooldown finishes

    private void Awake()
    {
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("PlayerController: Main camera not found. Please tag your main camera as 'MainCamera'.");
        }

        playerControls.Player.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        playerControls.Player.Move.canceled += ctx => movementInput = Vector2.zero;

        // Subscribe to Fire actions
        playerControls.Player.Shoot.performed += ctx => OnFirePerformed();
        playerControls.Player.Shoot.canceled += ctx => OnFireCanceled();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void Update() // Changed from FixedUpdate to Update for non-physics related checks (like input holding)
    {
        HandleAiming();

        // Handle Automatic fire here, as it needs to check input state continuously
        if (currentFiringMode == FiringMode.Automatic && isFiringInputHeld)
        {
            TryFire();
        }
    }

    private void OnFirePerformed()
    {
        isFiringInputHeld = true; // Input is now held down

        switch (currentFiringMode)
        {
            case FiringMode.SemiAutomatic:
                if (canFireSemiAuto)
                {
                    TryFire();
                    canFireSemiAuto = false; // Prevent firing again until button is released
                }
                break;
            case FiringMode.Burst:
                if (Time.time >= nextAvailableShotTime) // Only start burst if cooldown is over
                {
                    StartBurstFire();
                }
                break;
                // Automatic is handled in Update()
        }
    }

    private void OnFireCanceled()
    {
        isFiringInputHeld = false; // Input is no longer held
        canFireSemiAuto = true; // Allow semi-auto to fire again on next press
    }

    // This method tries to fire a single projectile based on baseFireRate
    private void TryFire()
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("Projectile Prefab or Fire Point not assigned in PlayerController!");
            return;
        }

        if (Time.time >= nextAvailableShotTime)
        {
            FireProjectile();
            nextAvailableShotTime = Time.time + baseFireRate; // Set cooldown for next individual shot
        }
    }

    private void FireProjectile()
    {
        // Get the direction the player is currently facing
        Vector3 fireDirection = transform.forward;

        // Instantiate the projectile
        GameObject bulletGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Initialize(fireDirection);
        }
        else
        {
            Debug.LogError("Projectile prefab does not have a Projectile script attached!");
        }
    }

    private void StartBurstFire()
    {
        bulletsFiredInBurst = 0;
        InvokeRepeating(nameof(FireBurstBullet), 0f, burstInterval); // Start firing burst bullets
        // Set nextAvailableShotTime to prevent new bursts until cooldown is over
        nextAvailableShotTime = Time.time + burstCooldown; // This is the cooldown for the *next* burst
    }

    private void FireBurstBullet()
    {
        if (bulletsFiredInBurst < burstAmount)
        {
            FireProjectile();
            bulletsFiredInBurst++;
        }
        else
        {
            CancelInvoke(nameof(FireBurstBullet)); // Stop the burst
        }
    }

    private void HandleMovement()
    {
        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        Vector3 cameraRight = mainCamera.transform.right;
        cameraRight.y = 0;
        cameraRight.Normalize();

        Vector3 moveDirection = (cameraForward * movementInput.y + cameraRight * movementInput.x).normalized;

        if (moveDirection != Vector3.zero)
        {
            rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    private void HandleAiming()
    {
        if (Mouse.current != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane groundPlane = new Plane(Vector3.up, transform.position);

            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                Vector3 directionToLook = hitPoint - transform.position;
                directionToLook.y = 0;

                if (directionToLook != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToLook);
                    transform.rotation = targetRotation; // Instant rotation for exact aiming
                }
            }
        }
    }
}