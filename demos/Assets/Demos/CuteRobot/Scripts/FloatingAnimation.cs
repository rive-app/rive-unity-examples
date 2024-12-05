using UnityEngine;

namespace Demos.CuteRobot
{
    /// <summary>
    /// Creates a simple floating up and down animation using a sine wave.
    /// </summary>
    public class FloatingAnimation : MonoBehaviour
    {
        [SerializeField, Tooltip("Maximum distance the object will move up and down")]
        [Range(0.1f, 2f)] private float bounceHeight = 0.5f;

        [SerializeField, Tooltip("Speed of the floating animation")]
        [Range(0.1f, 5f)] private float bounceSpeed = 2f;

        private Vector3 startPosition;

        private void Start()
        {
            startPosition = transform.position;
        }

        private void Update()
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }
    }
}