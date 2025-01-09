using Rive;
using UnityEngine;

//! An example implementation to access Rive Events.

[RequireComponent(typeof(RiveScreen))]
public class RiveEvents : MonoBehaviour
{
    private RiveScreen m_riveScreen;
    void Start()
    {
        m_riveScreen = GetComponent<RiveScreen>();
        m_riveScreen.OnRiveEvent += RiveScreen_OnRiveEvent;
    }

    private void RiveScreen_OnRiveEvent(ReportedEvent reportedEvent)
    {
        Debug.Log($"Event received, name: \"{reportedEvent.Name}\", secondsDelay: {reportedEvent.SecondsDelay}");

        // Access specific event properties
        if (reportedEvent.Name.StartsWith("rating"))
        {
            var rating = reportedEvent["rating"];
            var message = reportedEvent["message"];
            Debug.Log($"Rating: {rating}");
            Debug.Log($"Message: {message}");
        }
    }

    private void OnDisable()
    {
        m_riveScreen.OnRiveEvent -= RiveScreen_OnRiveEvent;
    }

}
