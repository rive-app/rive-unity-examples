using System.Collections;
using System.Collections.Generic;
using Rive.Components;
using UnityEngine;

namespace Demos.ShooterHud
{
    public class UseCurrentResolution : MonoBehaviour
    {

        [SerializeField] private RivePanel m_rivePanel;

        [SerializeField] private int m_aspectRatioWidth = 16;
        [SerializeField] private int m_aspectRatioHeight = 9;

        void OnEnable()
        {
            // The largest size of the screen should be the largest size of the aspect ratio
            Vector2Int resolution = new Vector2Int(Screen.width, Screen.height);
            float aspectRatio = (float)resolution.x / resolution.y;

            if (aspectRatio > m_aspectRatioWidth / m_aspectRatioHeight)
            {
                resolution.x = (int)(resolution.y * m_aspectRatioWidth / m_aspectRatioHeight);
            }
            else
            {
                resolution.y = (int)(resolution.x * m_aspectRatioHeight / m_aspectRatioWidth);
            }

            m_rivePanel.SetDimensions(resolution);
        }

    }
}