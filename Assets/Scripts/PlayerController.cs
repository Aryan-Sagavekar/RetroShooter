using System.Collections;
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

    [Header("Weapon and Combat Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float baseFireRate = 0.2f;
    [SerializeField] private int magazineSize = 30;
    [SerializeField] private int reserveSize = 120;
    [SerializeField] private FiringMode currentFiringMode = FiringMode.Automatic; // Default mode
    [SerializeField] private float reloadTime = 2f;
    [SerializeField] private Transform weaponPosition;


    [Header("Burst Fire Settings")]
    [SerializeField] private int burstAmount = 3; // Number of bullets in a burst
    [SerializeField] private float burstInterval = 0.05f; // Time between bullets in a burst
    [SerializeField] private float burstCooldown = 0.5f; // Cooldown after a full burst

    // Player controls
    private PlayerControls playerControls;
    private Vector2 movementInput;
    private Rigidbody rb;
    private Camera mainCamera;

    // Weapon
    private bool isReloading = false;
    private int currentAmmo;
    private int currentReserveAmmo;
    private bool isFiringInputHeld = false;
    private int bulletsFiredInBurst;
    private float nextAvailableShotTime;
    private bool canFireSemiAuto = true;
    private Health playerHealth;


    private void Awake()
    {
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody>();
        playerHealth = GetComponent<Health>();
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            UnityEngine.Debug.LogError("PlayerController: Main camera not found. Please tag your main camera as 'MainCamera'.");
        }

        playerControls.Player.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        playerControls.Player.Move.canceled += ctx => movementInput = Vector2.zero;

        playerControls.Player.Shoot.performed += ctx => OnFirePerformed();
        playerControls.Player.Shoot.canceled += ctx => OnFireCanceled();

        playerControls.Player.Reload.performed += ctx => OnReloadPerformed();

        currentAmmo = magazineSize;
        currentReserveAmmo = reserveSize;

        UIManager.Instance.UpdateAmmoUI(currentAmmo, currentReserveAmmo);
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

        if (currentFiringMode == FiringMode.Automatic && isFiringInputHeld && currentAmmo > 0 && !isReloading)
        {
            TryFire();
        }

        UIManager.Instance.UpdateAmmoUI(currentAmmo, currentReserveAmmo);
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
        if (isReloading || currentAmmo <= 0)
        {
            if (!isReloading && currentReserveAmmo > 0)
            {
                StartCoroutine(Reload());
            }
            return;
        }

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
        if (currentAmmo <= 0) return;

        Vector3 fireDirection = transform.forward;

        GameObject bulletGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Bullet bullet = bulletGO.GetComponent<Bullet>();

        if (bullet != null)
            bullet.Initialize(fireDirection);
        else
            Debug.LogError("Projectile prefab does not have a Bullet script attached!");

        currentAmmo--;

        if (currentAmmo == 0 && currentReserveAmmo > 0 && !isReloading)
        {
            StartCoroutine(Reload());
        }

        UIManager.Instance.UpdateAmmoUI(currentAmmo, currentReserveAmmo);
    }


    private void StartBurstFire()
    {
        if (isReloading || currentAmmo <= 0) return;

        bulletsFiredInBurst = 0;
        int actualBurstAmount = Mathf.Min(burstAmount, currentAmmo); // Handle low ammo in burst
        StartCoroutine(FireBurst(actualBurstAmount));
        nextAvailableShotTime = Time.time + burstCooldown;
    }

    private IEnumerator FireBurst(int shots)
    {
        while (bulletsFiredInBurst < shots)
        {
            if (currentAmmo <= 0) break;

            FireProjectile();
            bulletsFiredInBurst++;

            yield return new WaitForSeconds(burstInterval);
        }
    }

    private void OnReloadPerformed()
    {
        if (!isReloading && currentAmmo < magazineSize && currentReserveAmmo > 0)
        {
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        //canFire = false;
        isFiringInputHeld = false;
        Debug.Log("Reloading...");
        UIManager.Instance.ShowReloading(); // Update UI to show reloading status

        //TODO: Add reloading sound
        //if (currentWeaponData.reloadSound != null)
        //{
        //    AudioSource.PlayClipAtPoint(currentWeaponData.reloadSound, firePoint.position);
        //}

        yield return new WaitForSeconds(reloadTime);

        int bulletsNeeded = magazineSize - currentAmmo;
        int bulletsToLoad = Mathf.Min(bulletsNeeded, currentReserveAmmo);

        currentAmmo += bulletsToLoad;
        if (currentReserveAmmo != int.MaxValue) // Don't subtract if infinite
        {
            currentReserveAmmo -= bulletsToLoad;
        }

        isReloading = false;
        //canFire = true;
        Debug.Log("Reload complete!");
        UIManager.Instance.UpdateAmmoUI(currentAmmo, currentReserveAmmo);
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

    private void OnTriggerEnter(Collider hitbox)
    {
        if (hitbox.gameObject.CompareTag("EnemyHitbox"))
        {
            Bullet check = hitbox.gameObject.GetComponent<Bullet>();
            Enemy e = check != null ? check.Shooter.GetComponentInParent<Enemy>() : hitbox.gameObject.GetComponentInParent<Enemy>();            if (check != null)
            {
                Destroy(hitbox.gameObject);
            }
            playerHealth.TakeDamage(e.AttackDamage);
        }
    }
}
