using UnityEngine;
using UnityEngine.Pool;

namespace Demos.ShooterHud
{
    public class ExplosionPoolManager : MonoBehaviour
    {
        [Header("Pool Settings")]
        [SerializeField] private GameObject m_explosionPrefab;
        [SerializeField] private int m_defaultPoolSize = 10;
        [SerializeField] private int m_maxPoolSize = 30;
        [SerializeField] private bool m_collectionChecks = true;
        [SerializeField] private float m_explosionDuration = 2f;

        private IObjectPool<GameObject> m_explosionPool;
        private WaitForSeconds m_explosionDelay;

        private void Awake()
        {
            InitializePool();
            m_explosionDelay = new WaitForSeconds(m_explosionDuration);
        }

        private void InitializePool()
        {
            m_explosionPool = new ObjectPool<GameObject>(
                createFunc: CreateExplosion,
                actionOnGet: OnExplosionTaken,
                actionOnRelease: OnExplosionReturned,
                actionOnDestroy: OnExplosionDestroyed,
                collectionCheck: m_collectionChecks,
                defaultCapacity: m_defaultPoolSize,
                maxSize: m_maxPoolSize
            );
        }

        private GameObject CreateExplosion()
        {
            GameObject explosion = Instantiate(m_explosionPrefab);
            return explosion;
        }

        private void OnExplosionTaken(GameObject explosion)
        {
            explosion.SetActive(true);
            StartCoroutine(ReleaseExplosionDelayed(explosion));
        }

        private void OnExplosionReturned(GameObject explosion)
        {
            explosion.SetActive(false);
        }

        private void OnExplosionDestroyed(GameObject explosion)
        {
            Destroy(explosion);
        }

        private System.Collections.IEnumerator ReleaseExplosionDelayed(GameObject explosion)
        {
            yield return m_explosionDelay;
            m_explosionPool.Release(explosion);
        }

        public void SpawnExplosion(Vector3 position, Quaternion rotation)
        {
            GameObject explosion = m_explosionPool.Get();
            explosion.transform.SetPositionAndRotation(position, rotation);
        }

        private void OnValidate()
        {
            if (m_explosionPrefab == null)
            {
                Debug.LogError($"Explosion Prefab not assigned on {gameObject.name}");
            }
        }
    }
}