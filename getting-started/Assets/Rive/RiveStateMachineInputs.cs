using System;
using System.Collections.Generic;
using Rive;
using UnityEditor;
using UnityEngine;

//! An example implementation to get/set Rive State Machine Inputs.
[RequireComponent(typeof(RiveScreen))]
public class RiveStateMachineInputs : MonoBehaviour
{
    [Serializable]
    public struct SMITriggerDescriptor
    {
        public string name;
        public bool trigger;
        public SMITrigger reference;

        public SMITriggerDescriptor(string name, SMITrigger reference)
        {
            this.name = name;
            this.reference = reference;
            trigger = false;
        }
    }

    [Serializable]
    public struct SMIBoolDescriptor
    {
        public string name;
        public bool value;
        public SMIBool reference;

        public SMIBoolDescriptor(string name, bool value, SMIBool reference)
        {
            this.name = name;
            this.value = value;
            this.reference = reference;
        }
    }

    [Serializable]
    public class SMINumberDescriptor
    {
        public string name;
        public float value;
        public SMINumber reference;

        public SMINumberDescriptor(string name, float value, SMINumber reference)
        {
            this.name = name;
            this.value = value;
            this.reference = reference;
        }
    }

    private StateMachine m_riveStateMachine;

    [SerializeField]
    public List<SMITriggerDescriptor> triggers;
    [SerializeField]
    public List<SMIBoolDescriptor> booleans;
    [SerializeField]
    public List<SMINumberDescriptor> numbers;

    private void Start()
    {
        var riveScreen = GetComponent<RiveScreen>();
        m_riveStateMachine = riveScreen.stateMachine;

        booleans = new List<SMIBoolDescriptor>();
        triggers = new List<SMITriggerDescriptor>();
        numbers = new List<SMINumberDescriptor>();

        var inputs = m_riveStateMachine.Inputs();
        foreach (var input in inputs)
        {
            switch (input)
            {
                case SMITrigger smiTrigger:
                    {
                        var descriptor = new SMITriggerDescriptor(smiTrigger.Name, smiTrigger);
                        triggers.Add(descriptor);
                        break;
                    }
                case SMIBool smiBool:
                    {
                        var descriptor = new SMIBoolDescriptor(smiBool.Name, smiBool.Value, smiBool);
                        booleans.Add(descriptor);
                        break;
                    }
                case SMINumber smiNumber:
                    {
                        var descriptor = new SMINumberDescriptor(smiNumber.Name, smiNumber.Value, smiNumber);
                        numbers.Add(descriptor);
                        break;
                    }
            }
        }
    }

    private void OnValidate()
    {
        // State machine triggers
        var triggerDidChange = false;
        foreach (var inspectorInput in triggers)
        {
            if (inspectorInput.reference == null) continue;
            if (inspectorInput.trigger == true)
            {
                inspectorInput.reference.Fire();
                triggerDidChange = true;
            }
        }

        if (triggerDidChange)
        {
            var updatedTriggers = new List<SMITriggerDescriptor>();
            foreach (var inspectorInput in triggers)
            {
                updatedTriggers.Add(new SMITriggerDescriptor(inspectorInput.name, inspectorInput.reference));
            }
            triggers = updatedTriggers;
        }

        // State machine booleans
        foreach (var inspectorInput in booleans)
        {
            if (inspectorInput.reference == null) continue;
            if (inspectorInput.value == inspectorInput.reference.Value) continue;
            inspectorInput.reference.Value = inspectorInput.value;
        }

        // State machine numbers
        foreach (var inspectorInput in numbers)
        {
            if (inspectorInput.reference == null) continue;
            if (inspectorInput.value == inspectorInput.reference.Value) continue;
            inspectorInput.reference.Value = inspectorInput.value;
        }
    }

}

#if UNITY_EDITOR
// Creates a custom Label on the inspector.
// This also solves this issue when exiting play mode: https://forum.unity.com/threads/nullreferenceexception-serializedobject-of-serializedproperty-has-been-disposed.1431907/
[CustomEditor(typeof(RiveStateMachineInputs))]
public class TestOnInspector : Editor
{
    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
        {
           base.OnInspectorGUI();
        }
        GUILayout.Label ("Enter Play Mode to interact with available state machine inputs");
    }
}
#endif