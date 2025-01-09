using System;
using System.Collections;
using System.Collections.Generic;
using Rive;
using Rive.Components;
using Unity.VisualScripting;
using UnityEngine;
namespace Demos.ShooterHud
{
    public class RiveMessenger : MonoBehaviour
    {

        [SerializeField]
        private RiveWidget m_riveWidget;


        public ShootManager ShootManager;

        private SMIBool m_fire;
        private SMINumber m_weaponId;

        private bool m_overheating = false;

        private bool m_setupComplete = false;

        private void OnEnable()
        {
            m_riveWidget.OnWidgetStatusChanged += Setup;
            m_riveWidget.OnRiveEventReported += OnRiveEventReported;
        }

        private void OnDisable()
        {
            m_riveWidget.OnWidgetStatusChanged -= Setup;
            m_riveWidget.OnRiveEventReported -= OnRiveEventReported;
        }

        private void Setup()
        {

            if (m_setupComplete)
            {
                return;
            }

            if (m_riveWidget.Status == WidgetStatus.Loaded)
            {

                m_fire = m_riveWidget.StateMachine.GetBool("Fire");
                m_weaponId = m_riveWidget.StateMachine.GetNumber("Weapon ID");

                m_setupComplete = true;
            }


        }

        private void OnRiveEventReported(ReportedEvent reportedEvent)
        {
            if (reportedEvent.Name == "Overheat")
            {

                for (uint i = 0; i < reportedEvent.PropertyCount; i++)
                {
                    var prop = reportedEvent.GetProperty(i);
                    if (prop.TryGetBool(out bool value))
                    {

                        m_overheating = value;
                        ShootManager.CanShoot = !m_overheating;
                    }


                }

            }
        }

        void Start()
        {
            Setup();
        }

        public void FireState(bool value)
        {
            if (m_overheating && value) return;

            if (m_fire.Value != value)
            {
                m_fire.Value = value;
            }
        }

        private Coroutine m_fireCoroutine;

        public void FireOnce(float timeBetweenShooting)
        {
            m_fire.Value = true;
            if (m_fireCoroutine != null)
            {
                StopCoroutine(m_fireCoroutine);
            }
            m_fireCoroutine = StartCoroutine(TurnOff(timeBetweenShooting));
        }

        private IEnumerator TurnOff(float timeBetweenShooting)
        {
            yield return new WaitForSeconds(timeBetweenShooting);
            m_fire.Value = false;
        }
    }
}
