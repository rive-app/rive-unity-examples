using UnityEngine;
using Rive;
using UnityEngine.SceneManagement;
using System;
using UnityEditor;

[RequireComponent(typeof(RiveScreen))]
public class MenuController : MonoBehaviour
{
    [SerializeField]
    private int _quitValue = 6;

    private RiveScreen _riveScreen;

    private MenuAudioSystem _menuAudioSystem;

    void Start()
    {
        _riveScreen = GetComponent<RiveScreen>();
        _riveScreen.OnRiveEvent += RiveScreen_OnRiveEvent;

        _menuAudioSystem = FindObjectOfType<MenuAudioSystem>();
    }

    private void RiveScreen_OnRiveEvent(ReportedEvent reportedEvent)
    {
        if (Int32.TryParse(reportedEvent.Name, out int number))
        {
            _menuAudioSystem?.PlayClickSound();

            if (number == _quitValue)
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif

            LoadScene((int)number);
        }

        if (reportedEvent.Name == "OnHover")
        {
            _menuAudioSystem?.PlayHoverSound();
        }
    }

    private void LoadScene(int sceneNumber)
    {
        SceneManager.LoadScene(sceneNumber, LoadSceneMode.Single);
    }

    private void OnDisable()
    {
        if (_riveScreen != null)
        {
            _riveScreen.OnRiveEvent -= RiveScreen_OnRiveEvent;
        }
    }
}
