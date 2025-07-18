using System;
using Rive;
using Rive.Components;
using UnityEngine;
using UnityEngine.InputSystem;

public class ImageDataBindingDemo : MonoBehaviour
{
    [SerializeField] private RiveWidget m_riveWidget;

    [SerializeField] private ImageOutOfBandAsset[] m_imageAssets;

    private ViewModelInstanceImageProperty m_viewModelInstanceImageProperty;

    private bool m_imagesPreloaded = false;
    private int m_imageIndex = 0;

    private void OnEnable()
    {
        m_riveWidget.OnWidgetStatusChanged += OnWidgetStatusChanged;
    }

    private void OnDisable()
    {
        m_riveWidget.OnWidgetStatusChanged -= OnWidgetStatusChanged;
    }

    private void OnWidgetStatusChanged()
    {
        if (m_riveWidget.Status == WidgetStatus.Loaded)
        {
            // Load the image assets into memory
            PreloadImageAssets();

            ViewModelInstance viewModelInstance = m_riveWidget.StateMachine.ViewModelInstance;
            m_viewModelInstanceImageProperty = viewModelInstance.GetImageProperty("ball_image");

            // Set the initial image
            ImageOutOfBandAsset initialImageAsset = GetNextImageAsset();
            m_viewModelInstanceImageProperty.Value = initialImageAsset;

        }
    }

    private void PreloadImageAssets()
    {
        if (m_imagesPreloaded) return;
        foreach (ImageOutOfBandAsset imageAsset in m_imageAssets)
        {
            imageAsset.Load();
        }

        m_imagesPreloaded = true;
    }

    private void UnloadImageAssets()
    {
        if (!m_imagesPreloaded) return;
        foreach (ImageOutOfBandAsset imageAsset in m_imageAssets)
        {
            imageAsset.Unload();
        }
        m_imagesPreloaded = false;
    }

    private ImageOutOfBandAsset GetNextImageAsset()
    {
        m_imageIndex++;
        if (m_imageIndex >= m_imageAssets.Length)
        {
            m_imageIndex = 0;
        }
        return m_imageAssets[m_imageIndex];
    }

    void Update()
    {
        // Check if F key is pressed to swap the image
        if (Keyboard.current[Key.F].wasPressedThisFrame)
        {
            ImageOutOfBandAsset imageAsset = GetNextImageAsset();
            m_viewModelInstanceImageProperty.Value = imageAsset;
        }

        // Clear the image if C key is pressed
        if (Keyboard.current[Key.C].wasPressedThisFrame)
        {
            m_viewModelInstanceImageProperty.Value = null;
        }
    }

    private void OnDestroy()
    {
        // We need to unload the image assets to free up resources
        UnloadImageAssets();
    }
}
