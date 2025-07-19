using Rive;
using Rive.Components;
using UnityEngine;

public class ListDatabindingController : MonoBehaviour
{
    #region Constants
    private const string LIST_PROPERTY_NAME = "menu";
    private const string VIEW_MODEL_NAME = "listItem";
    private const string LABEL_PROPERTY = "label";
    private const string FONT_ICON_PROPERTY = "fontIcon";
    private const string COLOR_PROPERTY = "hoverColor";
    #endregion

    #region Inspector Fields
    [SerializeField] private RiveWidget m_riveWidget;

    [Header("Add Item")]
    [SerializeField] private TMPro.TMP_InputField m_addItemInputField;
    [SerializeField] private UnityEngine.UI.Button m_addItemButton;

    [Header("Swap Items")]
    [SerializeField] private TMPro.TMP_InputField m_swapItemIndex1InputField;
    [SerializeField] private TMPro.TMP_InputField m_swapItemIndex2InputField;
    [SerializeField] private UnityEngine.UI.Button m_swapItemButton;

    [Header("Remove Item")]
    [SerializeField] private TMPro.TMP_InputField m_removeItemIndexInputField;
    [SerializeField] private UnityEngine.UI.Button m_removeItemButton;
    #endregion

    private ViewModelInstanceListProperty m_listProperty; // The list property that holds the todo items
    private ViewModel m_todoItemViewModel; // The ViewModel for the list items, used to create new items

    #region Unity Lifecycle
    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }
    #endregion

    #region Event Subscription
    private void SubscribeToEvents()
    {
        if (m_riveWidget != null)
            m_riveWidget.OnWidgetStatusChanged += OnWidgetStatusChanged;

        if (m_addItemButton != null)
            m_addItemButton.onClick.AddListener(OnItemAddButtonClicked);

        if (m_swapItemButton != null)
            m_swapItemButton.onClick.AddListener(OnSwapItemButtonClicked);

        if (m_removeItemButton != null)
            m_removeItemButton.onClick.AddListener(OnRemoveItemButtonClicked);
    }

    private void UnsubscribeFromEvents()
    {
        if (m_riveWidget != null)
            m_riveWidget.OnWidgetStatusChanged -= OnWidgetStatusChanged;

        if (m_addItemButton != null)
            m_addItemButton.onClick.RemoveListener(OnItemAddButtonClicked);

        if (m_swapItemButton != null)
            m_swapItemButton.onClick.RemoveListener(OnSwapItemButtonClicked);

        if (m_removeItemButton != null)
            m_removeItemButton.onClick.RemoveListener(OnRemoveItemButtonClicked);
    }
    #endregion

    #region Initialization
    private void OnWidgetStatusChanged()
    {
        if (m_riveWidget.Status == WidgetStatus.Loaded)
        {
            var viewModelInstance = m_riveWidget.StateMachine.ViewModelInstance;
            m_listProperty = viewModelInstance.GetListProperty(LIST_PROPERTY_NAME);
            m_todoItemViewModel = m_riveWidget.File.GetViewModelByName(VIEW_MODEL_NAME);
        }
    }
    #endregion

    #region Button Event Handlers
    private void OnItemAddButtonClicked()
    {
        string itemName = m_addItemInputField.text;
        AddItemToList(itemName);
    }

    private void OnSwapItemButtonClicked()
    {
        if (int.TryParse(m_swapItemIndex1InputField.text, out int index1) &&
            int.TryParse(m_swapItemIndex2InputField.text, out int index2))
        {
            SwapItemInList(index1, index2);
        }
        else
        {
            Debug.LogError("Invalid indices for swapping items.");
        }
    }

    private void OnRemoveItemButtonClicked()
    {
        if (int.TryParse(m_removeItemIndexInputField.text, out int index))
        {
            RemoveItemFromList(index);
        }
        else
        {
            Debug.LogError("Invalid index for removing item.");
        }
    }
    #endregion

    #region List Operations
    private void AddItemToList(string itemName)
    {
        if (m_listProperty == null || m_todoItemViewModel == null)
            return;

        var newItem = m_todoItemViewModel.CreateInstance();
        SetupNewItem(newItem, itemName);
        m_listProperty.Add(newItem);
    }

    private void SetupNewItem(ViewModelInstance newItem, string itemName)
    {
        var labelProperty = newItem.GetStringProperty(LABEL_PROPERTY);
        var fontIconProperty = newItem.GetStringProperty(FONT_ICON_PROPERTY);
        var colorProperty = newItem.GetColorProperty(COLOR_PROPERTY);

        if (labelProperty != null)
            labelProperty.Value = itemName;

        if (fontIconProperty != null)
            fontIconProperty.Value = "";

        if (colorProperty != null)
            colorProperty.Value = new UnityEngine.Color(0.937f, 0.937f, 0.937f, 1f);
    }

    private void SwapItemInList(int index1, int index2)
    {
        if (m_listProperty != null)
            m_listProperty.Swap(index1, index2);
    }

    private void RemoveItemFromList(int index)
    {
        if (m_listProperty != null)
            m_listProperty.RemoveAt(index);
    }
    #endregion
}