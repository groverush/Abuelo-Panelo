using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Referencias UI")]
    [SerializeField] private TextMeshProUGUI textoCanaJugador;
    [SerializeField] private TextMeshProUGUI textoCanaBurro;
    [SerializeField] private TextMeshProUGUI textoMaquina;
    [SerializeField] private TextMeshProUGUI textoEntregar;
    [SerializeField] private TextMeshProUGUI textoEntregarJarabe;
    [SerializeField] private TextMeshProUGUI textoRecoger;
    [SerializeField] private TextMeshProUGUI textoLlenado;
    [SerializeField] private TextMeshProUGUI contadorProcesamientoTexto;
    [SerializeField] private TextMeshProUGUI porcentajeBarrilTexto;
    [SerializeField] private TextMeshProUGUI textoProgresoJarabe;
    [SerializeField] private GameObject panelVictoria;
    [SerializeField] private TextMeshProUGUI textoVictoria;
    [SerializeField] private GameObject panelDerrota;
    [SerializeField] private TextMeshProUGUI textoDerrota;
    [SerializeField] private GameObject panelPausa;
    [SerializeField] private TextMeshProUGUI textoPausa;
    [SerializeField] private Button continuarButton;
    [SerializeField] private Button reiniciarButton;

    [Header("UI Botellas de Vida")]
    [SerializeField] private RawImage[] botellasRawImages;

    [Header("UI Móvil")]
    [SerializeField] private GameObject mobileUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Suscribirse para ocultar/mostrar la UI de móvil
        if (InputDeviceDetector.Instance != null)
        {
            InputDeviceDetector.Instance.OnInputTypeChanged += OnInputChanged;
            
            // Inicializa la visibilidad de la UI móvil al cargar la escena
            OnInputChanged(InputDeviceDetector.Instance.CurrentInputType);
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse al destruir
        if (InputDeviceDetector.Instance != null)
            InputDeviceDetector.Instance.OnInputTypeChanged -= OnInputChanged;
    }

    // Método para gestionar la visibilidad de la UI móvil según el input
    private void OnInputChanged(InputDeviceDetector.InputType inputType)
    {
        // La UI móvil solo debe estar ACTIVA si el tipo de input es Touch
        bool isTouch = inputType == InputDeviceDetector.InputType.Touch;
        if (mobileUI != null)
        {
            mobileUI.SetActive(isTouch);
        }
    }
    
    // ====================================================================
    // LÓGICA DE SELECCIÓN DE BOTONES CONDICIONAL
    // ====================================================================
    /// <summary>
    /// Selecciona el primer botón del panel solo si el input no es táctil.
    /// Esto es crucial para la navegabilidad con Gamepad/Teclado en paneles de pausa.
    /// </summary>
    private void SetInitialButtonSelection(GameObject buttonToSelect)
    {
        // Limpia cualquier cosa que estuviera seleccionada
        EventSystem.current.SetSelectedGameObject(null);
        
        // Determina si estamos usando un dispositivo que requiere enfoque (Gamepad/Teclado/Mouse)
        bool needsSelection = InputDeviceDetector.Instance != null && 
                              InputDeviceDetector.Instance.CurrentInputType != InputDeviceDetector.InputType.Touch;

        if (needsSelection && buttonToSelect != null)
        {
            // Forzar la selección del botón para Gamepad/Teclado
            EventSystem.current.SetSelectedGameObject(buttonToSelect);
        }
    }

    // ====================================================================
    // MÉTODOS DE ACTUALIZACIÓN DE UI EN TIEMPO REAL
    // ====================================================================

    public void ActualizarCanaJugador(int actual, int maximo)
    {
        if (textoCanaJugador != null)
            textoCanaJugador.text = $" {actual} / {maximo}";
    }

    public void ActualizarCanaBurro(int actual, int maximo)
    {
        if (textoCanaBurro != null)
            textoCanaBurro.text = $" {actual} / {maximo}";
    }

    public void MostrarTextoEntregar(bool mostrar, string texto)
    {
        if (textoEntregar != null)
            textoEntregar.gameObject.SetActive(mostrar);
        textoEntregar.text = texto;
    }

    public void MostrarTextoRecoger(bool mostrar, string texto)
    {
        if (textoRecoger != null)
            textoRecoger.gameObject.SetActive(mostrar);
        textoRecoger.text = texto;
    }

    public void MostrarTextoEntregarJarabe(bool mostrar, string texto)
    {
        if (textoEntregarJarabe != null)
            textoEntregarJarabe.gameObject.SetActive(mostrar);
        textoEntregarJarabe.text = texto;
    }

    public void MostrarTextoLlenado(bool mostrar, string texto)
    {
        if (textoLlenado != null)
            textoLlenado.gameObject.SetActive(mostrar);
        textoLlenado.text = texto;
    }

    public void ActualizarCanaMaquina(int actual, int maximo)
    {
        if (textoMaquina != null)
            textoMaquina.text = $"Máquina: {actual} / {maximo}";
    }


    public void ActualizarContadorProcesamiento(int segundos)
    {
        if (contadorProcesamientoTexto != null)
            contadorProcesamientoTexto.text = $"Procesando: {segundos}s";
    }

    public void ActualizarPorcentajeBarril(int actual, int maximo)
    {
        if (porcentajeBarrilTexto != null)
        {
            float porcentaje = ((float)actual / maximo) * 100f;
            porcentajeBarrilTexto.text = $"Barril: {porcentaje:F0}%";
        }
    }

    public void MostrarContadorProcesamiento(bool mostrar)
    {
        if (contadorProcesamientoTexto != null)
            contadorProcesamientoTexto.gameObject.SetActive(mostrar);
    }

    public void MostrarPorcentajeBarril(bool mostrar)
    {
        if (porcentajeBarrilTexto != null)
            porcentajeBarrilTexto.gameObject.SetActive(mostrar);
    }

    public void ActualizarProgresoJarabe(int actual, int total)
    {
        if (textoProgresoJarabe != null)
            textoProgresoJarabe.text = $"{actual} / {total}";
    }

    public void ActualizarBotellasRotas(int botellasRotasActuales, int maxBotellas)
    {
        // Itera a través de las imágenes de las botellas de "vida"
        for (int i = 0; i < botellasRawImages.Length; i++)
        {
            // Muestra la botella si su índice es menor al número de botellas restantes (max - rotas)
            botellasRawImages[i].gameObject.SetActive(i < maxBotellas - botellasRotasActuales);
        }
    }
    
    // ====================================================================
    // MÉTODOS DE PANELES DE JUEGO (PAUSA, VICTORIA, DERROTA)
    // ====================================================================

    public void MostrarVictoria(string mensaje)
    {
        if (panelVictoria != null)
            panelVictoria.SetActive(true);

        if (textoVictoria != null)
            textoVictoria.text = mensaje;
        
        // Usar el nuevo método para la selección condicional
        SetInitialButtonSelection(reiniciarButton.gameObject);
    }

    public void MostrarDerrota(string mensaje)
    {
        if (panelDerrota != null)
            panelDerrota.SetActive(true);

        if (textoDerrota != null)
            textoDerrota.text = mensaje;
        
        // Usar el nuevo método para la selección condicional
        SetInitialButtonSelection(reiniciarButton.gameObject);
    }

    public void MostrarPausa(bool isPaused, string mensaje)
    {
        if (isPaused)
        {
            if (panelPausa != null)
                panelPausa.SetActive(true);

            if (textoPausa != null)
                textoPausa.text = mensaje;

            // Usar el nuevo método para la selección condicional
            SetInitialButtonSelection(continuarButton.gameObject);
        }
        else
        {
            // Ocultar panel y limpiar la selección
            panelPausa.SetActive(false);            
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}