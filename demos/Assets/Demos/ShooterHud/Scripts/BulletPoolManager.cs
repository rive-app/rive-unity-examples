using UnityEngine;
using UnityEngine.Pool;

namespace Demos.ShooterHud
{
    public class BulletPoolManager : MonoBehaviour
    {
        [Header("Pool Settings")]
        [SerializeField] private GameObject m_bulletPrefab;
        [SerializeField] private int m_defaultPoolSize = 20;
        [SerializeField] private int m_maxPoolSize = 100;
        [SerializeField] private bool m_collectionChecks = true;

        [SerializeField] private ExplosionPoolManager m_explosionPoolManager;

        private IObjectPool<CustomBullet> m_bulletPool;
        public IObjectPool<CustomBullet> BulletPool => m_bulletPool;

        private void Awake()
        {
            InitializePool();
        }

        private void InitializePool()
        {
            m_bulletPool = new ObjectPool<CustomBullet>(
                createFunc: CreateBullet,
                actionOnGet: OnBulletTaken,
                actionOnRelease: OnBulletReturned,
                actionOnDestroy: OnBulletDestroyed,
                collectionCheck: m_collectionChecks,
                defaultCapacity: m_defaultPoolSize,
                maxSize: m_maxPoolSize
            );
        }

        private CustomBullet CreateBullet()
        {
            GameObject bulletGO = Instantiate(m_bulletPrefab);

            CustomBullet bullet = bulletGO.GetComponent<CustomBullet>();

            if (bullet == null)
            {
                Debug.LogError($"Bullet Prefab does not have a CustomBullet component attached to it. Please attach one to the prefab.");
                return null;
            }

            bullet.SetPool(m_bulletPool);

            bullet.SetExplosionPoolManager(m_explosionPoolManager);
            return bullet;
        }

        private void OnBulletTaken(CustomBullet bullet)
        {
            bullet.gameObject.SetActive(true);
        }

        private void OnBulletReturned(CustomBullet bullet)
        {
            bullet.gameObject.SetActive(false);
        }

        private void OnBulletDestroyed(CustomBullet bullet)
        {
            Destroy(bullet.gameObject);
        }

        public CustomBullet SpawnBullet(Vector3 position, Quaternion rotation, Vector3 force, Vector3 upwardForce)
        {
            CustomBullet bullet = m_bulletPool.Get();
            bullet.Initialize(position, rotation, force, upwardForce);
            return bullet;
        }

        private void OnValidate()
        {
            if (m_bulletPrefab == null)
            {
                Debug.LogError($"Bullet Prefab not assigned on {gameObject.name}");
            }
        }
    }
}