using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class InputDeviceDetector : MonoBehaviour
{
    public static InputDeviceDetector Instance { get; private set; }

    public enum InputType { KeyboardMouse, Gamepad, Touch }

    public InputType CurrentInputType { get; private set; }

    public delegate void InputTypeChanged(InputType inputType);
    public event InputTypeChanged OnInputTypeChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Detectar dispositivo cuando se conecta o desconecta
        InputSystem.onDeviceChange += OnDeviceChange;
        InputSystem.onEvent += OnInputEvent;

        // Comprobar tipo inicial
        CheckCurrentInput();
    }

    private void OnDestroy()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
        InputSystem.onEvent -= OnInputEvent;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected)
            CheckCurrentInput();
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (eventPtr.IsA<StateEvent>() || eventPtr.IsA<DeltaStateEvent>())
            CheckCurrentInput(device);
    }

    private void CheckCurrentInput(InputDevice device = null)
    {
        InputType detectedType = InputType.KeyboardMouse;

        if (device == null)
            device = InputSystem.devices.Count > 0 ? InputSystem.devices[0] : null;

        if (device is Gamepad)
            detectedType = InputType.Gamepad;
        else if (device is Pointer || device is Keyboard)
            detectedType = InputType.KeyboardMouse;
        else if (device is Touchscreen)
            detectedType = InputType.Touch;

        if (detectedType != CurrentInputType)
        {
            CurrentInputType = detectedType;
            OnInputTypeChanged?.Invoke(CurrentInputType);
        }
    }
}
