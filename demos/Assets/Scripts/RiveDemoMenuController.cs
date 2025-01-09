using Rive;
using Rive.Components;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Shows a menu with additional information about the demo.
/// </summary>
public class RiveDemoMenuController : MonoBehaviour
{
    private bool isInfoOpen = false;
    private bool m_hasSetInfo = false;

    [SerializeField] private RiveWidget m_riveWidget;

    [SerializeField]
    [TextArea(15, 20), Tooltip("Additional information for the demo")]
    private string info = "Add info here";

    void OnEnable()
    {
        m_riveWidget.OnWidgetStatusChanged += OnWidgetStatusChanged;
        m_riveWidget.OnRiveEventReported += OnRiveEventReported;
    }

    private void OnWidgetStatusChanged()
    {
        if (m_hasSetInfo)
        {
            return;
        }

        if (m_riveWidget.Status == WidgetStatus.Loaded)
        {
            m_riveWidget.Artboard.SetTextRun("info", info);
            m_hasSetInfo = true;
        }
    }


    void OnDisable()
    {
        m_riveWidget.OnWidgetStatusChanged -= OnWidgetStatusChanged;

        m_riveWidget.OnRiveEventReported -= OnRiveEventReported;
    }

    private void OnRiveEventReported(ReportedEvent reportedEvent)
    {
        if (reportedEvent.Name == "InfoPressed")
        {
            isInfoOpen = !isInfoOpen;
        }

        if (reportedEvent.Name == "BackPressed")
        {
            SceneManager.LoadScene(0, LoadSceneMode.Single);

        }
    }

    void Start()
    {
        OnWidgetStatusChanged();
    }


}
