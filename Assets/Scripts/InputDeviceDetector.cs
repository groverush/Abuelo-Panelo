using UnityEngine;
using System;

public class InputDeviceDetector : MonoBehaviour
{
    public static InputDeviceDetector Instance;

    public enum InputType { KeyboardMouse, Controller, Touch }
    public InputType CurrentInputType { get; private set; }

    public event Action<InputType> OnInputTypeChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        DetectInputDevice();
    }

    private void DetectInputDevice()
    {
        if (Input.touchCount > 0)
            SetInputType(InputType.Touch);
        else if (Input.GetJoystickNames().Length > 0 && !string.IsNullOrEmpty(Input.GetJoystickNames()[0]))
            SetInputType(InputType.Controller);
        else if (Input.anyKeyDown)
            SetInputType(InputType.KeyboardMouse);
    }

    private void SetInputType(InputType newType)
    {
        if (newType == CurrentInputType) return;

        CurrentInputType = newType;
        OnInputTypeChanged?.Invoke(newType);
        Debug.Log($"Input cambiado a: {newType}");
    }
}
