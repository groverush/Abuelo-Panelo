using UnityEngine;
using UnityEngine.InputSystem;
using System;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Controls;

public class InputDeviceDetector : MonoBehaviour
{
    public static InputDeviceDetector Instance;
    public enum InputType { KeyboardMouse, Controller, Touch }
    public InputType CurrentInputType { get; private set; }
    public event Action<InputType> OnInputTypeChanged;

    private bool detectionActive = true; // Controla si el script está escuchando la primera pulsación

    private void Awake()
    {
        // Implementación del Singleton (asegura que solo haya una instancia)
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 1. FORZAR EL TIPO DE INPUT INICIAL A TOUCH (La UI móvil está visible por defecto)
        SetInputType(InputType.Touch);
        detectionActive = true; 
    }

    private void OnEnable()
    {
        // Suscribirse al evento de input de bajo nivel para detectar la primera pulsación
        InputSystem.onEvent += OnInputEvent;
        // Si el objeto se reactiva, reactivar la detección
        if (CurrentInputType == InputType.Touch)
        {
             detectionActive = true; 
        }
    }

    private void OnDisable()
    {
        InputSystem.onEvent -= OnInputEvent;
    }
    
    // NUEVO MÉTODO: Llamado desde MainMenu.cs para congelar la decisión
    public void StopDetection()
    {
        detectionActive = false;
        // Opcional: Desuscribirse para ahorrar rendimiento, aunque la bandera es suficiente.
        // InputSystem.onEvent -= OnInputEvent; 
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        // Dejar de procesar si ya se tomó una decisión (por ejemplo, saliendo del menú)
        if (!detectionActive) return; 
        
        // Solo escuchamos si el estado actual es Touch (esperando una anulación de PC/Gamepad)
        if (CurrentInputType == InputType.Touch)
        {
            if (eventPtr.IsA<StateEvent>() || eventPtr.IsA<DeltaStateEvent>())
            {
                if (device is Keyboard || device is Mouse || device is Gamepad)
                {
                    if (WasAnyButtonPressed(device))
                    {
                        // 2. Cambiar el tipo de input al detectar PC/Gamepad
                        if (device is Keyboard || device is Mouse)
                        {
                            SetInputType(InputType.KeyboardMouse);
                        }
                        else if (device is Gamepad)
                        {
                            SetInputType(InputType.Controller);
                        }
                        
                        // 3. Congelamos la detección inmediatamente (incluso en el menú)
                        StopDetection(); 
                    }
                }
            }
        }
    }
    
    private bool WasAnyButtonPressed(InputDevice device)
    {
        // Chequeo robusto de si hubo una pulsación o movimiento de joystick/ratón
        foreach (var control in device.allControls)
        {
            // Chequea botones (teclas, botones de gamepad/ratón)
            if (control is ButtonControl button && button.wasPressedThisFrame)
            {
                return true;
            }
            // Chequea sticks (movimiento significativo)
            if (control is StickControl stick && stick.ReadValue().sqrMagnitude > 0.01f)
            {
                return true;
            }
        }
        return false;
    }


    private void SetInputType(InputType newType)
    {
        if (newType == CurrentInputType) return;
        CurrentInputType = newType;
        OnInputTypeChanged?.Invoke(newType);
        Debug.Log($"Input cambiado a: {newType}");
    }
}