using UnityEngine;
using UnityEngine.InputSystem;

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

    private float nextAvailableShotTime;
    private bool canFireSemiAuto = true;
    private bool isFiringInputHeld = false;
    private int bulletsFiredInBurst;

    private void Awake()
    {
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            UnityEngine.Debug.LogError("PlayerController: Main camera not found. Please tag your main camera as 'MainCamera'.");
        }

        playerControls.Player.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        playerControls.Player.Move.canceled += ctx => movementInput = Vector2.zero;

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

    private void Update()
    {
        HandleAiming();

        if (currentFiringMode == FiringMode.Automatic && isFiringInputHeld)
        {
            TryFire();
        }
    }

    private void OnFirePerformed()
    {
        isFiringInputHeld = true;
        switch (currentFiringMode)
        {
            case FiringMode.SemiAutomatic:
                if (canFireSemiAuto)
                {
                    TryFire();
                    canFireSemiAuto = false;
                }
                break;
            case FiringMode.Burst:
                if (Time.time >= nextAvailableShotTime)
                {
                    StartBurstFire();
                }
                break;
                // Automatic handled in Update()
        }
    }

    private void OnFireCanceled()
    {
        isFiringInputHeld = false;
        canFireSemiAuto = true;
    }

    private void TryFire()
    {
        if (projectilePrefab == null || firePoint == null)
        {
            UnityEngine.Debug.LogWarning("Projectile Prefab or Fire Point not assigned in PlayerController!");
            return;
        }

        if (Time.time >= nextAvailableShotTime)
        {
            FireProjectile();
            nextAvailableShotTime = Time.time + baseFireRate;
        }
    }

    private void FireProjectile()
    {
        Vector3 fireDirection = transform.forward;

        GameObject bulletGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet != null)
            bullet.Initialize(fireDirection);
        else
            UnityEngine.Debug.LogError("Projectile prefab does not have a Bullet script attached!");
    }

    private void StartBurstFire()
    {
        bulletsFiredInBurst = 0;
        InvokeRepeating(nameof(FireBurstBullet), 0f, burstInterval);
        nextAvailableShotTime = Time.time + burstCooldown;
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
            CancelInvoke(nameof(FireBurstBullet));
        }
    }

    private void HandleMovement()
    {
        // Local (relative to player) movement control
        Vector3 moveDirection = new Vector3(movementInput.x, 0, movementInput.y);

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            moveDirection = moveDirection.normalized;
            Vector3 worldMoveDirection = transform.TransformDirection(moveDirection);
            rb.linearVelocity = new Vector3(worldMoveDirection.x * moveSpeed, rb.linearVelocity.y, worldMoveDirection.z * moveSpeed);
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    private void HandleAiming()
    {
        if (Mouse.current == null || mainCamera == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 directionToLook = hitPoint - transform.position;
            directionToLook.y = 0;

            if (directionToLook.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToLook);
                transform.rotation = targetRotation;
            }
        }
    }
}
