using System;
using Rive;
using Rive.Components;
using UnityEngine;

public class TextOverlayController : MonoBehaviour
{
    private enum HorizontalAlignment
    {
        Left,
        Right,
        Center
    }

    private enum VerticalAlignment
    {
        Top,
        Bottom,
        Middle
    }



    [SerializeField] private RiveWidget m_riveWidget;

    [SerializeField] private string m_textToDisplay = "Hello, World!";

    [SerializeField] private int m_fontSize = 20;

    [SerializeField] private Color32 m_textColor = new Color32(255, 255, 255, 255);


    [SerializeField] private HorizontalAlignment m_horizontalAlignment = HorizontalAlignment.Left;

    [SerializeField] private VerticalAlignment m_verticalAlignment = VerticalAlignment.Top;

    private ViewModelInstanceStringProperty m_textProperty;
    private ViewModelInstanceNumberProperty m_fontSizeProperty;
    private ViewModelInstanceColorProperty m_textColorProperty;

    private ViewModelInstanceEnumProperty m_horizontalAlignmentProperty;
    private ViewModelInstanceEnumProperty m_verticalAlignmentProperty;

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
            m_textColorProperty = vmInstance.GetColorProperty("color");
            m_horizontalAlignmentProperty = vmInstance.GetEnumProperty("horizontal_alignment");
            m_verticalAlignmentProperty = vmInstance.GetEnumProperty("vertical_alignment");

            if (m_textProperty != null)
            {
                m_textProperty.Value = m_textToDisplay;
            }

            if (m_fontSizeProperty != null)
            {
                m_fontSizeProperty.Value = m_fontSize;
            }

            if (m_textColorProperty != null)
            {
                m_textColorProperty.Value = m_textColor;
            }

            if (m_horizontalAlignmentProperty != null)
            {
                m_horizontalAlignmentProperty.Value = m_horizontalAlignmentProperty.EnumValues[(int)m_horizontalAlignment];
            }
            if (m_verticalAlignmentProperty != null)
            {
                m_verticalAlignmentProperty.Value = m_verticalAlignmentProperty.EnumValues[(int)m_verticalAlignment];
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

        if (m_textColorProperty != null && m_textColorProperty.Value != m_textColor)
        {
            m_textColorProperty.Value = m_textColor;
        }

        if (m_horizontalAlignmentProperty != null && m_horizontalAlignmentProperty.Value != m_horizontalAlignmentProperty.EnumValues[(int)m_horizontalAlignment])
        {
            m_horizontalAlignmentProperty.Value = m_horizontalAlignmentProperty.EnumValues[(int)m_horizontalAlignment];
        }

        if (m_verticalAlignmentProperty != null && m_verticalAlignmentProperty.Value != m_verticalAlignmentProperty.EnumValues[(int)m_verticalAlignment])
        {
            m_verticalAlignmentProperty.Value = m_verticalAlignmentProperty.EnumValues[(int)m_verticalAlignment];
        }
    }

    private void OnDisable()
    {
        m_riveWidget.OnWidgetStatusChanged -= OnWidgetStatusChanged;
    }
}
