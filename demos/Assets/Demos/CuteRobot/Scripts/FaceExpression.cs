using System;
using Rive;
using Rive.Components;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Demos.CuteRobot
{
    /// <summary>
    /// Controls Rive face expressions using data binding of an enum property.
    /// </summary>
    public class FaceExpression : MonoBehaviour
    {
        [Tooltip("The Rive widget that displays the face expressions.")]
        [SerializeField] private RiveWidget m_riveWidget;

        private ViewModelInstanceEnumProperty m_expression;

        private void OnEnable()
        {
            Keyboard.current.onTextInput += HandleTextInput;
            m_riveWidget.OnWidgetStatusChanged += OnWidgetStatusChanged;
        }

        private void OnDisable()
        {
            Keyboard.current.onTextInput -= HandleTextInput;
            m_riveWidget.OnWidgetStatusChanged -= OnWidgetStatusChanged;
        }

        private void OnWidgetStatusChanged()
        {
            if (m_riveWidget.Status == WidgetStatus.Loaded)
            {
                InitializeVMI();

                m_riveWidget.OnWidgetStatusChanged -= OnWidgetStatusChanged;
            }
        }

        /// <summary>
        /// Retrieves the Expression enum property from the ViewModelInstance.
        /// </summary>
        private void InitializeVMI()
        {
            var vmi = m_riveWidget.StateMachine.ViewModelInstance;
            if (vmi == null)
            {
                Debug.LogWarning("ViewModelInstance not found in State Machine.");
                return;
            }
            m_expression = vmi.GetEnumProperty("Expression");
            if (m_expression == null)
            {
                Debug.LogWarning("Expression property not found in ViewModelInstance.");
                return;
            }
        }

        /// <summary>
        /// Handles text input from the keyboard to change face expressions.
        /// 
        /// The number keys 0-9 correspond to different expressions.
        /// </summary>
        /// <param name="obj">The character input from the keyboard.</param>
        private void HandleTextInput(char obj)
        {
            if (m_expression == null)
            {
                Debug.LogWarning("Expression property not found.");
                return;
            }
            // Check if the input is a number and set the expression if it is between 0 and 9
            // We have inputs mapped to the number keys 0-9
            if (int.TryParse(obj.ToString(), out int expressionNumber) &&
                expressionNumber >= 0 && expressionNumber <= 9)
            {
                switch (expressionNumber)
                {
                    case 0:
                        m_expression.Value = "Neutral";
                        break;
                    case 1:
                        m_expression.Value = "Happy";
                        break;
                    case 2:
                        m_expression.Value = "ExtraHappy";
                        break;
                    case 3:
                        m_expression.Value = "Sad";
                        break;
                    case 4:
                        m_expression.Value = "Angry";
                        break;
                    case 5:
                        m_expression.Value = "Amazed";
                        break;
                    case 6:
                        m_expression.Value = "ExtraAmazed";
                        break;
                    case 7:
                        m_expression.Value = "Dead";
                        break;
                    case 8:
                        m_expression.Value = "Error";
                        break;
                    case 9:
                        m_expression.Value = "Loading";
                        break;
                }
            }
        }
    }
}
