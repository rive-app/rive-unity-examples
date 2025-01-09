using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections;

namespace Demos.ShooterHud
{
    public class ShootManager : MonoBehaviour
    {
        [SerializeField]
        private bool m_canShoot = true;

        private WaitForSeconds m_waitBetweenShots;
        private WaitForSeconds m_waitBetweenShooting;
        private WaitForSeconds m_waitReload;
        private float m_lastTimeBetweenShots;
        private float m_lastTimeBetweenShooting;
        private float m_lastReloadTime;

        [System.Serializable]
        public class ShootingSettings
        {
            [Tooltip("Force applied to the projectile when shot")]
            public float shootForce = 100f;
            [Tooltip("Upward force applied to give slight arc to projectile")]
            public float upwardForce = 1f;
            [Tooltip("Random spread angle in degrees")]
            public float spread = 2f;
            [Tooltip("Whether to allow holding the shoot button")]
            public bool allowButtonHold = true;
            [Tooltip("Number of bullets fired per tap")]
            public int bulletsPerTap = 1;
            [Tooltip("Invert the shooting direction")]
            public bool invertShootDirection;
        }

        [System.Serializable]
        public class TimingSettings
        {
            [Tooltip("Time between shots")]
            public float timeBetweenShooting = 0.1f;
            [Tooltip("Time between individual bullets when firing multiple per tap")]
            public float timeBetweenShots = 0.05f;
            [Tooltip("Time to reload")]
            public float reloadTime = 1.5f;
        }

        [Header("Input Actions")]
        [SerializeField] private InputActionReference m_shootAction;
        [SerializeField] private InputActionReference m_reloadAction;

        [Header("References")]
        [SerializeField] private RiveMessenger m_riveMessenger;
        [SerializeField] private BulletPoolManager m_bulletPoolManager;
        [SerializeField] private GameObject m_muzzleFlashPrefab;
        [SerializeField] private Camera m_playerCamera;
        [SerializeField] private Transform m_shootPoint;

        [Header("Settings")]
        [SerializeField] private ShootingSettings m_shootingSettings;
        [SerializeField] private TimingSettings m_timingSettings;

        [Header("Magazine")]
        [SerializeField] private int m_magazineSize = 30;

        private bool isReloading;
        private bool readyToShoot = true;
        private bool isShooting = false;
        private int currentBullets;
        private int bulletsShot;

        private Coroutine reloadCoroutine;
        private Coroutine shootResetCoroutine;
        private Coroutine multiShotCoroutine;

        public bool CanShoot
        {
            get => m_canShoot;
            set
            {
                if (m_canShoot != value)
                {
                    m_canShoot = value;
                    OnShootStateChange.Invoke(m_canShoot);
                }
            }
        }

        public UnityEvent<bool> OnShootStateChange = new UnityEvent<bool>();

        private void Awake()
        {
            ValidateReferences();
            currentBullets = m_magazineSize;
            UpdateWaitObjects();
        }

        private void UpdateWaitObjects()
        {
            if (m_lastTimeBetweenShots != m_timingSettings.timeBetweenShots)
            {
                m_waitBetweenShots = new WaitForSeconds(m_timingSettings.timeBetweenShots);
                m_lastTimeBetweenShots = m_timingSettings.timeBetweenShots;
            }

            if (m_lastTimeBetweenShooting != m_timingSettings.timeBetweenShooting)
            {
                m_waitBetweenShooting = new WaitForSeconds(m_timingSettings.timeBetweenShooting);
                m_lastTimeBetweenShooting = m_timingSettings.timeBetweenShooting;
            }

            if (m_lastReloadTime != m_timingSettings.reloadTime)
            {
                m_waitReload = new WaitForSeconds(m_timingSettings.reloadTime);
                m_lastReloadTime = m_timingSettings.reloadTime;
            }
        }

        private void OnEnable()
        {
            EnableInput();
        }

        private void OnDisable()
        {
            DisableInput();
            StopAllCoroutines();
        }

        private void EnableInput()
        {
            m_shootAction.action.Enable();
            m_reloadAction.action.Enable();

            m_shootAction.action.performed += HandleShootPerformed;
            m_shootAction.action.canceled += HandleShootCanceled;
            m_reloadAction.action.performed += HandleReloadInput;
        }

        private void DisableInput()
        {
            m_shootAction.action.Disable();
            m_reloadAction.action.Disable();

            m_shootAction.action.performed -= HandleShootPerformed;
            m_shootAction.action.canceled -= HandleShootCanceled;
            m_reloadAction.action.performed -= HandleReloadInput;
        }

        private void HandleShootPerformed(InputAction.CallbackContext context)
        {
            isShooting = true;
            m_riveMessenger?.FireState(true);

            // Initial shot
            if (currentBullets <= 0 && !isReloading)
            {
                StartReload();
                return;
            }

            TryShoot();

            // Start continuous shooting if button hold is allowed
            if (m_shootingSettings.allowButtonHold)
            {
                StartCoroutine(ContinuousShootCoroutine());
            }
        }

        private void HandleShootCanceled(InputAction.CallbackContext context)
        {
            isShooting = false;
            m_riveMessenger?.FireState(false);

        }

        private IEnumerator ContinuousShootCoroutine()
        {
            while (isShooting && m_shootingSettings.allowButtonHold)
            {
                // Wait for the next shot
                yield return m_waitBetweenShooting;

                if (isShooting && readyToShoot && !isReloading)
                {
                    if (currentBullets <= 0)
                    {
                        StartReload();
                        yield break;
                    }

                    TryShoot();
                }
            }
        }

        private void HandleReloadInput(InputAction.CallbackContext context)
        {
            if (currentBullets < m_magazineSize && !isReloading)
            {
                StartReload();
            }
        }


        private void TryShoot()
        {
            if (!CanShoot || !readyToShoot || isReloading) return;

            if (currentBullets <= 0)
            {
                StartReload();
                return;
            }

            bulletsShot = 0;
            Shoot();
        }

        private void Shoot()
        {
            readyToShoot = false;

            Vector3 targetPoint = CalculateTargetPoint();
            Vector3 shootDirection = CalculateShootDirection(targetPoint);

            SpawnProjectile(shootDirection);
            SpawnMuzzleFlash();

            currentBullets--;
            bulletsShot++;

            if (shootResetCoroutine != null)
                StopCoroutine(shootResetCoroutine);

            shootResetCoroutine = StartCoroutine(ResetShotCoroutine());

            if (bulletsShot < m_shootingSettings.bulletsPerTap && currentBullets > 0)
            {
                if (multiShotCoroutine != null)
                    StopCoroutine(multiShotCoroutine);

                multiShotCoroutine = StartCoroutine(MultiShotCoroutine());
            }
        }

        private Vector3 CalculateTargetPoint()
        {
            Ray ray = m_playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            return Physics.Raycast(ray, out RaycastHit hit) ? hit.point : ray.GetPoint(75f);
        }

        private Vector3 CalculateShootDirection(Vector3 targetPoint)
        {
            Vector3 direction = targetPoint - m_shootPoint.position;
            if (m_shootingSettings.invertShootDirection) direction = -direction;

            // Add spread
            float spreadX = Random.Range(-m_shootingSettings.spread, m_shootingSettings.spread);
            float spreadY = Random.Range(-m_shootingSettings.spread, m_shootingSettings.spread);

            return (direction + new Vector3(spreadX, spreadY, 0f)).normalized;
        }

        private void SpawnProjectile(Vector3 direction)
        {
            Vector3 force = direction * m_shootingSettings.shootForce;
            Vector3 upwardForce = m_playerCamera.transform.up * m_shootingSettings.upwardForce;

            m_bulletPoolManager.SpawnBullet(m_shootPoint.position,
                                          Quaternion.LookRotation(direction),
                                          force,
                                          upwardForce);
        }

        private void SpawnMuzzleFlash()
        {
            if (m_muzzleFlashPrefab != null)
            {
                Instantiate(m_muzzleFlashPrefab, m_shootPoint.position, Quaternion.identity);
            }
        }

        private IEnumerator ResetShotCoroutine()
        {
            if (m_lastTimeBetweenShooting != m_timingSettings.timeBetweenShooting)
                UpdateWaitObjects();

            yield return m_waitBetweenShooting;
            readyToShoot = true;
        }

        private IEnumerator MultiShotCoroutine()
        {
            if (m_lastTimeBetweenShots != m_timingSettings.timeBetweenShots)
                UpdateWaitObjects();

            yield return m_waitBetweenShots;
            Shoot();
        }

        private void StartReload()
        {
            isReloading = true;
            if (reloadCoroutine != null)
                StopCoroutine(reloadCoroutine);

            reloadCoroutine = StartCoroutine(ReloadCoroutine());

        }

        private IEnumerator ReloadCoroutine()
        {
            if (m_lastReloadTime != m_timingSettings.reloadTime)
                UpdateWaitObjects();

            yield return m_waitReload;
            FinishReload();
        }

        private void FinishReload()
        {
            currentBullets = m_magazineSize;
            isReloading = false;
            CanShoot = true;
        }

        private void ValidateReferences()
        {
            if (m_playerCamera == null) m_playerCamera = Camera.main;
            if (m_shootPoint == null) Debug.LogError($"Shoot Point not assigned on {gameObject.name}");
            if (m_bulletPoolManager == null) Debug.LogError($"Bullet Pool Manager not assigned on {gameObject.name}");
            if (m_shootAction == null) Debug.LogError($"Shoot Action not assigned on {gameObject.name}");
            if (m_reloadAction == null) Debug.LogError($"Reload Action not assigned on {gameObject.name}");
        }

        public int GetCurrentBullets() => currentBullets;
        public bool IsReloading() => isReloading;
    }
}