using System;
using Rive;
using Rive.Components;
using UnityEngine;

public class RewardsController : MonoBehaviour
{
    [SerializeField] private RiveWidget m_riveWidget;

    [Tooltip("The initial number of coins")]
    [SerializeField] private float coins = 2000;

    [Tooltip("The initial number of gems")]
    [SerializeField] private float gems = 3000;

    [Tooltip("The initial number of lives")]
    [SerializeField] private float lives = 5;

    [Tooltip("The initial energy value")]
    [Range(0.0f, 100.0f)]
    [SerializeField] private float m_energyBar = 50;

    [Tooltip("The color of the energy bar")]
    [SerializeField] private UnityEngine.Color m_barColor;

    [Tooltip("The text to display on the button")]
    [SerializeField] private string m_buttonText = "Continue2";

    private ViewModelInstance viewModelInstance;
    private ViewModelInstanceNumberProperty coinNumber;
    private ViewModelInstanceNumberProperty gemsNumber;
    private ViewModelInstanceNumberProperty livesNumber;
    private ViewModelInstanceNumberProperty energyBarNumber;
    private ViewModelInstanceColorProperty energyBarColor;
    private ViewModelInstanceStringProperty buttonTextProperty;

    private float _energyBarLastValue = 0;
    private UnityEngine.Color _lastColor;


    void OnEnable()
    {
        m_riveWidget.OnWidgetStatusChanged += OnWidgetStatusChanged;
    }

    public float EnergyBar
    {
        get => m_energyBar;
        set
        {
            m_energyBar = value;
            OnSliderValueChanged(value);
        }
    }

    public UnityEngine.Color BarColor
    {
        get => m_barColor;
        set
        {
            m_barColor = value;
            OnColorChanged(value);
        }
    }


    private void Update()
    {
        if (!Mathf.Approximately(m_energyBar, _energyBarLastValue))
        {
            EnergyBar = m_energyBar;
            _energyBarLastValue = m_energyBar;
        }

        if (m_barColor != _lastColor)
        {
            BarColor = m_barColor;
            _lastColor = m_barColor;
        }
    }

    private void OnWidgetStatusChanged()
    {
        if (m_riveWidget.Status == WidgetStatus.Loaded)
        {
            StateMachine m_stateMachine = m_riveWidget.StateMachine;
            viewModelInstance = m_stateMachine.ViewModelInstance;
            coinNumber = viewModelInstance.GetNumberProperty("Coin/Item_Value");
            gemsNumber = viewModelInstance.GetNumberProperty("Gem/Item_Value");
            buttonTextProperty = viewModelInstance.GetStringProperty("Button/State_1");

            var energyBarVM = viewModelInstance.GetViewModelInstanceProperty("Energy_Bar");
            energyBarNumber = energyBarVM.GetNumberProperty("Energy_Bar");
            livesNumber = energyBarVM.GetNumberProperty("Lives");
            energyBarColor = energyBarVM.GetColorProperty("Bar_Color");

            SetInitialViewModelData();
        }
    }

    private void SetInitialViewModelData()
    {
        coinNumber.Value = coins;
        gemsNumber.Value = gems;
        energyBarNumber.Value = m_energyBar;
        livesNumber.Value = lives;
        energyBarColor.Value = BarColor;
        buttonTextProperty.Value = m_buttonText;


        coinNumber.OnValueChanged += CoinChanged;
    }

    private void CoinChanged(float newValue)
    {
        coins = newValue;
    }


    private void OnSliderValueChanged(float newValue)
    {
        energyBarNumber.Value = newValue;
    }

    private void OnColorChanged(UnityEngine.Color color)
    {
        energyBarColor.Value = color;
    }


    void OnDisable()
    {
        m_riveWidget.OnWidgetStatusChanged -= OnWidgetStatusChanged;
    }

    void OnDestroy()
    {
        coinNumber.OnValueChanged -= CoinChanged;

    }
}
