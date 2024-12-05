using System.Collections.Generic;
using Rive;
using Rive.Components;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Demos.Building
{
    public class BuildingControls : MonoBehaviour
    {

        [SerializeField] private List<RiveWidget> m_riveWidgetsWithColorMode;


        [SerializeField] private List<RiveWidget> m_riveWidgetsWithLotus;

        [SerializeField] private List<RiveWidget> m_riveWidgetsWithNumText;

        private bool m_isColorful = false;


        private Dictionary<RiveWidget, SMINumber> m_colorfulInputs = new Dictionary<RiveWidget, SMINumber>();
        private Dictionary<RiveWidget, SMINumber> m_numTextInputs = new Dictionary<RiveWidget, SMINumber>();
        private Dictionary<RiveWidget, SMITrigger> m_lotusSignInputs = new Dictionary<RiveWidget, SMITrigger>();


        private void OnEnable()
        {

            Keyboard.current.onTextInput += OnKeyboardInputReceived;
        }


        private void OnDisable()
        {

            Keyboard.current.onTextInput -= OnKeyboardInputReceived;

        }

        private void OnKeyboardInputReceived(char ch)
        {
            // If the value is C, toggle the color mode
            if (char.ToLower(ch) == 'c')
            {
                m_isColorful = !m_isColorful;
                SetColorful(m_isColorful);
            }

            // If the value is L, trigger the lotus sign
            if (char.ToLower(ch) == 'l')
            {
                TriggerLotusChange();
            }

            // Use the number keys to change the number, it should go between 0 and 6
            if (char.IsDigit(ch) && ch >= '0' && ch <= '6')
            {
                if (int.TryParse(ch.ToString(), out int result))
                {
                    SetNumTextInputValue(result);
                }
            }
        }


        private void SetColorful(bool isColorful)
        {
            int colorMode = isColorful ? 1 : 0;

            for (int i = 0; i < m_riveWidgetsWithColorMode.Count; i++)
            {
                var widget = m_riveWidgetsWithColorMode[i];

                if (widget.Status != WidgetStatus.Loaded)
                {
                    continue;
                }

                SMINumber colorModeInput = null;

                if (!m_colorfulInputs.ContainsKey(widget))
                {
                    colorModeInput = widget.StateMachine.GetNumber("color_mode_num");

                    if (colorModeInput == null)
                    {
                        Debug.LogError("Color mode input not found - " + widget.name);
                    }

                    m_colorfulInputs.Add(widget, colorModeInput);

                }
                else if (m_colorfulInputs.ContainsKey(widget))
                {
                    colorModeInput = m_colorfulInputs[widget];
                }

                if (colorModeInput != null)
                {
                    colorModeInput.Value = colorMode;
                }
            }


        }

        private void SetNumTextInputValue(int val)
        {

            for (int i = 0; i < m_riveWidgetsWithNumText.Count; i++)
            {
                var widget = m_riveWidgetsWithNumText[i];

                if (widget.Status != WidgetStatus.Loaded)
                {
                    continue;
                }

                SMINumber numTextInput = null;

                if (!m_numTextInputs.ContainsKey(widget))
                {
                    numTextInput = widget.StateMachine.GetNumber("num_text");

                    if (numTextInput == null)
                    {
                        Debug.LogError("Num text input not found - " + widget.name);
                    }

                    m_numTextInputs.Add(widget, numTextInput);

                }
                else if (m_numTextInputs.ContainsKey(widget))
                {
                    numTextInput = m_numTextInputs[widget];
                }

                if (numTextInput != null)
                {
                    numTextInput.Value = val;
                }
            }


        }

        private void TriggerLotusChange()
        {
            for (int i = 0; i < m_riveWidgetsWithLotus.Count; i++)
            {
                var widget = m_riveWidgetsWithLotus[i];

                if (widget.Status != WidgetStatus.Loaded)
                {
                    continue;
                }

                SMITrigger lotusSignInput = null;

                if (!m_lotusSignInputs.ContainsKey(widget))
                {
                    lotusSignInput = widget.StateMachine.GetTrigger("lotusSignIsOn");

                    if (lotusSignInput == null)
                    {
                        Debug.LogError("Lotus sign input not found - " + widget.name);
                    }

                    m_lotusSignInputs.Add(widget, lotusSignInput);

                }
                else if (m_lotusSignInputs.ContainsKey(widget))
                {
                    lotusSignInput = m_lotusSignInputs[widget];
                }

                if (lotusSignInput != null)
                {
                    lotusSignInput.Fire();
                }
            }
        }

    }
}