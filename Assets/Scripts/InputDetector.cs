// using UnityEngine;
// using UnityEngine.InputSystem;

// public class InputDetector : MonoBehaviour
// {
//     // Singleton pattern para acceso global
//     public static InputDetector Instance { get; private set; }

//     // Variable de estado que indica si se usó Gamepad/Teclado
//     public bool IsUsingGamepadOrKeyboard { get; private set; } = false;

//     // Referencia al contenedor de la UI móvil del menú (para ocultarlo inmediatamente)
//     [SerializeField]
//     private GameObject mobileUIContainerMenu;

//     private void Awake()
//     {
//         // Implementación del Singleton
//         if (Instance == null)
//         {
//             Instance = this;
//             // Haz que este objeto persista entre escenas
//             DontDestroyOnLoad(gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//             return;
//         }

//         // Suscribirse a la detección de cualquier dispositivo de entrada
//         InputSystem.onAfterUpdate += CheckForDeviceChange;

//         // Si es un dispositivo táctil (móvil) por defecto, la UI debe estar visible.
//         // Si no hay dispositivos (ej. solo ratón), asumimos touch.
//         if (Application.isMobilePlatform)
//         {
//              // En móvil, la UI móvil debe estar visible inicialmente
//              if (mobileUIContainerMenu != null)
//                  mobileUIContainerMenu.SetActive(true);
//         }
//     }

//     private void OnDestroy()
//     {
//         InputSystem.onAfterUpdate -= CheckForDeviceChange;
//     }

//     private void CheckForDeviceChange()
//     {
//         InputDevice lastDevice = InputSystem.lastUsedDevice;

//         if (lastDevice != null)
//         {
//             // Comprobamos si el último dispositivo activo fue Gamepad, Teclado o Ratón
//             if (lastDevice is Gamepad || lastDevice is Keyboard || lastDevice is Mouse)
//             {
//                 IsUsingGamepadOrKeyboard = true;
//             }
//             // Si fue Touchscreen, Touchpad, etc., no usamos Gamepad/Teclado
//             else if (lastDevice is Touchscreen || lastDevice is Touchpad)
//             {
//                 IsUsingGamepadOrKeyboard = false;
//             }

//             // Aplicar el ocultamiento en el menú
//             if (mobileUIContainerMenu != null)
//             {
//                 // Si es Gamepad/Teclado, desactiva la UI móvil (false)
//                 // Si es Táctil, mantén la UI móvil activa (true)
//                 mobileUIContainerMenu.SetActive(!IsUsingGamepadOrKeyboard);
//             }
//         }
//     }
// }