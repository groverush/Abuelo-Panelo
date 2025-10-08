using UnityEngine;

public class GameUIController : MonoBehaviour
{
    [Tooltip("El GameObject contenedor de toda la UI específica para móvil (ej. Joystick, botones de ataque).")]
    [SerializeField]
    private GameObject mobileGameUIContainer;

    void Start()
    {
        // 1. Verificar si el detector existe y si la decisión fue congelada en el menú.
        if (InputDeviceDetector.Instance != null)
        {
            InputDeviceDetector.InputType currentType = InputDeviceDetector.Instance.CurrentInputType;
            
            // 2. La UI móvil solo debe estar activa si el input FINAL es Touch
            bool shouldShowMobileUI = currentType == InputDeviceDetector.InputType.Touch;

            if (mobileGameUIContainer != null)
            {
                // Aplicar la decisión inmediatamente al inicio de la escena.
                mobileGameUIContainer.SetActive(shouldShowMobileUI);
                Debug.Log($"[GameUIController] UI Móvil configurada: {shouldShowMobileUI}. Tipo de Input: {currentType}");
            }
        }
        else
        {
            // Fallback: Si el detector no existe (ej. cargando directamente la escena para pruebas), asumimos Touch.
            if (mobileGameUIContainer != null)
            {
                 mobileGameUIContainer.SetActive(true);
                 Debug.Log("[GameUIController] Detector no encontrado. Asumiendo Touch por seguridad.");
            }
        }
    }
}