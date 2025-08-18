using System;
using Rive;
using Rive.Components;
using UnityEngine;
using UnityEngine.InputSystem;

public class ImageDataBindingDemo : MonoBehaviour
{
    #region Constants
    private const string IMAGE_PROPERTY_NAME = "ball_image";
    #endregion

    #region Inspector Fields
    [SerializeField] private RiveWidget m_riveWidget;
    [SerializeField] private ImageOutOfBandAsset[] m_imageAssets;
    #endregion

    private ViewModelInstanceImageProperty m_viewModelInstanceImageProperty;
    private bool m_imagesPreloaded = false;
    private int m_imageIndex = 0;

    #region Unity Lifecycle
    private void OnEnable()
    {
        if (m_riveWidget != null)
            m_riveWidget.OnWidgetStatusChanged += OnWidgetStatusChanged;
    }

    private void OnDisable()
    {
        if (m_riveWidget != null)
            m_riveWidget.OnWidgetStatusChanged -= OnWidgetStatusChanged;
    }

    private void Update()
    {
        HandleInput();
    }

    private void OnDestroy()
    {
        UnloadImageAssets();
    }
    #endregion

    #region Initialization
    private void OnWidgetStatusChanged()
    {
        if (m_riveWidget.Status == WidgetStatus.Loaded)
        {
            PreloadImageAssets();
            InitializeImageProperty();
            SetInitialImage();
        }
    }

    private void InitializeImageProperty()
    {
        ViewModelInstance viewModelInstance = m_riveWidget.StateMachine.ViewModelInstance;
        m_viewModelInstanceImageProperty = viewModelInstance.GetImageProperty(IMAGE_PROPERTY_NAME);
    }

    private void SetInitialImage()
    {
        if (m_viewModelInstanceImageProperty != null)
        {
            ImageOutOfBandAsset initialImageAsset = GetNextImageAsset();
            m_viewModelInstanceImageProperty.Value = initialImageAsset;
        }
    }
    #endregion

    #region Input Handling
    private void HandleInput()
    {
        // Swap to next image when F key is pressed
        if (Keyboard.current[Key.F].wasPressedThisFrame)
        {
            SwapToNextImage();
        }

        // Clear the image when C key is pressed
        if (Keyboard.current[Key.C].wasPressedThisFrame)
        {
            ClearImage();
        }
    }

    private void SwapToNextImage()
    {
        if (m_viewModelInstanceImageProperty != null)
        {
            ImageOutOfBandAsset imageAsset = GetNextImageAsset();
            m_viewModelInstanceImageProperty.Value = imageAsset;
        }
    }

    private void ClearImage()
    {
        if (m_viewModelInstanceImageProperty != null)
        {
            m_viewModelInstanceImageProperty.Value = null;
        }
    }
    #endregion

    #region Image Management
    private void PreloadImageAssets()
    {
        if (m_imagesPreloaded || m_imageAssets == null)
            return;

        foreach (ImageOutOfBandAsset imageAsset in m_imageAssets)
        {
            imageAsset.Load();
        }

        m_imagesPreloaded = true;
    }

    private void UnloadImageAssets()
    {
        if (!m_imagesPreloaded || m_imageAssets == null)
            return;

        foreach (ImageOutOfBandAsset imageAsset in m_imageAssets)
        {
            imageAsset.Unload();
        }

        m_imagesPreloaded = false;
    }

    private ImageOutOfBandAsset GetNextImageAsset()
    {
        if (m_imageAssets == null || m_imageAssets.Length == 0)
            return null;

        m_imageIndex = (m_imageIndex + 1) % m_imageAssets.Length;
        return m_imageAssets[m_imageIndex];
    }
    #endregion
}