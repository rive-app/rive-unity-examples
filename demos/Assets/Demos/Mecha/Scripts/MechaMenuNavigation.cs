using System;
using System.Collections;
using System.Collections.Generic;
using Rive;
using Rive.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


namespace Demos.Mecha
{
    public class MechaMenuNavigation : MonoBehaviour
    {
        enum CurrentScreen
        {
            Main = 0,
            NewGame = 1,
            Settings = 2
        }

        [SerializeField] private RiveWidget m_riveWidget;


        [SerializeField] private InputActionReference navAction;
        [SerializeField] private InputActionReference continueAction;
        [SerializeField] private InputActionReference backAction;


        private readonly string kNestedNewGameMenuIndexInputName = "menuIndex";
        private readonly string kNestedNewGameMenuIndexInputPath = "New Game Screen";

        private SMITrigger backInput;
        private SMITrigger continueInput;
        private SMITrigger controllerInput;

        private SMINumber inputDeviceInput;
        private SMINumber menuIndexSettingsInput;
        private SMINumber menuIndexInput;

        private CurrentScreen m_currentScreen = CurrentScreen.Main;

        private bool m_setupComplete = false;



        void OnEnable()
        {
            m_riveWidget.OnWidgetStatusChanged += Setup;

            m_riveWidget.OnRiveEventReported += OnMenuRiveEvent;


            navAction.action.performed += UpdateNavMovement;
            continueAction.action.performed += UpdateContinueAction;
            backAction.action.performed += UpdateBackAction;

            navAction.action.Enable();
            continueAction.action.Enable();
            backAction.action.Enable();

            Keyboard.current.onTextInput += HandleTextInput;

        }

        private void Setup()
        {
            if (m_riveWidget.Status == WidgetStatus.Loaded && !m_setupComplete)
            {
                // Reference all state machine inputs
                List<SMIInput> inputs = m_riveWidget.StateMachine.Inputs();
                foreach (var input in inputs)
                {

                    switch (input.Name)
                    {
                        case "inputDevice":
                            {
                                inputDeviceInput = (SMINumber)input;
                                break;
                            }
                        case "back":
                            {
                                backInput = (SMITrigger)input;
                                break;
                            }
                        case "continue":
                            {
                                continueInput = (SMITrigger)input;
                                break;
                            }
                        case "menuIndexSettings":
                            {
                                menuIndexSettingsInput = (SMINumber)input;
                                break;
                            }
                        case "menuIndex":
                            {
                                menuIndexInput = (SMINumber)input;
                                break;
                            }
                        case "controllerInput":
                            {
                                controllerInput = (SMITrigger)input;
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }


                // Highlight the start option in the menu
                StartCoroutine(HighlightMenuOption(2));


                m_setupComplete = true;
            }

        }

        private IEnumerator HighlightMenuOption(int index)
        {
            yield return new WaitForSeconds(0.1f);
            menuIndexInput.Value = index;
            controllerInput.Fire();
        }

        void OnDisable()
        {

            m_riveWidget.OnWidgetStatusChanged -= Setup;
            m_riveWidget.OnRiveEventReported -= OnMenuRiveEvent;

            navAction.action.performed -= UpdateNavMovement;
            continueAction.action.performed -= UpdateContinueAction;
            backAction.action.performed -= UpdateBackAction;

            navAction.action.Disable();
            continueAction.action.Disable();
            backAction.action.Disable();

            Keyboard.current.onTextInput -= HandleTextInput;

        }

        private void HandleTextInput(char obj)
        {
            // If B is pressed, we go back
            if (obj == 'b' || obj == 'B')
            {
                HandleBackAction();
            }
        }

        private float HandleMenuNavigation(Vector2 direction, float currentValue, bool isVertical, int minBound, int maxBound)
        {
            // Use y for vertical (main menu) and x for horizontal (settings) navigation
            float directionValue = isVertical ? direction.y : direction.x;

            float newPosition = isVertical ? currentValue - directionValue : currentValue + directionValue;

            // Wrap around bounds
            if (newPosition < minBound)
            {
                newPosition = maxBound;
            }
            else if (newPosition > maxBound)
            {
                newPosition = minBound;
            }

            return newPosition;
        }

        private void UpdateNavMovement(InputAction.CallbackContext context)
        {
            var direction = context.ReadValue<Vector2>();

            if (m_currentScreen == CurrentScreen.Main)
            {
                menuIndexInput.Value = HandleMenuNavigation(direction, menuIndexInput.Value, true, 0, 4);
                controllerInput.Fire();
            }

            if (m_currentScreen == CurrentScreen.Settings)
            {
                menuIndexSettingsInput.Value = HandleMenuNavigation(direction, menuIndexSettingsInput.Value, false, 0, 1);
            }

            if (m_currentScreen == CurrentScreen.NewGame)
            {
                int minBound = 1;
                int maxBound = 3;
                float newVal = HandleMenuNavigation(direction, GetNewGameMenuIndex(), false, minBound, maxBound);
                SetNewGameMenuIndex(newVal);
            }
        }



        private float GetNewGameMenuIndex()
        {

            float? menuIndex = m_riveWidget.Artboard.GetNumberInputStateAtPath(kNestedNewGameMenuIndexInputName, kNestedNewGameMenuIndexInputPath);
            if (!menuIndex.HasValue)
            {
                return 0;
            }
            return menuIndex.Value;
        }

        private void SetNewGameMenuIndex(float index)
        {
            m_riveWidget.Artboard.SetNumberInputStateAtPath(kNestedNewGameMenuIndexInputName, index, kNestedNewGameMenuIndexInputPath);
        }

        private void UpdateContinueAction(InputAction.CallbackContext context)
        {
            continueInput.Fire();


            if (menuIndexInput.Value == 2)
            {
                m_currentScreen = CurrentScreen.NewGame;
                SetNewGameMenuIndex(1);

            }
            else if (menuIndexInput.Value == 3)
            {
                m_currentScreen = CurrentScreen.Settings;

            }
            else if (menuIndexInput.Value == 4)
            {
                GoBackToMainDemo();
            }
        }

        private void UpdateBackAction(InputAction.CallbackContext context)
        {
            HandleBackAction();
        }

        private void HandleBackAction()
        {
            backInput.Fire();

            if (m_currentScreen == CurrentScreen.Main)
            {
                GoBackToMainDemo();
            }

            m_currentScreen = CurrentScreen.Main;
        }

        void GoBackToMainDemo()
        {
            BackMenuController.NavigateToMainMenu();
        }


        void Start()
        {
            Setup();
        }

        private void OnMenuRiveEvent(ReportedEvent reportedEvent)
        {
            if (reportedEvent.Name.StartsWith("Back"))
            {
                m_currentScreen = CurrentScreen.Main;
            }




        }
    }
}