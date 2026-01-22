using UnityEngine;
using UnityEngine.InputSystem;

public class HealthBarKeyboardControls : MonoBehaviour
{
    [SerializeField] private HealthBarController m_healthBar;
    [SerializeField] private float m_damageAmount = 10f;
    [SerializeField] private float m_healAmount = 10f;

    private void Update()
    {
        if (m_healthBar == null)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        // Damage (left arrow key )
        if (keyboard.leftArrowKey.wasPressedThisFrame)
            m_healthBar.Damage(m_damageAmount);

        // Heal (right arrow key)
        if (keyboard.rightArrowKey.wasPressedThisFrame)
            m_healthBar.Heal(m_healAmount);
    }
}