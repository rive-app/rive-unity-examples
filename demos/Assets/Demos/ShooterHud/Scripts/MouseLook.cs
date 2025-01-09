using UnityEngine;
using UnityEngine.InputSystem;

namespace Demos.ShooterHud
{
    public class MouseLook : MonoBehaviour
    {
        [SerializeField] private float mouseSensitivity = 100f;
        [SerializeField] private Transform playerBody;
        [SerializeField] private InputActionReference lookAction;

        private float xRotation = 0f;
        private Vector2 lookInput;

        private void OnEnable()
        {
            lookAction.action.Enable();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDisable()
        {
            lookAction.action.Disable();
            Cursor.lockState = CursorLockMode.Confined;
        }

        private void Update()
        {
            lookInput = lookAction.action.ReadValue<Vector2>();

            float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
            float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}