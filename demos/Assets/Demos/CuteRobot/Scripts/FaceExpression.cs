using System;
using Rive;
using Rive.Components;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Demos.CuteRobot
{
    /// <summary>
    /// Controls Rive face expressions using the new Input System.
    /// Requires a FaceControls Input Action with a Value axis binding for numbers 0-9.
    /// </summary>
    public class FaceExpression : MonoBehaviour
    {
        [Tooltip("The Rive widget that displays the face expressions.")]
        [SerializeField] private RiveWidget m_riveWidget;

        private SMINumber m_numExpression;

        private void OnEnable()
        {
            Keyboard.current.onTextInput += HandleTextInput;
        }

        private void Start()
        {
            if (m_riveWidget.Status == WidgetStatus.Loaded)
            {
                SetExpressionInputFromWidget();
                return;
            }

            m_riveWidget.OnWidgetStatusChanged += OnWidgetStatusChanged;


        }

        private void HandleTextInput(char obj)
        {
            if (m_numExpression == null)
            {
                Debug.LogWarning("Number expression not found.");
                return;
            }
            // Check if the input is a number and set the expression if it is between 0 and 9
            // We have inputs mapped to the number keys 0-9
            if (int.TryParse(obj.ToString(), out int expressionNumber) &&
                expressionNumber >= 0 && expressionNumber <= 9)
            {
                m_numExpression.Value = expressionNumber;
            }
        }

        private void OnDisable()
        {
            Keyboard.current.onTextInput -= HandleTextInput;
        }


        private void OnWidgetStatusChanged()
        {
            if (m_riveWidget.Status == WidgetStatus.Loaded)
            {
                SetExpressionInputFromWidget();

                m_riveWidget.OnWidgetStatusChanged -= OnWidgetStatusChanged;

            }
        }

        private void SetExpressionInputFromWidget()
        {
            if (m_numExpression == null)
            {
                m_numExpression = m_riveWidget.StateMachine.GetNumber("numExpression");
            }
        }


    }
}