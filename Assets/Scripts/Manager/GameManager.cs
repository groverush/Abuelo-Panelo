using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Sonidos")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip victoriaAudioClip;
    [SerializeField] private AudioClip derrotaAudioClip;
    [SerializeField] private AudioSource camAudioSource; // Fuente de audio para la música de fondo
    [SerializeField] private Burro burro;
    [SerializeField] private PlayerController player; 
    [SerializeField] private PlayerInput playerInput;                                                   // Fuente de audio para la música de fondo
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
            // Calcula minutos y segundos
            int minutes = Mathf.FloorToInt(currCountdownValue / 60);
            int seconds = Mathf.FloorToInt(currCountdownValue % 60);

            // Muestra en formato MM:SS
            /*timeLeftText.text = "Tiempo de entrega: " + minutes.ToString("0") + ":" + seconds.ToString("00");*/
            timeLeftText.text =  minutes.ToString("0") + ":" + seconds.ToString("00");

            yield return new WaitForSeconds(1.0f);
            currCountdownValue--;

            if (currCountdownValue == 0 && isGameActive)
            {
                AudioManager.Instance.PlayOneShot(SoundType.GameOver);
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
    // En el script GameManager.cs
    public void StartGame()
    {
        isGameActive = true;
        timeLeftText.gameObject.SetActive(true);
        StartCoroutine("StartCountdown", 200);

        // Muestra todas las botellas de vida al iniciar
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ActualizarBotellasRotas(0, player.maxBotellasRotas);
        }
        
        // Reproduce la música del juego en bucle
        AudioManager.Instance.PlayLoop(SoundType.MusicWorld);
        AudioManager.Instance.StopLoop(SoundType.MusicMenu);
    }

    public void DetenerSonidosEnJuego()
    {
        // Detiene solo los sonidos de pasos (que son loops)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopLoop(SoundType.PlayerWalk);
            AudioManager.Instance.StopLoop(SoundType.PlayerRun);
            AudioManager.Instance.StopLoop(SoundType.DonkeyWalk);
        }
    }

    public void GanarJuego()
    {
        isGameActive = false;
        Debug.Log("🎉 Has ganado el juego.");
        
        DetenerSonidosEnJuego(); // ⬅️ ¡Llamar primero!
        AudioManager.Instance.StopLoop(SoundType.MusicWorld);

        UIManager.Instance.MostrarVictoria("¡VICTORIA!");

        AudioManager.Instance.PlayOneShot(SoundType.Victory);
        Time.timeScale = 0;
    }

    // En GameManager.cs

    public void PerderJuego()
    {
        isGameActive = false;
        Debug.Log("💀 Has perdido el juego.");
        
        DetenerSonidosEnJuego(); // ⬅️ ¡Llamar primero!
        AudioManager.Instance.StopLoop(SoundType.MusicWorld);

        UIManager.Instance.MostrarDerrota("¡DERROTA!");
        AudioManager.Instance.PlayOneShot(SoundType.GameOver);
        Time.timeScale = 0;
    }
    public void PausarJuego()
    {
        isGameActive = false;
        Debug.Log("Juego pausado.");

        DetenerSonidosEnJuego(); // ⬅️ ¡Llamar primero!
        Time.timeScale = 0;
        UIManager.Instance.MostrarPausa(true, "Juego pausado");
        playerInput.SwitchCurrentActionMap("UI");
    }

    public void Continuar()
    {
        isGameActive = true;
        Debug.Log("Continuando juego...");
        Time.timeScale = 1;
        UIManager.Instance.MostrarPausa(false, "");
        playerInput.SwitchCurrentActionMap("Player");
    }

    public void ReiniciarJuego()
    {
        Debug.Log("Reiniciando juego...");        
        AudioManager.Instance.StopLoop(SoundType.MusicMenu);
        Time.timeScale = 1; // Reanuda el tiempo antes de recargar la escena

        // Si GameManager NO es persistente, esto es suficiente:
        SceneManager.LoadScene("MainScene");
        AudioManager.Instance.PlayLoop(SoundType.MusicMenu);
    }

    public void IrAlMenuPrincipal()
    {
        Debug.Log("Volviendo al menú principal...");
        Time.timeScale = 1; // Asegúrate de reanudar el tiempo antes de cambiar de escena
        SceneManager.LoadScene("MenuPrincipal");
        AudioManager.Instance.PlayLoop(SoundType.MusicMenu);
        AudioManager.Instance.StopLoop(SoundType.MusicWorld);
        playerInput.SwitchCurrentActionMap("UI");
    }
}
