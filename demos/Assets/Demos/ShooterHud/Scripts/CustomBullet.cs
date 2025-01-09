using UnityEngine;
using UnityEngine.Pool;

namespace Demos.ShooterHud
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class CustomBullet : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ExplosionPoolManager m_explosionPoolManager;
        [SerializeField] private LayerMask targetEnemyLayers;

        [SerializeField] private TrailRenderer trailRenderer;

        [Header("Physics Settings")]
        [SerializeField, Range(0f, 1f)] private float bounciness;
        [SerializeField] private bool useGravity;

        [Header("Explosion Settings")]
        [SerializeField] private int explosionDamage;
        [SerializeField] private float explosionRange = 5f;
        [SerializeField] private float explosionForce = 10f;

        [Header("Bullet Settings")]
        [SerializeField] private int maxCollisions = 3;
        [SerializeField] private float maxLifetime = 3f;
        [SerializeField] private bool explodeOnTouch = true;

        private Rigidbody rb;
        private float remainingLifetime;
        private int collisionCount;
        private bool hasExploded;
        private IObjectPool<CustomBullet> pool;
        private PhysicMaterial physicsMaterial;
        private float m_lastReturnToPoolDelay;
        private WaitForSeconds m_returnToPoolDelay;

        public void SetPool(IObjectPool<CustomBullet> bulletPool)
        {
            pool = bulletPool;
        }

        public void SetExplosionPoolManager(ExplosionPoolManager explosionManager)
        {
            m_explosionPoolManager = explosionManager;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            SetupPhysics();
        }

        private void OnEnable()
        {
            hasExploded = false;
            collisionCount = 0;
            remainingLifetime = maxLifetime;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (trailRenderer != null)
            {
                trailRenderer.Clear();
                trailRenderer.enabled = true;
            }
        }

        private void OnDisable()
        {
            if (trailRenderer != null)
            {
                trailRenderer.enabled = false;
            }
        }

        private void Update()
        {
            CheckLifetime();
        }

        private void OnCollisionEnter(Collision collision)
        {
            collisionCount++;

            if (ShouldExplode(collision))
            {
                Explode();
            }
        }

        private void SetupPhysics()
        {
            rb.useGravity = useGravity;

            if (physicsMaterial == null)
            {
                physicsMaterial = new PhysicMaterial
                {
                    bounciness = bounciness,
                    frictionCombine = PhysicMaterialCombine.Minimum,
                    bounceCombine = PhysicMaterialCombine.Maximum
                };
            }

            if (TryGetComponent<SphereCollider>(out var sphereCollider))
            {
                sphereCollider.material = physicsMaterial;
            }
        }

        private void CheckLifetime()
        {
            remainingLifetime -= Time.deltaTime;

            if (remainingLifetime <= 0 || collisionCount > maxCollisions)
            {
                Explode();
            }
        }

        private bool ShouldExplode(Collision collision)
        {
            return explodeOnTouch && collision.gameObject.CompareTag("Enemy");
        }

        private void Explode()
        {
            if (hasExploded) return;

            hasExploded = true;
            SpawnExplosionEffect();
            ApplyExplosionForce();

            if (pool != null)
            {
                StartCoroutine(ReturnToPoolDelayed(0.05f));
            }
            else
            {
                Destroy(gameObject, 0.05f);
            }
        }

        private System.Collections.IEnumerator ReturnToPoolDelayed(float delay)
        {
            if (m_returnToPoolDelay == null || m_lastReturnToPoolDelay != delay)
            {
                m_returnToPoolDelay = new WaitForSeconds(delay);
                m_lastReturnToPoolDelay = delay;
            }

            yield return m_returnToPoolDelay;
            pool.Release(this);
        }

        private void SpawnExplosionEffect()
        {
            if (m_explosionPoolManager != null)
            {
                m_explosionPoolManager.SpawnExplosion(transform.position, Quaternion.identity);
            }
        }

        private void ApplyExplosionForce()
        {
            var affectedObjects = Physics.OverlapSphere(transform.position, explosionRange, targetEnemyLayers);

            foreach (var obj in affectedObjects)
            {
                if (obj.TryGetComponent<Rigidbody>(out var targetRb))
                {
                    targetRb.AddExplosionForce(explosionForce, transform.position, explosionRange);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRange);
        }

        public void Initialize(Vector3 position, Quaternion rotation, Vector3 force, Vector3 upwardForce)
        {
            transform.SetPositionAndRotation(position, rotation);
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(force, ForceMode.Impulse);
            rb.AddForce(upwardForce, ForceMode.Impulse);
        }
    }
}