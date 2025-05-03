using System;
using Rive;
using Rive.Components;
using UnityEngine;

public class TextOverlayController : MonoBehaviour
{
    [SerializeField] private RiveWidget m_riveWidget;

    [SerializeField] private string m_textToDisplay = "Hello, World!";

    [SerializeField] private int m_fontSize = 20;

    private ViewModelInstanceStringProperty m_textProperty;
    private ViewModelInstanceNumberProperty m_fontSizeProperty;

    private void OnEnable()
    {
        m_riveWidget.OnWidgetStatusChanged += OnWidgetStatusChanged;
    }

    private void OnWidgetStatusChanged()
    {
        if (m_riveWidget.Status == WidgetStatus.Loaded)
        {
            var vmInstance = m_riveWidget.StateMachine.ViewModelInstance;
            m_textProperty = vmInstance.GetStringProperty("value");
            m_fontSizeProperty = vmInstance.GetNumberProperty("font_size");

            if (m_textProperty != null)
            {
                m_textProperty.Value = m_textToDisplay;
            }

            if (m_fontSizeProperty != null)
            {
                m_fontSizeProperty.Value = m_fontSize;
            }


        }
    }

    private void Update()
    {
        if (m_textProperty != null && m_textProperty.Value != m_textToDisplay)
        {
            m_textProperty.Value = m_textToDisplay;
        }

        if (m_fontSizeProperty != null && m_fontSizeProperty.Value != m_fontSize)
        {
            m_fontSizeProperty.Value = m_fontSize;
        }
    }

    private void OnDisable()
    {
        m_riveWidget.OnWidgetStatusChanged -= OnWidgetStatusChanged;
    }
}
