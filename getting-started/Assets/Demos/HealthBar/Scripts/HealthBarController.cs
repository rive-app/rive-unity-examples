using System;
using Rive;
using Rive.Components;
using UnityEngine;
using UnityEngine.Events;

public class HealthBarController : MonoBehaviour
{
    [Header("Rive")]
    [Tooltip("The Rive Widget that is displaying your health bar file.")]
    [SerializeField] private RiveWidget m_riveWidget;

    [Header("Initial health")]
    [Tooltip("Initial health applied when the widget finishes loading.")]
    [SerializeField] private float m_startingHealth = 100f;

    [Header("ViewModel Property Names")]
    [Tooltip("ViewModel Number property name used by the Rive file.")]
    [SerializeField] private string m_healthPropertyName = "health";

    [Tooltip("ViewModel Trigger property name fired by the Rive file.")]
    [SerializeField] private string m_gameOverPropertyName = "gameOver";

    [Header("Events")]
    [Tooltip("Invoked whenever health changes in the Rive file. It is called with the new health value.")]
    public FloatEvent OnHealthChanged = new FloatEvent();

    [Tooltip("Invoked when the Rive file fires the gameOver trigger.")]
    public UnityEvent OnGameOver = new UnityEvent();

    [Serializable]
    public class FloatEvent : UnityEvent<float> { }

    private ViewModelInstanceNumberProperty m_healthProperty;
    private ViewModelInstanceTriggerProperty m_gameOverProperty;

    // Track whether we've initialized health so we don't overwrite it if the widget GameObject is disabled and re-enabled.
    private bool m_hasInitialized;

    public float Health
    {
        get
        {
            // If the widget is loaded, read from the Rive view model instance property
            if (m_healthProperty != null)
            {
                return m_healthProperty.Value;
            }

            // Widget isn't loaded yet, return the starting value
            return m_startingHealth;
        }
    }

    private void WriteHealth(float value)
    {
        // If the widget is loaded, write to the Rive view model instance property
        if (m_healthProperty != null)
        {
            m_healthProperty.Value = value;
            return;
        }

        // Widget isn't loaded yet. Store it so we can apply it when the view model instance is ready.
        m_startingHealth = value;
    }

    private void OnEnable()
    {
        if (m_riveWidget == null)
        {
            Debug.LogError($"{nameof(HealthBarController)}: No RiveWidget assigned.", this);
            return;
        }

        m_riveWidget.OnWidgetStatusChanged += HandleWidgetStatusChanged;

        // If the widget was already loaded before we subscribed, initialize the health bar immediately.
        HandleWidgetStatusChanged();
    }

    private void OnDisable()
    {
        if (m_riveWidget != null)
        {
            m_riveWidget.OnWidgetStatusChanged -= HandleWidgetStatusChanged;
        }

        // Clean up event listeners to avoid duplicate subscriptions.
        if (m_healthProperty != null)
            m_healthProperty.OnValueChanged -= HandleHealthChangedFromRive;

        if (m_gameOverProperty != null)
            m_gameOverProperty.OnTriggered -= HandleGameOverTriggeredFromRive;
    }

    private void HandleWidgetStatusChanged()
    {
        if (m_riveWidget.Status != WidgetStatus.Loaded)
            return;

        ViewModelInstance viewModelInstance = m_riveWidget.StateMachine?.ViewModelInstance;
        if (viewModelInstance == null)
        {
            Debug.LogError($"{nameof(HealthBarController)}: ViewModelInstance is null. " +
                           "Make sure Data Binding Mode is set to Auto Bind Default / Selected.", this);
            return;
        }

        // Clean up old listeners first.
        if (m_healthProperty != null)
            m_healthProperty.OnValueChanged -= HandleHealthChangedFromRive;

        if (m_gameOverProperty != null)
            m_gameOverProperty.OnTriggered -= HandleGameOverTriggeredFromRive;

        // Get the health property by name.
        m_healthProperty = viewModelInstance.GetNumberProperty(m_healthPropertyName);
        if (m_healthProperty == null)
        {
            Debug.LogError($"{nameof(HealthBarController)}: Number property '{m_healthPropertyName}' not found.", this);
            return;
        }

        // Get the gameOver property by name.
        m_gameOverProperty = viewModelInstance.GetTriggerProperty(m_gameOverPropertyName);
        if (m_gameOverProperty == null)
        {
            Debug.LogError($"{nameof(HealthBarController)}: Trigger property '{m_gameOverPropertyName}' not found.", this);
            return;
        }

        // Subscribe to changes from the Rive file for the health and gameOver properties.
        m_healthProperty.OnValueChanged += HandleHealthChangedFromRive;
        m_gameOverProperty.OnTriggered += HandleGameOverTriggeredFromRive;

        // Set the initial health value only once
        if (!m_hasInitialized)
        {
            m_healthProperty.Value = m_startingHealth;
            m_hasInitialized = true;
        }
    }

    public void Damage(float amount)
    {
        WriteHealth(Health - amount);
    }

    public void Heal(float amount)
    {
        WriteHealth(Health + amount);
    }

    private void HandleHealthChangedFromRive(float newValue)
    {
        OnHealthChanged.Invoke(newValue);
    }

    private void HandleGameOverTriggeredFromRive()
    {
        OnGameOver.Invoke();
    }
}