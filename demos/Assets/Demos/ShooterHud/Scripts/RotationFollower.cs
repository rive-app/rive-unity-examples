using UnityEngine;

namespace Demos.ShooterHud
{
    /// <summary>
    /// Smoothly follows the X rotation of a camera and Y rotation of a player.
    /// </summary>
    public class RotationFollower : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform playerTransform;

        [SerializeField, Range(0.1f, 5f)]
        private float rotationDelay = 2f;

        private Vector3 currentVelocity;

        private void Awake()
        {
            if (cameraTransform == null)
            {
                cameraTransform = Camera.main.transform;
            }

            if (playerTransform == null)
            {
                Debug.LogWarning("Player Transform not assigned to Rotation Follower", this);
            }
        }

        private void LateUpdate()
        {
            if (playerTransform == null) return;

            var targetRotation = Quaternion.Euler(cameraTransform.eulerAngles.x,
                playerTransform.eulerAngles.y, 0f);

            transform.rotation = SmoothDampQuaternion(transform.rotation,
                targetRotation, ref currentVelocity, rotationDelay);
        }

        private static Quaternion SmoothDampQuaternion(Quaternion current, Quaternion target,
            ref Vector3 currentVelocity, float smoothTime)
        {
            if (Time.deltaTime == 0) return current;
            if (smoothTime == 0) return target;

            var currentEuler = current.eulerAngles;
            var targetEuler = target.eulerAngles;

            return Quaternion.Euler(
                Mathf.SmoothDampAngle(currentEuler.x, targetEuler.x, ref currentVelocity.x, smoothTime),
                Mathf.SmoothDampAngle(currentEuler.y, targetEuler.y, ref currentVelocity.y, smoothTime),
                Mathf.SmoothDampAngle(currentEuler.z, targetEuler.z, ref currentVelocity.z, smoothTime)
            );
        }
    }
}