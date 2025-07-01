using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Sonidos")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip victoriaAudioClip;
    [SerializeField] private AudioClip derrotaAudioClip;
    [SerializeField] private AudioSource camAudioSource; // Fuente de audio para la música de fondo
    [SerializeField] private AudioSource burritoAudioSource; // Fuente de audio para la música de fondo
    [SerializeField] TextMeshProUGUI timeLeftText;
    float currCountdownValue;
    public bool isGameActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Si necesitas que persista entre escenas, déjalo activado:
        // DontDestroyOnLoad(gameObject);

        // Si NO necesitas persistencia, coméntalo para evitar problemas de referencia:
        // DontDestroyOnLoad(gameObject);

        // Registrar callback para cuando se cargue una escena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        // Inicializar referencias de audio
        StartGame();
    }
    public IEnumerator StartCountdown(float countdownValue)
    {
        currCountdownValue = countdownValue;
        while (currCountdownValue >= 0 && isGameActive)
        {
            timeLeftText.text = "Tiempo de entrega: " + currCountdownValue.ToString("0");
            yield return new WaitForSeconds(1.0f);
            currCountdownValue--;
            if (currCountdownValue == 0 && isGameActive)
            {
                audioSource.PlayOneShot(derrotaAudioClip);
                PerderJuego(); // Llama a PerderJuego cuando el tiempo se agote

            }
        }
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reasignar camAudioSource si la cámara cambia entre escenas
        Camera cam = Camera.main;
        if (cam != null)
        {
            camAudioSource = cam.GetComponent<AudioSource>();
        }

        // Reasignar referencias en UIManager si es necesario
        // if (UIManager.Instance != null)
        // {
        //     UIManager.Instance.ReasignarReferencias();
        // }
    }
    public void StartGame()
    {

        isGameActive = true;
        timeLeftText.gameObject.SetActive(true);
        StartCoroutine("StartCountdown", 240);

    }

    public void GanarJuego()
    {
        isGameActive = false; // Detiene el juego
        Debug.Log("🎉 Has ganado el juego.");
        UIManager.Instance.MostrarVictoria("¡VICTORIA!");
        burritoAudioSource.Stop(); // Detiene el audio del burrito si está sonando

        if (camAudioSource != null) camAudioSource.Stop();
        audioSource.PlayOneShot(victoriaAudioClip);
        Time.timeScale = 0;
    }

    public void PerderJuego()
    {
        Debug.Log("💀 Has perdido el juego.");
        UIManager.Instance.MostrarDerrota("¡DERROTA!");
        if (camAudioSource != null) camAudioSource.Stop();
        burritoAudioSource.Stop(); // Detiene el audio del burrito si está sonando
        audioSource.PlayOneShot(derrotaAudioClip);
        isGameActive = false; // Detiene el juego
        Time.timeScale = 0;
    }

    public void ReiniciarJuego()
    {
        Debug.Log("Reiniciando juego...");

        Time.timeScale = 1; // Reanuda el tiempo antes de recargar la escena

        // Si GameManager NO es persistente, esto es suficiente:
        SceneManager.LoadScene("MainScene");

        // Si GameManager es persistente (DontDestroyOnLoad) y persiste entre escenas,
        // asegúrate de reconfigurar UIManager en OnSceneLoaded como se muestra arriba.
    }

    public void PausarJuego()
    {
        Debug.Log("Juego pausado.");
        burritoAudioSource.Stop(); // Detiene el audio del burrito si está sonando

        Time.timeScale = 0;
        UIManager.Instance.MostrarPausa(true, "Juego pausado");
    }

    public void Continuar()
    {

        Debug.Log("Continuando juego...");
        Time.timeScale = 1;
        UIManager.Instance.MostrarPausa(false, "");
    }

    public void IrAlMenuPrincipal()
    {
        Debug.Log("Volviendo al menú principal...");
        Time.timeScale = 1; // Asegúrate de reanudar el tiempo antes de cambiar de escena
        SceneManager.LoadScene("MenuPrincipal");
    }
}
