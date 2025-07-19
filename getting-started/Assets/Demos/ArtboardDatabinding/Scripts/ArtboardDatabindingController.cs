using System.Collections.Generic;
using Rive;
using Rive.Components;
using UnityEngine;

public class ArtboardDatabindingController : MonoBehaviour
{
    #region Constants
    private const string MAIN_ARTBOARD_ICON_PROPERTY = "icon";
    private const string DEFAULT_TRAVEL_PACK_ICON = "map";
    private const string DEFAULT_WEBPACK_ICON = "download";
    #endregion

    #region Inspector Fields
    [Header("Rive Components")]
    [SerializeField] private RiveWidget m_riveWidget;
    [SerializeField] private Asset m_webPackRiveAsset;

    [Header("UI Controls")]
    [SerializeField] private TMPro.TMP_Dropdown m_travelPackIconDropdown;
    [SerializeField] private TMPro.TMP_Dropdown m_webPackIconDropdown;
    #endregion

    // Rive file references
    private File m_webPackFile;
    private ViewModelInstanceArtboardProperty m_mainArtboardIconProperty;

    // Current selections
    private string m_currentTravelPackIcon = DEFAULT_TRAVEL_PACK_ICON;
    private string m_currentWebPackIcon = DEFAULT_WEBPACK_ICON;

    // Bindable artboards
    private BindableArtboard m_selectedTravelPackBindableArtboard;
    private BindableArtboard m_selectedWebPackBindableArtboard;

    // Icon collections
    private readonly List<string> m_travelPackIconNames = new List<string>
    {
        "map", "car", "gas", "compass", "walk", "food", "GPS", "coffee"
    };

    private readonly List<string> m_webPackIconNames = new List<string>
    {
        "download", "refresh", "lock", "wifi", "email", "www"
    };

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

    private void OnDestroy()
    {
        UnsubscribeFromDropdowns();
        DisposeWebPackFile();
    }
    #endregion

    #region Initialization
    private void OnWidgetStatusChanged()
    {
        if (m_riveWidget.Status == WidgetStatus.Loaded)
        {
            LoadWebPackFile();
            InitializeViewModelProperty();
            InitializeDropdowns();
        }
    }

    private void InitializeViewModelProperty()
    {
        ViewModelInstance viewModelInstance = m_riveWidget.StateMachine.ViewModelInstance;
        m_mainArtboardIconProperty = viewModelInstance.GetArtboardProperty(MAIN_ARTBOARD_ICON_PROPERTY);

        if (m_mainArtboardIconProperty == null)
        {
            Debug.LogError($"Main artboard property '{MAIN_ARTBOARD_ICON_PROPERTY}' not found in the ViewModelInstance.");
        }
    }

    private void InitializeDropdowns()
    {
        SetupDropdown(m_travelPackIconDropdown, m_travelPackIconNames, OnTravelPackIconChanged);
        SetupDropdown(m_webPackIconDropdown, m_webPackIconNames, OnWebPackIconChanged);

        // Set initial selections
        SetTravelPackIcon(m_currentTravelPackIcon);
        SetWebPackIcon(m_currentWebPackIcon);
    }

    private void SetupDropdown(TMPro.TMP_Dropdown dropdown, List<string> options, UnityEngine.Events.UnityAction<int> callback)
    {
        if (dropdown == null) return;

        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        dropdown.onValueChanged.AddListener(callback);
    }
    #endregion

    #region Event Handlers
    private void OnTravelPackIconChanged(int index)
    {
        if (IsValidIndex(index, m_travelPackIconNames))
        {
            m_currentTravelPackIcon = m_travelPackIconNames[index];
            SetTravelPackIcon(m_currentTravelPackIcon);
        }
    }

    private void OnWebPackIconChanged(int index)
    {
        if (IsValidIndex(index, m_webPackIconNames))
        {
            m_currentWebPackIcon = m_webPackIconNames[index];
            SetWebPackIcon(m_currentWebPackIcon);
        }
    }
    #endregion

    #region Core Functionality
    private void SetTravelPackIcon(string iconName)
    {
        if (!m_travelPackIconNames.Contains(iconName) || m_mainArtboardIconProperty == null)
            return;

        m_selectedTravelPackBindableArtboard = m_riveWidget.File.BindableArtboard(iconName);

        if (m_selectedTravelPackBindableArtboard == null)
        {
            Debug.LogError($"Bindable artboard '{iconName}' not found in the main Rive file.");
            return;
        }

        m_mainArtboardIconProperty.Value = m_selectedTravelPackBindableArtboard;
    }

    private void SetWebPackIcon(string iconName)
    {
        if (!m_webPackIconNames.Contains(iconName) || m_mainArtboardIconProperty == null)
            return;

        m_selectedWebPackBindableArtboard = m_webPackFile?.BindableArtboard(iconName);

        if (m_selectedWebPackBindableArtboard == null)
        {
            Debug.LogError($"Bindable artboard '{iconName}' not found in the WebPack Rive file.");
            return;
        }

        m_mainArtboardIconProperty.Value = m_selectedWebPackBindableArtboard;
    }
    #endregion

    #region Utility Methods
    private void LoadWebPackFile()
    {
        if (m_webPackFile == null && m_webPackRiveAsset != null)
        {
            m_webPackFile = Rive.File.Load(m_webPackRiveAsset);
        }
    }

    private bool IsValidIndex(int index, List<string> collection)
    {
        if (index >= 0 && index < collection.Count)
            return true;

        Debug.LogError($"Selected index {index} is out of bounds for collection with {collection.Count} items.");
        return false;
    }

    private void UnsubscribeFromDropdowns()
    {
        if (m_travelPackIconDropdown != null)
            m_travelPackIconDropdown.onValueChanged.RemoveListener(OnTravelPackIconChanged);

        if (m_webPackIconDropdown != null)
            m_webPackIconDropdown.onValueChanged.RemoveListener(OnWebPackIconChanged);
    }

    private void DisposeWebPackFile()
    {
        if (m_webPackFile != null)
        {
            m_webPackFile.Dispose();
            m_webPackFile = null;
        }
    }
    #endregion
}