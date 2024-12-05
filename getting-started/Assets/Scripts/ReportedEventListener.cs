using System;
using Rive;
using Rive.Components;
using UnityEngine;

public class ReportedEventListener : MonoBehaviour
{
    [SerializeField] private RiveWidget m_riveWidget;

    private void OnEnable()
    {
        m_riveWidget.OnRiveEventReported += OnRiveEventReported;
    }

    private void OnRiveEventReported(ReportedEvent evt)
    {
        Debug.Log($"Event received, name: \"{evt.Name}\", secondsDelay: {evt.SecondsDelay}");

        // Access specific event properties
        if (evt.Name.StartsWith("rating"))
        {

            // For Typesafe access to properties, use the TryGet* methods
            for (uint i = 0; i < evt.PropertyCount; i++)
            {
                ReportedEvent.Property property = evt.GetProperty(i);

                if (property.Name == "rating" && property.TryGetString(out string ratingValue))
                {
                    Debug.Log($"Rating: {ratingValue}");
                }
                else if (property.Name == "message" && property.TryGetString(out string messageValue))
                {
                    Debug.Log($"Message: {messageValue}");

                }

            }

            // OR access properties directly and cast them

            /*
                        if (evt.Properties.TryGetValue("rating", out object rating))
                        {
                            float ratingValue = (float)rating;
                            Debug.Log($"Rating: {ratingValue}");
                        }

                        if (evt.Properties.TryGetValue("message", out object message))
                        {
                            string messageValue = message as string;
                            Debug.Log($"Message: {messageValue}");
                        }
                        */

        }
    }

    private void OnDisable()
    {
        m_riveWidget.OnRiveEventReported -= OnRiveEventReported;
    }
}
