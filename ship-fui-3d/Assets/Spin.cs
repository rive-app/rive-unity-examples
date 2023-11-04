using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

using LoadAction = UnityEngine.Rendering.RenderBufferLoadAction;
using StoreAction = UnityEngine.Rendering.RenderBufferStoreAction;

namespace Rive
{
    public class Spin : MonoBehaviour
    {
        public float speed = 5.0f;
        public float settledSpeed = 1.0f;

        private float m_appliedSpeed;

        private void OnEnable()
        {
            m_appliedSpeed = speed;
        }
        private void Update()
        {
            transform.Rotate(0.0f, m_appliedSpeed * Time.deltaTime, 0.0f);
            m_appliedSpeed += (settledSpeed - m_appliedSpeed) * Mathf.Min(1.0f, 1.5f * Time.deltaTime);
        }

    }
}
