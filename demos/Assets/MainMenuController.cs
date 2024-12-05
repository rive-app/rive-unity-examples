using System;
using Rive;
using Rive.Components;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private RiveWidget m_riveWidget;

    private void OnEnable()
    {
        m_riveWidget.OnRiveEventReported += OnRiveEventReported;
    }


    private void OnRiveEventReported(ReportedEvent reportedEvt)
    {
        if (Int32.TryParse(reportedEvt.Name, out int number))
        {
            if (MenuAudioSystem.Instance != null)
            {
                MenuAudioSystem.Instance.PlayClickSound();
            }

            LoadScene(number);
        }

        if (MenuAudioSystem.Instance == null)
        {
            return;
        }

        if (reportedEvt.Name == "OnHover")
        {
            MenuAudioSystem.Instance.PlayHoverSound();
        }
    }

    private void LoadScene(int sceneNumber)
    {
        SceneManager.LoadScene(sceneNumber, LoadSceneMode.Single);
    }

    private void OnDisable()
    {
        m_riveWidget.OnRiveEventReported -= OnRiveEventReported;
    }
}
